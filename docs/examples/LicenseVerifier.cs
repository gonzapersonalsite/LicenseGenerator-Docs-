using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LicenseGenerator.IntegrationExample;

public sealed record VerifiedLicense(
    string AppId,
    string RegistrationName,
    string HardwareId,
    DateTime? ExpirationDate,
    string? UserDefined,
    int? FormatVersion);

public enum LicenseInspectionFailure
{
    MalformedLicense,
    UnsupportedFormat,
    InvalidExpectedProduct,
    InvalidPublicKey,
    SignatureMismatch
}

public enum LicenseProductCheck { Match, Mismatch }
public enum LicenseHardwareCheck { NotBound, NotChecked, Match, Mismatch }
public enum LicenseExpirationCheck { NeverExpires, Current, Expired }

public abstract record LicenseInspectionResult
{
    private protected LicenseInspectionResult() { }

    public sealed record Rejected(LicenseInspectionFailure Reason) : LicenseInspectionResult;

    // Authenticated claims may still fail the caller's product, hardware or expiration policy.
    public sealed record Authenticated(
        VerifiedLicense License,
        LicenseProductCheck Product,
        LicenseHardwareCheck Hardware,
        LicenseExpirationCheck Expiration,
        DateTimeOffset CheckedAtUtc) : LicenseInspectionResult
    {
        public bool IsValid => Product == LicenseProductCheck.Match &&
            (Hardware is LicenseHardwareCheck.NotBound or LicenseHardwareCheck.Match) &&
            (Expiration is LicenseExpirationCheck.NeverExpires or LicenseExpirationCheck.Current);
    }
}

/// <summary>Pure .NET 8 reference verifier. The caller supplies the trusted key, product, hardware ID and clock.</summary>
public static class LicenseVerifier
{
    public const int MaximumEncodedLength = 131072;
    public const int MaximumPublicKeyLength = 65536;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool TryVerify(
        string encodedLicense,
        string publicKeyPem,
        string expectedAppId,
        string expectedHardwareId,
        DateTimeOffset nowUtc,
        out VerifiedLicense? license)
    {
        license = null;
        if (expectedHardwareId is null) return false;
        var result = Inspect(encodedLicense, publicKeyPem, expectedAppId, expectedHardwareId, nowUtc);
        if (result is not LicenseInspectionResult.Authenticated { IsValid: true } authenticated) return false;
        license = authenticated.License;
        return true;
    }

    /// <summary>Authenticates first, then reports policy checks. A null expected HWID explicitly skips that check.</summary>
    public static LicenseInspectionResult Inspect(
        string encodedLicense,
        string publicKeyPem,
        string expectedAppId,
        string? expectedHardwareId,
        DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(expectedAppId))
            return Reject(LicenseInspectionFailure.InvalidExpectedProduct);
        if (string.IsNullOrWhiteSpace(publicKeyPem) || publicKeyPem.Length > MaximumPublicKeyLength)
            return Reject(LicenseInspectionFailure.InvalidPublicKey);

        var failure = ReadEnvelope(encodedLicense, out var license, out var signedBytes, out var signatureBytes);
        if (failure != null) return Reject(failure.Value);

        using var rsa = RSA.Create();
        try { rsa.ImportFromPem(publicKeyPem); }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException)
        {
            return Reject(LicenseInspectionFailure.InvalidPublicKey);
        }
        try
        {
            if (!rsa.VerifyData(signedBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                return Reject(LicenseInspectionFailure.SignatureMismatch);
        }
        catch (CryptographicException) { return Reject(LicenseInspectionFailure.SignatureMismatch); }

        var authenticated = license!;
        var product = string.Equals(authenticated.AppId, expectedAppId, StringComparison.Ordinal)
            ? LicenseProductCheck.Match : LicenseProductCheck.Mismatch;
        var hardware = authenticated.HardwareId.Length == 0 ? LicenseHardwareCheck.NotBound
            : expectedHardwareId is null ? LicenseHardwareCheck.NotChecked
            : string.Equals(authenticated.HardwareId, expectedHardwareId, StringComparison.Ordinal)
                ? LicenseHardwareCheck.Match : LicenseHardwareCheck.Mismatch;
        var expiry = authenticated.ExpirationDate is null ? LicenseExpirationCheck.NeverExpires
            : nowUtc >= new DateTimeOffset(authenticated.ExpirationDate.Value) ? LicenseExpirationCheck.Expired : LicenseExpirationCheck.Current;
        return new LicenseInspectionResult.Authenticated(authenticated, product, hardware, expiry, nowUtc.ToUniversalTime());
    }

    /// <summary>Checks envelope syntax and limits before key I/O. Success is not authentication and exposes no claims.</summary>
    public static LicenseInspectionFailure? CheckFormat(string encodedLicense) =>
        ReadEnvelope(encodedLicense, out _, out _, out _);

    private static LicenseInspectionFailure? ReadEnvelope(string encodedLicense,
        out VerifiedLicense? license, out byte[] signedBytes, out byte[] signatureBytes)
    {
        license = null;
        signedBytes = [];
        signatureBytes = [];
        if (string.IsNullOrWhiteSpace(encodedLicense) || encodedLicense.Length > MaximumEncodedLength)
            return LicenseInspectionFailure.MalformedLicense;

        try
        {
            using var document = JsonDocument.Parse(Convert.FromBase64String(encodedLicense),
                new JsonDocumentOptions { MaxDepth = 8 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return LicenseInspectionFailure.MalformedLicense;
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject())
                if (!names.Add(property.Name)) return LicenseInspectionFailure.MalformedLicense;

            if (!ReadRequiredString(root, "AppId", out var appId) ||
                !ReadRequiredString(root, "RegistrationName", out var registrationName) ||
                !ReadRequiredString(root, "HardwareId", out var hardwareId, allowBlank: true) ||
                !ReadRequiredString(root, "Signature", out var signature) ||
                !ReadOptionalString(root, "UserDefined", out var userDefined))
                return LicenseInspectionFailure.MalformedLicense;
            if (appId.Length > 128 || StrictUtf8.GetByteCount(appId) > 255 ||
                registrationName.Length > 1024 || hardwareId.Length > 1024 ||
                userDefined?.Length > 8192) return LicenseInspectionFailure.MalformedLicense;

            int? formatVersion = null;
            if (root.TryGetProperty("FormatVersion", out var version) && version.ValueKind != JsonValueKind.Null)
            {
                if (!version.TryGetInt32(out var number)) return LicenseInspectionFailure.MalformedLicense;
                if (number is not (1 or 2)) return LicenseInspectionFailure.UnsupportedFormat;
                formatVersion = number;
            }

            DateTime? expirationDate = null;
            if (root.TryGetProperty("ExpirationDate", out var expiration) && expiration.ValueKind != JsonValueKind.Null)
            {
                if (expiration.ValueKind != JsonValueKind.String || !expiration.TryGetDateTime(out _))
                    return LicenseInspectionFailure.MalformedLicense;
                var text = expiration.GetString()!;
                // The signed contract contains a calendar date, never a timezone conversion or time of day.
                if (text.Length < 10 || !DateTime.TryParseExact(text[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date)) return LicenseInspectionFailure.MalformedLicense;
                expirationDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            var dateText = expirationDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "NEVER";
            string signedText;
            if (formatVersion is null or 1)
            {
                if (!string.IsNullOrEmpty(userDefined) || appId.Contains('|') ||
                    registrationName.Contains('|') || hardwareId.Contains('|')) return LicenseInspectionFailure.MalformedLicense;
                signedText = string.Join('|', appId, registrationName, hardwareId, dateText);
            }
            else
            {
                if (string.IsNullOrEmpty(userDefined)) return LicenseInspectionFailure.MalformedLicense;
                var fields = new[] { appId, registrationName, hardwareId, dateText, userDefined };
                signedText = "LG2|" + string.Join('|', fields.Select(field => Convert.ToBase64String(StrictUtf8.GetBytes(field))));
            }

            signatureBytes = Convert.FromBase64String(signature);
            signedBytes = StrictUtf8.GetBytes(signedText);
            license = new VerifiedLicense(appId, registrationName, hardwareId, expirationDate,
                string.IsNullOrEmpty(userDefined) ? null : userDefined, formatVersion);
            return null;
        }
        catch (Exception exception) when (exception is FormatException or JsonException or CryptographicException or
                                          ArgumentException or InvalidOperationException)
        {
            return LicenseInspectionFailure.MalformedLicense;
        }
    }

    private static LicenseInspectionResult.Rejected Reject(LicenseInspectionFailure reason) => new(reason);

    private static bool ReadRequiredString(JsonElement root, string name, out string value, bool allowBlank = false)
    {
        value = string.Empty;
        if (!root.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String) return false;
        value = property.GetString()!;
        return allowBlank || !string.IsNullOrWhiteSpace(value);
    }

    private static bool ReadOptionalString(JsonElement root, string name, out string? value)
    {
        value = null;
        if (!root.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null) return true;
        if (property.ValueKind != JsonValueKind.String) return false;
        value = property.GetString();
        return true;
    }
}

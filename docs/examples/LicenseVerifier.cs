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

/// <summary>Pure .NET 8 reference verifier. The caller supplies the trusted key, product, hardware ID and clock.</summary>
public static class LicenseVerifier
{
    private const int MaximumEncodedLength = 131072;
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
        if (string.IsNullOrWhiteSpace(encodedLicense) || encodedLicense.Length > MaximumEncodedLength ||
            string.IsNullOrWhiteSpace(publicKeyPem) || string.IsNullOrWhiteSpace(expectedAppId) ||
            expectedHardwareId is null)
            return false;

        try
        {
            using var document = JsonDocument.Parse(Convert.FromBase64String(encodedLicense),
                new JsonDocumentOptions { MaxDepth = 8 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject())
                if (!names.Add(property.Name)) return false;

            if (!ReadRequiredString(root, "AppId", out var appId) ||
                !ReadRequiredString(root, "RegistrationName", out var registrationName) ||
                !ReadRequiredString(root, "HardwareId", out var hardwareId, allowBlank: true) ||
                !ReadRequiredString(root, "Signature", out var signature) ||
                !ReadOptionalString(root, "UserDefined", out var userDefined))
                return false;
            if (appId.Length > 128 || StrictUtf8.GetByteCount(appId) > 255 ||
                registrationName.Length > 1024 || hardwareId.Length > 1024 ||
                userDefined?.Length > 8192) return false;

            int? formatVersion = null;
            if (root.TryGetProperty("FormatVersion", out var version) && version.ValueKind != JsonValueKind.Null)
            {
                if (!version.TryGetInt32(out var number) || number is not (1 or 2)) return false;
                formatVersion = number;
            }

            DateTime? expirationDate = null;
            if (root.TryGetProperty("ExpirationDate", out var expiration) && expiration.ValueKind != JsonValueKind.Null)
            {
                if (expiration.ValueKind != JsonValueKind.String || !expiration.TryGetDateTime(out _)) return false;
                var text = expiration.GetString()!;
                // The signed contract contains a calendar date, never a timezone conversion or time of day.
                if (text.Length < 10 || !DateTime.TryParseExact(text[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date)) return false;
                expirationDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            var dateText = expirationDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "NEVER";
            string signedText;
            if (formatVersion is null or 1)
            {
                if (!string.IsNullOrEmpty(userDefined) || appId.Contains('|') ||
                    registrationName.Contains('|') || hardwareId.Contains('|')) return false;
                signedText = string.Join('|', appId, registrationName, hardwareId, dateText);
            }
            else
            {
                if (string.IsNullOrEmpty(userDefined)) return false;
                var fields = new[] { appId, registrationName, hardwareId, dateText, userDefined };
                signedText = "LG2|" + string.Join('|', fields.Select(field => Convert.ToBase64String(StrictUtf8.GetBytes(field))));
            }

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            if (!rsa.VerifyData(StrictUtf8.GetBytes(signedText), Convert.FromBase64String(signature),
                    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)) return false;
            if (!string.Equals(appId, expectedAppId, StringComparison.Ordinal) ||
                (hardwareId.Length > 0 && !string.Equals(hardwareId, expectedHardwareId, StringComparison.Ordinal)) ||
                (expirationDate.HasValue && nowUtc >= new DateTimeOffset(expirationDate.Value))) return false;

            license = new VerifiedLicense(appId, registrationName, hardwareId, expirationDate,
                string.IsNullOrEmpty(userDefined) ? null : userDefined, formatVersion);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or JsonException or CryptographicException or
                                          ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

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

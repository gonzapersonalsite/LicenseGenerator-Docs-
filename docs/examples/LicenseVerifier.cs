using System;
using System.IO;
using System.Security.Cryptography;
using LicenseGenerator.Protocol;
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
    public const int MaximumEncodedLength = LicenseProtocol.MaximumEncodedLength;
    public const int MaximumPublicKeyLength = LicensePublicKey.MaximumPemLength;

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

        RSA rsa;
        try { rsa = LicensePublicKey.Read(publicKeyPem); }
        catch (Exception exception) when (exception is ArgumentException or CryptographicException or InvalidDataException)
        {
            return Reject(LicenseInspectionFailure.InvalidPublicKey);
        }
        using (rsa)
        {
            try
            {
                if (!rsa.VerifyData(signedBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    return Reject(LicenseInspectionFailure.SignatureMismatch);
            }
            catch (CryptographicException) { return Reject(LicenseInspectionFailure.SignatureMismatch); }
        }

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
        try
        {
            var envelope = LicenseProtocol.ReadEncoded(encodedLicense);
            signedBytes = LicenseProtocol.GetSignedBytes(envelope);
            signatureBytes = Convert.FromBase64String(envelope.Signature);
            license = new VerifiedLicense(envelope.AppId, envelope.RegistrationName, envelope.HardwareId,
                envelope.ExpirationDate, envelope.UserDefined, envelope.FormatVersion);
            return null;
        }
        catch (NotSupportedException) { return LicenseInspectionFailure.UnsupportedFormat; }
        catch (Exception exception) when (exception is FormatException or JsonException or CryptographicException or
                                          ArgumentException or InvalidOperationException)
        {
            return LicenseInspectionFailure.MalformedLicense;
        }
    }

    private static LicenseInspectionResult.Rejected Reject(LicenseInspectionFailure reason) => new(reason);
}

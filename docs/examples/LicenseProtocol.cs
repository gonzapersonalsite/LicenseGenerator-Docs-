// Copyright (c) 2026 Gonzalo Martínez García.
// License Generator integration example: you may use, copy, modify and distribute this file
// under section 2 of the License Generator EULA, keeping this notice.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LicenseGenerator.Protocol;

/// <summary>Parsed claims are untrusted until their signature has been verified with a trusted public key.</summary>
public sealed record LicenseEnvelope(string AppId, string RegistrationName, string HardwareId,
    DateTime? ExpirationDate, string? UserDefined, int? FormatVersion, string Signature);

/// <summary>The shared, I/O-free wire contract used by issuers, storage readers and verifiers.</summary>
public static class LicenseProtocol
{
    public const int MaxAppIdLength = 128;
    public const int MaxAppIdUtf8Bytes = 255;
    public const int MaxRegistrationNameLength = 1024;
    public const int MaxHardwareIdLength = 1024;
    public const int MaxUserDefinedLength = 8192;
    public const int MaximumEncodedLength = 131072;
    public const int MaximumJsonBytes = MaximumEncodedLength / 4 * 3;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static string GetDataToSign(string appId, string registrationName, string hardwareId,
        DateTime? expirationDate, string? userDefined, int? formatVersion)
    {
        ValidateAppIdentifier(appId);
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationName);
        ArgumentNullException.ThrowIfNull(hardwareId);
        ValidateText(registrationName, MaxRegistrationNameLength, nameof(registrationName));
        ValidateText(hardwareId, MaxHardwareIdLength, nameof(hardwareId));
        userDefined = string.IsNullOrEmpty(userDefined) ? null : userDefined;
        if (userDefined != null) ValidateText(userDefined, MaxUserDefinedLength, nameof(userDefined));
        var date = expirationDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "NEVER";
        switch (formatVersion)
        {
            case null:
            case 1:
                if (userDefined != null) throw new InvalidOperationException("Custom license data requires format version 2.");
                if (appId.Contains('|') || registrationName.Contains('|') || hardwareId.Contains('|'))
                    throw new ArgumentException("Legacy license fields cannot contain the '|' separator.");
                return $"{appId}|{registrationName}|{hardwareId}|{date}";
            case 2:
                if (userDefined == null) throw new InvalidOperationException("Format version 2 requires custom license data.");
                return "LG2|" + string.Join('|', Encode(appId), Encode(registrationName), Encode(hardwareId), Encode(date), Encode(userDefined));
            default:
                throw new NotSupportedException("The license format version is not supported.");
        }
    }

    public static byte[] GetSignedBytes(LicenseEnvelope license) => StrictUtf8.GetBytes(GetDataToSign(
        license.AppId, license.RegistrationName, license.HardwareId, license.ExpirationDate, license.UserDefined, license.FormatVersion));

    public static LicenseEnvelope ReadEncoded(string encodedLicense)
    {
        if (string.IsNullOrWhiteSpace(encodedLicense) || encodedLicense.Length > MaximumEncodedLength)
            throw new JsonException("The license envelope is empty or exceeds the supported size.");
        return ReadJson(Convert.FromBase64String(encodedLicense));
    }

    public static LicenseEnvelope ReadJson(ReadOnlyMemory<byte> json)
    {
        if (json.Length > MaximumJsonBytes) throw new JsonException("The license JSON exceeds the supported size.");
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 });
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) throw new JsonException("A license must be a JSON object.");
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
            if (!names.Add(property.Name)) throw new JsonException("Duplicate license properties are not supported.");

        var appId = ReadString(root, "AppId");
        var registrationName = ReadString(root, "RegistrationName");
        var hardwareId = ReadString(root, "HardwareId", BlankTextPolicy.Allow);
        var signature = ReadString(root, "Signature");
        string? userDefined = null;
        if (root.TryGetProperty("UserDefined", out var custom) && custom.ValueKind != JsonValueKind.Null)
        {
            if (custom.ValueKind != JsonValueKind.String) throw new JsonException("UserDefined must be a string or null.");
            userDefined = custom.GetString();
            if (userDefined!.Length == 0) userDefined = null;
        }

        int? formatVersion = null;
        if (root.TryGetProperty("FormatVersion", out var version) && version.ValueKind != JsonValueKind.Null)
        {
            if (version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var number))
                throw new JsonException("FormatVersion must be an integer.");
            if (number is not (1 or 2)) throw new NotSupportedException("The license format version is not supported.");
            formatVersion = number;
        }
        DateTime? expirationDate = null;
        if (root.TryGetProperty("ExpirationDate", out var expiration) && !TryReadExpirationDate(expiration, out expirationDate))
            throw new JsonException("The license expiration must be a valid signed calendar date.");
        var license = new LicenseEnvelope(appId, registrationName, hardwareId, expirationDate, userDefined, formatVersion, signature);
        GetSignedBytes(license);
        if (Convert.FromBase64String(signature).Length == 0) throw new FormatException("A license signature cannot be empty.");
        return license;
    }

    public static bool TryReadExpirationDate(JsonElement expiration, out DateTime? expirationDate)
    {
        expirationDate = null;
        if (expiration.ValueKind == JsonValueKind.Null) return true;
        if (expiration.ValueKind != JsonValueKind.String || !expiration.TryGetDateTime(out _)) return false;
        var text = expiration.GetString()!;
        // Only the calendar date is signed; a timezone or time of day cannot extend it.
        if (text.Length < 10 || !DateTime.TryParseExact(text[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date)) return false;
        expirationDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        return true;
    }

    public static void ValidateAppIdentifier(string appId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);
        ValidateText(appId, MaxAppIdLength, nameof(appId));
        if (StrictUtf8.GetByteCount(appId) > MaxAppIdUtf8Bytes)
            throw new ArgumentException("Application IDs cannot exceed 255 UTF-8 bytes.", nameof(appId));
    }

    public static void ValidateText(string value, int maximumLength, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        if (value.Length > maximumLength)
            throw new ArgumentException($"The field cannot exceed {maximumLength} UTF-16 code units.", parameterName);
        StrictUtf8.GetByteCount(value);
    }

    private enum BlankTextPolicy { Reject, Allow }

    private static string ReadString(JsonElement root, string name, BlankTextPolicy blankPolicy = BlankTextPolicy.Reject)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
            throw new JsonException($"The license requires a string {name}.");
        var text = value.GetString()!;
        return blankPolicy switch
        {
            BlankTextPolicy.Reject when string.IsNullOrWhiteSpace(text) =>
                throw new JsonException($"The license requires a nonempty {name}."),
            BlankTextPolicy.Reject or BlankTextPolicy.Allow => text,
            _ => throw new ArgumentOutOfRangeException(nameof(blankPolicy), blankPolicy, "Unsupported blank-text policy.")
        };
    }

    private static string Encode(string value) => Convert.ToBase64String(StrictUtf8.GetBytes(value));
}

/// <summary>Reads only public RSA material; callers remain responsible for trusting its source.</summary>
public static class LicensePublicKey
{
    public const int MaximumPemLength = 65536;

    public static RSA Read(string pem)
    {
        ArgumentNullException.ThrowIfNull(pem);
        if (pem.Length > MaximumPemLength) throw new InvalidDataException("The RSA public key exceeds the supported size.");
        var text = pem.AsSpan().Trim();
        if (!PemEncoding.TryFind(text, out var fields) ||
            fields.Location.Start.GetOffset(text.Length) != 0 || fields.Location.End.GetOffset(text.Length) != text.Length)
            throw new InvalidDataException("A public key must contain exactly one public PEM block.");
        var label = text[fields.Label];
        if (!label.SequenceEqual("PUBLIC KEY") && !label.SequenceEqual("RSA PUBLIC KEY"))
            throw new InvalidDataException("Private key material is not accepted as a public key.");
        var data = Convert.FromBase64String(text[fields.Base64Data].ToString());
        var key = RSA.Create();
        try
        {
            int bytesRead;
            if (label.SequenceEqual("PUBLIC KEY")) key.ImportSubjectPublicKeyInfo(data, out bytesRead);
            else key.ImportRSAPublicKey(data, out bytesRead);
            if (bytesRead != data.Length || key.KeySize < 2048)
                throw new InvalidDataException("The RSA public key is invalid or smaller than 2048 bits.");
            return key;
        }
        catch { key.Dispose(); throw; }
    }
}

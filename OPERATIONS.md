# Operations and integration guide

[Español](docs/es/OPERATIONS.md) · **English** · [Deutsch](docs/de/OPERATIONS.md) · [Português](docs/pt/OPERATIONS.md) · [中文](docs/zh/OPERATIONS.md)

## Create a product and issue a license

Create a product in App Management. The generator stores a separate RSA key pair under `<LocalApplicationData>/LicenseGenerator/Keys/<AppId>/`. Your client application needs `public.pem`; never distribute `private.pem`.

For hardware binding, implement hardware identification in your client, show that exact value to its user, and accept it in the generator's HWID field. The generator does not choose the hardware-identification algorithm. Avoid truncated identifiers and shared values such as `UNKNOWN-HWID`; report identification failures instead of issuing a bound license for a generic value. An intentionally empty HWID produces a license without hardware binding.

Select the product, enter the registration name and optional HWID, and optionally select an expiration date. Expiration occurs at **00:00 UTC at the start of the selected date**. To allow all of June 10, choose June 11. Leave the date empty for a perpetual license.

Generate, then copy the resulting code to the customer. History records issued licenses. Deleting a history entry only removes the local record; deleting a product removes its keys and prevents issuing further licenses with them. Neither operation revokes previously delivered licenses. Back up keys before deletion.

## Signed custom data: UserDefined

Leave UserDefined empty to generate the existing legacy format. No new JSON properties are emitted in this case.

For a client that implements format 2, enter plain text such as:

```text
features=export,reports;plan=professional
```

The exact text is included in the signature. The generator assigns no meaning to it. Your client can reject an unsupported schema, check a feature list or apply its own policy **after cryptographic verification**. Do not put secrets in this field: Base64 is encoding, not encryption.

Any nonempty custom text uses format 2 and requires updated client verification. Do not append it to the old signing string or use an unchecked JSON field. The [canonical contract](ARCHITECTURE.md) defines every signed byte and all compatibility rules.

## Inspect a license

Open **Inspect license** and paste a complete license code, or open a saved entry from History. Inspection does not modify the saved record. Explicitly select a local product's public key or paste a public PEM key obtained from a trusted source. The license payload never selects a trusted key. Private PEM keys are rejected by this screen.

Provide the expected product ID and, optionally, the exact hardware ID from the client. An empty expected HWID means **not checked** for a hardware-bound license; it does not confirm that the license is usable on this device. An unbound license needs no hardware check.

The screen authenticates the RSA signature before showing any license fields, then reports product, hardware and expiration checks separately. Authentic expired licenses can be inspected, but remain expired. A signature mismatch can mean a wrong key or changed license data; it cannot distinguish those causes. The result shows the UTC time of the check. Editing inputs clears the result; verify again for a later time. Nothing is uploaded or saved by inspection.

## Client integration

Copy both [LicenseVerifier.cs](docs/examples/LicenseVerifier.cs) and [LicenseProtocol.cs](docs/examples/LicenseProtocol.cs) into your client project. Together they form the dependency-free executable reference compiled into the regression suite. Its boundary has no disk, network, global clock or hardware lookup: your application supplies those values explicitly.

The [EULA's integration example exception](LICENSE) permits using, copying, modifying and distributing these two source files in source or compiled form as part of commercial or free license-verifying clients, with the copyright notice retained. This permission does not extend to the License Generator application source.

```csharp
using LicenseGenerator.IntegrationExample;

bool valid = LicenseVerifier.TryVerify(
    encodedLicense: codeFromCustomer,
    publicKeyPem: embeddedPublicKey,
    expectedAppId: "MyProduct",
    expectedHardwareId: hardwareIdFromYourApplication,
    nowUtc: DateTimeOffset.UtcNow,
    out var verified);

if (valid)
{
    // Apply your own UserDefined policy using verified!.UserDefined.
}
```

The snippet assumes your application supplies its code, public key and hardware ID. Failed verification returns false and a null result. Verify before persisting an activation, then verify again when loading it and when checking time-limited access. Store the original license code. Do not cache a boolean indefinitely.

`TryVerify` keeps its existing acceptance contract. For diagnostics, `LicenseVerifier.Inspect` returns either `Rejected` with a reason and no claims, or `Authenticated` with signed fields and separate policy results. Pass `null` as the expected HWID only when deliberately skipping that check; a bound license then has `Hardware = NotChecked` and `IsValid = false`. `CheckFormat` only checks envelope syntax and limits before key access; success is not authentication. These APIs share one parser with the desktop inspector.

For Python, JavaScript or another language, implement the same [transport and signing contract](ARCHITECTURE.md) using a maintained RSA library. Keep the signed date as the literal Gregorian calendar date; JavaScript's `new Date(...).toISOString()` can shift that date and is unsuitable for reconstructing it. Validate both legacy and format-2 fixtures, tampering, Unicode, separators, wrong products, wrong hardware IDs and the exact UTC expiration boundary before release.

## Back up or migrate

Use Settings to export a ZIP backup. It contains private keys, history and settings without encryption. Keep it in storage you control; encrypt it with your chosen backup solution before sharing or uploading it.

Import merges products and history. Existing keys/history are preserved. If an incoming product has different keys or a history filename has different contents, import fails instead of overwriting it. Valid imported settings replace the current settings. Archive structure, size and RSA key pairs are validated before importing. Export a backup of the current profile first if you want to restore its previous settings later.

Windows and Linux downloads are self-contained applications, not portable-data installations. The data directory belongs to the operating-system user. Copying only the executable to another machine does not move the keys or history; use backup/import.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Signature rejected | Correct public key, exact field values, invariant date, supported format and PKCS#1 v1.5/SHA-256. |
| Older client rejects a new license | Leave UserDefined empty until that client supports format 2. |
| Expired unexpectedly | The selected date is the exclusive cutoff at 00:00 UTC; it is not the last permitted day. |
| Wrong-machine rejection | Use the exact same deterministic HWID representation on generation and verification. |
| Import reports a conflict | Keep the current data and inspect the backup; do not replace a product's private key accidentally. |
| Data absent after moving the executable | Data is in the current user's application-data folder; import the backup on the new profile. |

Offline issuance does not provide remote revocation, a trusted clock or protection against modified client code. See [security boundaries](ARCHITECTURE.md#security-boundaries) before choosing an integration policy.

[Report a problem](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Support development](https://ko-fi.com/gonzaloapps)

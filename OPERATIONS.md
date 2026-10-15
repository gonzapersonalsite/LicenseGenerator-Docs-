# Operations guide

[Español](docs/es/OPERATIONS.md) · **English** · [Deutsch](docs/de/OPERATIONS.md) · [Português](docs/pt/OPERATIONS.md) · [中文](docs/zh/OPERATIONS.md)

This guide covers using the License Generator application: creating products,
issuing and inspecting licenses, and keeping your data safe. Verifying licenses
in your own application is covered by the [integration guide](INTEGRATION.md),
and the exact license format by the [format and security contract](ARCHITECTURE.md).

## Create a product and issue a license

Create a product under **Applications**. The generator keeps a separate RSA key pair for it in its data folder. Your client application needs the product's `public.pem`; never distribute `private.pem`.

For hardware binding, your client computes a hardware identifier, shows that exact value to its user, and you enter it in the generator's HWID field. The generator does not choose the hardware-identification method; the [integration guide](INTEGRATION.md#choose-a-hardware-identifier) describes what a good one needs. An intentionally empty HWID produces a license without hardware binding.

Select the product, enter the registration name and optional HWID, and optionally select an expiration date. Expiration occurs at **00:00 UTC at the start of the selected date**. To allow all of June 10, choose June 11. Leave the date empty for a perpetual license.

Generate, then copy the resulting code to the customer. History records issued licenses. Deleting a history entry only removes the local record; deleting a product removes its keys and prevents issuing further licenses with them. Neither operation revokes previously delivered licenses. Back up keys before deletion.

## Signed custom data: UserDefined

Leave UserDefined empty to generate the existing legacy format. No new JSON properties are emitted in this case.

For a client that implements format 2, enter plain text such as:

```text
features=export,reports;plan=professional
```

The exact text is included in the signature. The generator assigns no meaning to it; your client decides what it means **after cryptographic verification** ([integration guide](INTEGRATION.md#use-userdefined)). Do not put secrets in this field: Base64 is encoding, not encryption.

Any nonempty custom text uses format 2, which a client written for the legacy format rejects. Update every client that must accept the license first.

## Inspect a license

Open **Inspect license** and paste a complete license code, or open a saved entry from History. Inspection does not modify the saved record. Explicitly select a local product's public key or paste a public PEM key obtained from a trusted source. The license payload never selects a trusted key. Private PEM keys are rejected by this screen.

Provide the expected product ID and, optionally, the exact hardware ID from the client. An empty expected HWID means **not checked** for a hardware-bound license; it does not confirm that the license is usable on this device. An unbound license needs no hardware check.

The screen authenticates the RSA signature before showing any license fields, then reports product, hardware and expiration checks separately. Authentic expired licenses can be inspected, but remain expired. A signature mismatch can mean a wrong key or changed license data; it cannot distinguish those causes. The result shows the UTC time of the check. Editing inputs clears the result; verify again for a later time. Nothing is uploaded or saved by inspection.

## Your data

The application keeps signing keys, license history, settings and a diagnostic log in a data folder that belongs to your operating-system user, separate from the executable. The [privacy policy](https://gonzaloapps.es/apps/license-generator/legal/privacy-policy/) lists what each file holds and where the folder is for the Microsoft Store, portable Windows and Linux versions. Export a backup before uninstalling the Microsoft Store version: Windows removes its data folder with it.

Private keys and backups are **not encrypted**. Anyone who obtains a private key can issue licenses that your applications accept.

## Back up or migrate

Use Settings to export a ZIP backup. It contains private keys, history and settings without encryption. Keep it in storage you control; encrypt it with your chosen backup solution before sharing or uploading it.

Import merges products and history. Existing keys/history are preserved. If an incoming product has different keys or a history filename has different contents, import fails instead of overwriting it. Valid imported settings replace the current settings. Archive structure, size and RSA key pairs are validated before importing. Export a backup of the current profile first if you want to restore its previous settings later. An import does not revoke or replace licenses already delivered to customers.

Copying only the executable to another machine does not move the keys or history, and switching between the Store and a portable version starts from a different folder: export a backup, then import it in the new installation.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| A client rejects a license | Follow the client checks in the [integration guide](INTEGRATION.md#troubleshooting); inspect the license here with the same public key, product and HWID. |
| Expired unexpectedly | The selected date is the exclusive cutoff at 00:00 UTC; it is not the last permitted day. |
| Import reports a conflict | Keep the current data and inspect the backup; do not replace a product's private key accidentally. |
| Data absent after moving the executable or reinstalling | Data is in the current user's data folder for that version; import the backup in the new installation. |

Offline issuance does not provide remote revocation, a trusted clock or protection against modified client code. See [security boundaries](ARCHITECTURE.md#security-boundaries) before choosing an integration policy.

[Report a problem](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Support development](https://ko-fi.com/gonzaloapps)

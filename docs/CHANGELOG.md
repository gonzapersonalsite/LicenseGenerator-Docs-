# Changelog

## 1.2.4 — 2026-10-15

- Name every concept in the interface with the term the manuals use, in all five
  languages. The generator's field for the customer's identifier is now
  **Hardware ID (Optional)**, and the inspection results speak of the hardware ID
  rather than a device ID. Spanish now says *caducidad* and Portuguese
  *expiração* for expiration, German *Hardwarekennung* for the hardware ID, and
  Chinese 到期 and 吊销 for expiration and revocation.
- Screen readers announce the date picker, the history table's rows and cells and
  the application and notification lists by name, read notifications aloud when
  they appear, and no longer count the navigation's section headings as pages.
- Generating a license from a product's card selects that product on the first
  visit to the generator too.
- On Linux, the application starts when the user's `~/.local/share` folder does
  not exist yet, and confirmation dialogs take the keyboard focus, so Escape
  closes them.
- The backup file filter is named in the interface language.

Nothing else changes: keys, licenses, history, settings, license formats and the
integration examples are the same as in 1.2.3.

## Integration examples — 2026-10-15

- Added the [integration guide](../INTEGRATION.md): an activation screen, a check
  at every start, how to choose and derive a hardware identifier, revocation and
  key changes, and pitfalls on other stacks, in all five languages.
- Added Python and Node.js verifiers with their tests beside the C# pair, and
  published test vectors that all three pass.
- Every verifier now returns a revocation identifier. Revoke licenses by it, never
  by their `Signature` text. In the C# example it is the new `RevocationId` of
  `VerifiedLicense`; code that only reads the record keeps working.
- The [contract](../ARCHITECTURE.md) now states the order of every check, which
  whitespace each part of a license and of a key accepts, and that every length
  counts UTF-16 code units, and the test vectors pin each of these rules.

The application itself is unchanged since 1.2.3.

## 1.2.3 — 2026-10-15

- Show the third-party notices in Settings and ship them beside the Windows and
  Linux executables and inside the Store package. They list every component the
  application distributes with its license, including the .NET runtime, Avalonia,
  SkiaSharp, HarfBuzz, the Inter typeface and the interface icons.
- Link the license agreement and privacy policy from Settings.
- Reorganize Settings into appearance, your data, help and tools, support and
  about sections, and rewrite the support section to thank users and explain
  that contributions are optional.
- Show the full copyright holder's name.

Nothing else changes: keys, licenses, history, settings, license formats and the
C# integration example are the same as in 1.2.2.

## 1.2.2 — 2026-10-15

- Fix the Microsoft Store app's taskbar icon, which Windows showed on a square in
  the system accent colour. The package resource index now names the logos as the
  manifest refers to them, so Windows finds their plate-free variants, and
  packaging refuses an index that does not.

Nothing else changes: keys, licenses, history, settings, license formats and the
C# integration example are the same as in 1.2.1.

## 1.2.1 — 2026-10-14

- Hardened public-key handling: malformed backups cannot substitute private key
  material for a public key. Completed backups are validated before replacing an
  existing export.
- Fix the unresponsive Windows window after backup import by waiting for the
  confirmation dialog's owner cleanup before opening the file picker.
- Preserve signed expiration dates when copying history and restoring licenses
  containing timezone offsets.
- Prevent delayed navigation and inspection operations from overwriting newer
  input or releasing application resources before work finishes.
- Improve recovery from failed language, appearance and settings changes; refresh
  existing notification colors when the theme changes.
- Use a stable application identity for shortcuts created by the packaged Windows
  app, and reduce repeated directory scans during backup validation.

Existing license formats and signing keys remain compatible. Update both
`LicenseVerifier.cs` and `LicenseProtocol.cs` when replacing the C# integration
example; the existing `TryVerify` API remains available.

## 1.2.0 — 2026-10-14

- Moved to .NET 10 LTS, Avalonia 11.3.22 and DataGrid 11.3.13.
- Added a local license inspector with an explicitly trusted public key, authenticated
  fields and separate product, hardware and expiration results. The desktop app and
  standalone client example share one verifier; existing `TryVerify` callers remain compatible.
- Redesigned navigation, forms, dashboard, history and settings around adaptive
  layouts, theme-aware surfaces, accessible actions and actionable empty states.
- Moved license-store loads, key generation and license issuance off the UI thread;
  prevented duplicate operations and stale results while retaining drafts between pages.
- Consolidated workspace reads, surfaced unreadable history entries and avoided
  repeated filtering during pagination.
- Added field-level input feedback, owned confirmation dialogs, persistent error
  notifications and localized date presentation across all five interface languages.
- Removed the artificial two-second startup delay. Existing keys, stored licenses
  and signed formats remain compatible.
- Kept initial required fields quiet until edited, localized date placeholders and
  corrected accessible application and settings labels.
- Wait for startup and preference writes to finish before the window closes, so
  closing never interrupts them.

## 1.1.0 — 2026-10-14

- Added optional signed `UserDefined` text for application-defined claims. Nonempty text uses format 2; empty text preserves the legacy JSON and signature format. Update client verification before enabling custom claims.
- Added a dependency-free reference C# verifier.
- Made product identifiers portable and bounded, rejected ambiguous legacy separators, and prevented rapid license generation from overwriting history.
- Hardened backup validation and made imports preserve existing signing keys and history. Conflicting data is rejected before publication; settings are replaced atomically and applied without restarting.
- Added atomic settings persistence, safe recovery of invalid settings, startup diagnostics and exclusion between instances using the same data directory.
- Corrected stale generated-license output, pagination after deletion, navigation state, error feedback and Linux shortcut escaping.
- Clarified expiration at the beginning of the selected date in UTC and displayed signed custom data in searchable history.
- Corrected the donation link to https://ko-fi.com/gonzaloapps and revised all five documentation languages.
- Removed unsupported security claims and clarified that local private keys and ZIP backups are not encrypted.
- Corrected Store icon packaging with transparent backgrounds and indexed taskbar resources for light and dark themes.
- Updated Tmds.DBus.Protocol to 0.21.3 for CVE-2026-39959. Removed unused SVG and obfuscation dependencies.

Windows and Linux x64 downloads include the .NET runtime. Their data remains in the current user's application-data directory; transfer it using backup/import.

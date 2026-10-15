# Changelog

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
- Separate the pure Core from desktop/storage adapters, consolidate protocol
  parsing and enforce architecture, test-harness and documentation boundaries.
- Adopt the shared contribution scaffold and one local/CI verification workflow;
  harden process/container cleanup and package the verified version/icon snapshot.

Existing license formats and signing keys remain compatible. Update both
`LicenseVerifier.cs` and `LicenseProtocol.cs` when replacing the C# integration
example; the existing `TryVerify` API remains available.

## 1.2.0 — 2026-10-14

- Migrated to .NET 10 LTS with SDK 10.0.402, Avalonia 11.3.22 and DataGrid 11.3.13;
  pinned the official SDK container by digest and refreshed the dependency locks.
  Package archives, signatures and build imports were reviewed before compilation.
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
- Wait for startup and preference writes before closing, with bounded shutdown
  handling that retains the profile lease while work is still active.

## 1.1.0 — 2026-10-14

- Added optional signed `UserDefined` text for application-defined claims. Nonempty text uses format 2; empty text preserves the legacy JSON and signature format. Update client verification before enabling custom claims.
- Added a dependency-free reference C# verifier and regression coverage for compatibility, tampering, Unicode, expiration, storage failures, backups and view-model behavior.
- Made product identifiers portable and bounded, rejected ambiguous legacy separators, and prevented rapid license generation from overwriting history.
- Hardened backup validation and made imports preserve existing signing keys and history. Conflicting data is rejected before publication; settings are replaced atomically and applied without restarting.
- Added atomic settings persistence, safe recovery of invalid settings, startup diagnostics and exclusion between instances using the same data directory.
- Corrected stale generated-license output, pagination after deletion, navigation state, error feedback and Linux shortcut escaping.
- Clarified expiration at the beginning of the selected date in UTC and displayed signed custom data in searchable history.
- Corrected the donation link to https://ko-fi.com/gonzaloapps and revised all five documentation languages.
- Removed unsupported security claims and clarified that local private keys and ZIP backups are not encrypted.
- Corrected Store icon packaging with transparent backgrounds and indexed taskbar resources for light and dark themes.
- Replaced the obsolete release scripts and certificate setup with a pinned Docker/.NET toolchain, locked dependencies, bounded build processes, Windows/Linux CI and validated unsigned Store packaging.
- Updated Tmds.DBus.Protocol to 0.21.3 for CVE-2026-39959. Removed unused SVG and obfuscation dependencies.

Windows and Linux x64 downloads include the .NET runtime. Their data remains in the current user's application-data directory; transfer it using backup/import. Microsoft Store availability depends on certification of the submitted update.

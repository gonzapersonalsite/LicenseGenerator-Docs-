# Changelog

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

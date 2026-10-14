# Release procedure

The application version is defined once in `src/LicenseGenerator/LicenseGenerator.csproj`. The Store requires a four-part version with a zero final component. Keep the package identity stable across updates.

## Build artifacts

```powershell
./scripts/release.ps1 -PackageMsix
```

Or use the pre-provisioned, approved SDK Docker image while running packaging on the Windows host:

```powershell
./scripts/release.ps1 -UseDocker -PackageMsix
```

The script runs the verification gate first, then produces self-contained Windows/Linux x64 binaries in a fresh `dist/<version>-<unique-run>/` directory. To publish one platform, pass `-Runtime win-x64` or `-Runtime linux-x64`. Linux publication can run on either build host; GUI behavior must also be checked on a native Linux desktop before claiming desktop validation there.

`-PackageMsix` requires Windows and the Windows SDK MakeAppx and MakePri tools. It creates correctly sized logos from the source artwork, stages the exact Windows publish output separately, generates the manifest from the project version, runs MakeAppx validation and writes an unsigned `.msix`. A `SHA256SUMS` file identifies the binaries and package. Failed builds remain incomplete; never upload partial output.

The source PNG retains its transparency. Packaging generates target-size icons with default, `altform-unplated` and `altform-lightunplated` variants, plus larger scale variants. MakePri indexes them into `resources.pri`; the manifest keeps a transparent background. These resources prevent Windows from adding a background plate to the Store app's taskbar and Start icons. See Microsoft's [icon requirements](https://learn.microsoft.com/en-us/windows/apps/design/iconography/app-icon-construction) and [manual MSIX resource-indexing procedure](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-manual-conversion#generate-a-package-resource-index-pri-file-using-makepri). Verify the installed package's icons in light and dark themes; an unpackaged executable does not test shell resource selection.

The Microsoft Store signs packages during submission, so Store builds do not generate certificates or modify certificate trust stores. The unsigned package is not a sideloading installer. See [Microsoft's signing guide](https://learn.microsoft.com/en-us/windows/msix/package/sign-msix-package-guide).

## Store identity

Verified against Partner Center for product `9NR38VBCF8JK`:

- Name: `gonzalo.dev.LicenseGenerator`
- Publisher: `CN=1DC503D2-4E89-4B7D-91F5-07A5543E1AFC`
- Publisher display name: `gonzalo.dev`
- Package family: `gonzalo.dev.LicenseGenerator_091xbq64s670a`

The package script owns these manifest values. Review them against Partner Center before submission; changing them creates an incompatible package identity.

## Publication checklist

Use matching `LicenseGenerator-X.Y.Z.0` tag names in the two repositories, with
different targets: verified application source in the private repository and the
reviewed documentation snapshot in the public one. Never push private tags or
mirror the private repository into the public remote. Run the public repository's
`scripts/verify-public.ps1` before its signed documentation PR. Public version tags
are protected against updates and deletion; create a new tag for a new release.
GitHub's automatic source archives contain the repository at that tag, so explain
that public ZIP/TAR source downloads are documentation, not app installers.

1. Review staged, unstaged and untracked files. Exclude private keys, profile data and build caches; examine every changed file and scan for secrets/residue.
2. Run the gate on the exact release content. Run the published Windows app, issue legacy and custom-data licenses, verify them with the public example, and check history, locale changes and backup/import. Check Linux desktop behavior on Linux separately.
3. Inspect the package manifest and contents. Confirm its version exceeds the Store version and its identity matches the existing product. Preserve the checksum and validation results.
4. With owner authorization, commit/push the private application repository and synchronize public documentation without copying private source or keys. The public documentation repository contains manuals, the integration example and legal documentation.
5. Upload the verified MSIX to the existing Partner Center product, update release notes and submit it. Record the actual certification/publication status; a successful upload is not a live release.
6. Once the app is finished and published, answer the GitHub feature request with the released version, UserDefined compatibility notes and integration link. Do not close it based only on local implementation.

Store certification is an external step. Record any pending certification or required account action accurately in the release report.

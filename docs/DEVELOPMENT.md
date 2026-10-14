# Development

These instructions apply to the private application repository. The public documentation repository contains the manuals and client verification example, not the application sources or build scripts.

The application targets .NET 10 LTS, Avalonia 11.3 and CommunityToolkit.Mvvm. `global.json` fixes the SDK; the project fixes direct package versions and committed `packages.lock.json` files fix the resolved dependency graph. `nuget.config` permits only nuget.org. Do not change dependency versions as a side effect of a build.

## Verify

Install the SDK version from `global.json` and PowerShell 7 through your approved tool-installation process. From the repository root:

```powershell
./scripts/verify.ps1
```

This restores in locked mode, builds the solution in Release, and runs the dependency-free regression console. Every native process has a deadline and a nonzero exit stops the gate. Tests use temporary data and fixed clocks instead of the real application profile. The suite also compiles the public client verifier example.

For an already-approved Docker installation and locally available SDK image:

```powershell
./scripts/verify.ps1 -UseDocker
```

The exact Linux/amd64 SDK image is pinned by digest in `scripts/toolchain.json`. The script never pulls or updates it automatically. Provision that image separately through the approved process. The container runs as the non-root `app` user without capabilities or privilege escalation. It has an ephemeral filesystem, a temporary directory, the repository mount and a repository-local NuGet cache under `artifacts/`. Restore may access nuget.org; build/test run without container networking. Do not run unreviewed repository code in this environment as a substitute for reviewing it.

Use one build environment per checkout at a time. If switching between native and Docker builds, remove generated `bin`/`obj` outputs within that checkout before restoring to prevent absolute NuGet paths from the prior environment being reused.

## Structure and conventions

- `src/LicenseGenerator`: desktop app; retain the existing MVVM/service boundaries.
- `tests/LicenseGenerator.Tests`: regression executable with no additional test packages.
- `docs/examples`: standalone integration source linked into the app; tests access
  that same compiled implementation through their project reference.
- `scripts`: verification, bounded process execution and release packaging.
- `.github/workflows/verify.yml`: Windows/Linux verification and native release artifacts. Actions are pinned to reviewed commit IDs.

Keep signing and validation deterministic. Inject storage roots and clocks where tests need isolation. Validate input before filesystem changes. Throw or return an explicit failure; do not treat corrupt data as successful empty state. Preserve existing keys and issued license compatibility.

For a dependency change, review the publisher, package contents, build imports and transitive graph before executing build targets. Deliberately regenerate the locks, review the diff, and rerun the locked gate. Ordinary restores must not rewrite locks.

## UI and interaction checks

Verify 680, 1000 and 1440-pixel windows, light and dark themes, and the 0.8/1/1.5
font scales. Long product names, customer names and custom data must remain
readable through wrapping, scrolling or selected details. Check navigation,
keyboard focus, search shortcuts, modal cancellation, empty lists, failed loads
and stale input during generation. These checks complement the console's pure
layout, view-model, signature and persistence tests; compiling XAML alone does not
establish visual correctness.

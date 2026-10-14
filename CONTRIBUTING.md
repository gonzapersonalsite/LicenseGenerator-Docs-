# Contributing

This public repository contains documentation, the client verification example,
issue reports and downloadable releases. The desktop application's source and
build tooling are maintained in a separate private repository.

## Reports and suggestions

Use [Issues](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues)
for reproducible bugs, integration questions and feature suggestions. Include the
application version, operating system, installation source, steps and expected
result. Use invented customer data in examples. Never attach private keys,
unencrypted backups, real customer licenses or credentials.

Follow [SECURITY.md](SECURITY.md) for security vulnerabilities.

## Documentation changes

Open a pull request against `main`. Keep the English, Spanish, German, Brazilian
Portuguese and Simplified Chinese manuals consistent when changing shared
behavior. Verify relative links and the license format examples. Run:

```powershell
./scripts/verify-public.ps1
```

This uses PowerShell 7 and Git, without installing packages. The public-content
check rejects files outside the documentation and maintenance allowlist, private
key material and broken relative file links. It is a guard against mistakes, not
a substitute for reviewing every file before pushing: a pushed file may already
be publicly accessible even when CI fails.

## Releases and tags

Each public release tag points to a reviewed documentation snapshot. GitHub adds
`Source code (zip)` and `Source code (tar.gz)` automatically; here those archives
contain documentation and the public integration example. They are not the app's
buildable source or installers. Use the named executable downloads to run the app.

Use one `LicenseGenerator-X.Y.Z.0` tag per released version. The corresponding tag
in the private repository identifies the verified application source. Do not push
private branches or tags into this repository, and never use `git push --mirror`
to publish documentation. Publish new versions rather than overwriting released
executables. Historical corrections must be documented and backed up first.

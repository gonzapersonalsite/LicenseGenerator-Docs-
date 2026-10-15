# Contributing

This public repository contains the documentation, the client integration
examples (the C#, Python and Node.js verifiers with their tests and test
vectors), issue reports and downloadable releases. The desktop application's
source and build tooling are maintained in a separate private repository.

## Reports and suggestions

Use [Issues](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues)
for reproducible bugs, integration questions and feature suggestions. Include the
application version, operating system, installation source, steps and expected
result, and for an integration problem the verifier and stack you use. Use
invented customer data in examples. Never attach private keys, unencrypted
backups, real customer licenses or credentials.

Follow [SECURITY.md](SECURITY.md) for security vulnerabilities.

## Documentation and example changes

Pull requests are welcome as proposals. The documentation and examples here are
published from a maintained source, so an accepted change is applied there and
then published here, in English and in the Spanish, German, Brazilian Portuguese
and Simplified Chinese translations. Code in the integration guides is copied from
the example tests, so a change to that code starts in the tests.

Check public content and relative links with PowerShell 7 and Git, without
installing packages:

```powershell
./scripts/verify-public.ps1
```

Run the example tests from the repository root; the Python example first installs
its locked dependencies, preferably in a virtual environment:

```text
cd docs/examples/python
python -m pip install --require-hashes --only-binary :all: -r requirements.txt
python -m unittest
cd ../node
node --test license-verifier.test.mjs
```

The public-content check rejects files outside the documentation and maintenance
allowlist, private key material and broken relative file links. It is a guard
against mistakes, not a substitute for reviewing every file before pushing: a
pushed file may already be publicly accessible even when CI fails. CI runs that
check and both example suites on every push and pull request.

## Releases and tags

Each public release tag `LicenseGenerator-X.Y.Z.0` points to a reviewed
documentation snapshot for that application version. GitHub adds
`Source code (zip)` and `Source code (tar.gz)` automatically; here those archives
contain the documentation and integration examples, not the application's
buildable source or installers. Use the named executable downloads to run the
application.

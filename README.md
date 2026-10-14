# License Generator

[Español](docs/es/README.md) · **English** · [Deutsch](docs/de/README.md) · [Português](docs/pt/README.md) · [中文](docs/zh/README.md)

A desktop tool for issuing and managing offline software licenses. Create a separate RSA key pair for each product, sign licenses, and validate them in your own application using its public key.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Downloads](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/releases) · [Report an issue](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Support development](https://ko-fi.com/gonzaloapps)

## Features

- RSA-2048 signatures with SHA-256 and PKCS#1 v1.5; standard PEM keys.
- Product, customer name, hardware ID and optional expiration date in every signed license.
- Optional signed **UserDefined** text for features, plans or application-specific metadata.
- Local application management, searchable history and ZIP backup/import.
- English, Spanish, German, Brazilian Portuguese and Simplified Chinese interfaces.
- Self-contained Windows x64 and Linux x64 builds; end users do not need to install .NET. Linux still needs the desktop system libraries required by Avalonia.

## Start using it

1. Create a product under App Management.
2. Retrieve its `public.pem` and integrate verification in your client application.
3. Enter the customer name, the hardware ID supplied by your client and an optional expiration date.
4. Leave UserDefined empty for the existing license format, or enter custom text after updating your client to support format 2.
5. Generate and deliver the license code. Keep the private key and backups private.

UserDefined is signed text, not an executable rule or a built-in feature manager. Your application decides what its contents mean **after** verifying the signature.

## Documentation

- [Operations and integration](OPERATIONS.md): issuance, validation, backups and compatibility.
- [Architecture and security contract](ARCHITECTURE.md): exact signed bytes and security boundaries.
- [Reference C# verifier](docs/examples/LicenseVerifier.cs): dependency-free example compiled by the regression suite.
- [Development and verification](docs/DEVELOPMENT.md).
- [Release procedure](docs/RELEASE.md).
- [Changelog](docs/CHANGELOG.md).
- [Review checkpoint — 2026-10-14](docs/REVIEW-2026-10-14.md): verified results and remaining release checks.

Licenses are Base64-encoded JSON, not encrypted secrets. Hardware binding and expiration depend on the checks implemented by your client; offline licensing cannot prevent client modification or a manipulated system clock.

## Data and licensing

Data lives in the current user's local application-data directory under `LicenseGenerator`, independently of the executable. Moving the executable to a USB drive does not make the data portable. Export a backup before moving computers or modifying keys.

License Generator is proprietary software under the [EULA](LICENSE). Generated licenses and public keys can be distributed with your own commercial or free applications under those terms.

Developed by Gonzalo Martínez García · gonzalo.dev

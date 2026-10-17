# License Generator

[Español](docs/es/README.md) · **English** · [Deutsch](docs/de/README.md) · [Português](docs/pt/README.md) · [中文](docs/zh/README.md)

A desktop tool for issuing, inspecting and managing offline software licenses. Create a separate RSA key pair for each product, sign licenses, inspect them locally with a trusted public key, and validate them in your own application.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Downloads](https://github.com/gonzapersonalsite/license-generator-docs/releases) · [Report an issue](https://github.com/gonzapersonalsite/license-generator-docs/issues) · [Support development](https://ko-fi.com/gonzaloapps)

## Features

- RSA-2048 signatures with SHA-256 and PKCS#1 v1.5; standard PEM keys.
- Product, customer name, hardware ID and optional expiration date in every signed license.
- Optional signed **UserDefined** text for features, plans or application-specific metadata.
- Local application management, searchable history and ZIP backup/import.
- Local license inspection with an explicitly trusted public key and separate signature, product, hardware and expiration results.
- English, Spanish, German, Brazilian Portuguese and Simplified Chinese interfaces.
- Self-contained Windows x64 and Linux x64 builds; end users do not need to install .NET. Linux still needs the desktop system libraries required by Avalonia.

## Start using it

1. Create a product under **Applications**.
2. Retrieve its `public.pem` and add license verification to your client application with the [integration guide](INTEGRATION.md).
3. Enter the customer name, the hardware ID supplied by your client and an optional expiration date.
4. Leave UserDefined empty for the existing license format, or enter custom text after updating your client to support format 2.
5. Generate and deliver the license code. Keep the private key and backups private.

UserDefined is signed text, not an executable rule or a built-in feature manager. Your application decides what its contents mean **after** verifying the signature.

## Documentation

- [Operations guide](OPERATIONS.md): products, issuance, inspection, backups and your data.
- [Integration guide](INTEGRATION.md): verify licenses in your application, from activation screen to hardware identifier, with examples in C#, Python and Node.js.
- [License format and security contract](ARCHITECTURE.md): exact signed bytes, verification rules and security boundaries.
- Reference verifiers: [C#](docs/examples/LicenseVerifier.cs) with its [shared protocol](docs/examples/LicenseProtocol.cs), [Python](docs/examples/python/license_verifier.py) and [Node.js](docs/examples/node/license-verifier.mjs), all checked against the same [test vectors](docs/examples/test-vectors.json).
- [Changelog](docs/CHANGELOG.md).

Licenses are Base64-encoded JSON, not encrypted secrets. Hardware binding and expiration depend on the checks implemented by your client; offline licensing cannot prevent client modification or a manipulated system clock.

## Data and licensing

Keys, history, settings and a diagnostic log live in a data folder of your operating-system user, separate from the executable; the [privacy policy](https://gonzaloapps.es/en/apps/license-generator/legal/privacy-policy/) says where it is for each version and what it holds. Moving the executable does not move the data. Export a backup before moving computers, uninstalling or modifying keys.

License Generator is proprietary software under the [EULA](LICENSE). Generated licenses and public keys can be distributed with your own commercial or free applications under those terms, and the integration examples under `docs/examples` can be copied into them.

Developed by Gonzalo Martínez García · gonzalo.dev

# License Generator

**Español** · [English](../../README.md) · [Deutsch](../de/README.md) · [Português](../pt/README.md) · [中文](../zh/README.md)

Herramienta de escritorio para emitir, inspeccionar y gestionar licencias sin conexión. Cada producto tiene su propio par de claves RSA; puedes inspeccionar las licencias localmente con una clave pública de confianza y verificarlas en tu aplicación.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Comunicar un problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoyar el desarrollo](https://ko-fi.com/gonzaloapps)

- Firmas RSA-2048 con SHA-256 y PKCS#1 v1.5; claves PEM.
- Producto, nombre del cliente, HWID opcional y fecha de vencimiento opcional.
- **UserDefined**: texto adicional firmado para funciones, planes o metadatos.
- Gestión de aplicaciones, historial y copias ZIP.
- Inspección local con una clave pública elegida expresamente y comprobaciones separadas de firma, producto, equipo y caducidad.
- Interfaz en español, inglés, alemán, portugués brasileño y chino simplificado.
- Distribuciones autosuficientes para Windows x64 y Linux x64. No requieren instalar .NET; Linux necesita las bibliotecas de escritorio correspondientes.

Crea el producto, integra `public.pem` en tu aplicación y genera la licencia con los datos del cliente. Nunca distribuyas `private.pem`. Deja UserDefined vacío para mantener el formato anterior; el texto adicional requiere un cliente compatible con formato 2.

[Uso e integración](OPERATIONS.md) · [Arquitectura y seguridad](ARCHITECTURE.md) · [Verificador C#](../examples/LicenseVerifier.cs)

Los datos se guardan en el perfil del usuario, separados del ejecutable. Llevar el ejecutable en un USB no traslada las claves ni el historial. Las copias ZIP contienen claves privadas sin cifrar.

Software propietario sujeto al [EULA](../../LICENSE). Puedes distribuir las licencias generadas y las claves públicas con tus propios productos conforme a sus términos.

Desarrollado por Gonzalo Martínez García · gonzalo.dev

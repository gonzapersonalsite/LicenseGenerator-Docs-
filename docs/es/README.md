# License Generator

**Español** · [English](../../README.md) · [Deutsch](../de/README.md) · [Português](../pt/README.md) · [中文](../zh/README.md)

Una herramienta de escritorio para emitir, inspeccionar y gestionar licencias de software sin conexión. Crea un par de claves RSA propio para cada producto, firma licencias, inspecciónalas en local con una clave pública de confianza y valídalas en tu propia aplicación.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Descargas](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/releases) · [Comunicar un problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoyar el desarrollo](https://ko-fi.com/gonzaloapps)

## Funciones

- Firmas RSA-2048 con SHA-256 y PKCS#1 v1.5; claves PEM estándar.
- Producto, nombre del cliente, ID de hardware y fecha de caducidad opcional en cada licencia firmada.
- Texto **UserDefined** firmado opcional para funciones, planes o metadatos propios de la aplicación.
- Gestión local de aplicaciones, historial con búsqueda y copia de seguridad e importación en ZIP.
- Inspección local de licencias con una clave pública de confianza elegida expresamente y resultados separados de firma, producto, hardware y caducidad.
- Interfaz en inglés, español, alemán, portugués de Brasil y chino simplificado.
- Compilaciones autocontenidas para Windows x64 y Linux x64; los usuarios finales no necesitan instalar .NET. Linux sigue necesitando las bibliotecas de escritorio del sistema que requiere Avalonia.

## Empezar a usarla

1. Crea un producto en **Aplicaciones**.
2. Obtén su `public.pem` y añade la verificación de licencias a tu aplicación cliente con la [guía de integración](INTEGRATION.md).
3. Introduce el nombre del cliente, el ID de hardware que aporta tu aplicación cliente y una fecha de caducidad opcional.
4. Deja UserDefined vacío para el formato de licencia existente, o introduce texto personalizado después de actualizar tu cliente para que admita el formato 2.
5. Genera y entrega el código de licencia. Mantén en privado la clave privada y las copias de seguridad.

UserDefined es texto firmado, no una regla ejecutable ni un gestor de funciones integrado. Tu aplicación decide qué significa su contenido **después** de verificar la firma.

## Documentación

- [Guía de uso](OPERATIONS.md): productos, emisión, inspección, copias de seguridad y tus datos.
- [Guía de integración](INTEGRATION.md): verifica licencias en tu aplicación, desde la pantalla de activación hasta el identificador de hardware, con ejemplos en C#, Python y Node.js.
- [Contrato de formato y seguridad de la licencia](ARCHITECTURE.md): bytes firmados exactos, reglas de verificación y límites de seguridad.
- Verificadores de referencia: [C#](../examples/LicenseVerifier.cs) con su [protocolo compartido](../examples/LicenseProtocol.cs), [Python](../examples/python/license_verifier.py) y [Node.js](../examples/node/license-verifier.mjs), todos comprobados con los mismos [vectores de prueba](../examples/test-vectors.json).
- [Registro de cambios](../CHANGELOG.md), en inglés.

Las licencias son JSON codificado en Base64, no secretos cifrados. La vinculación al hardware y la caducidad dependen de las comprobaciones que implemente tu cliente; las licencias sin conexión no pueden impedir que se modifique el cliente ni que se manipule el reloj del sistema.

## Datos y licencia

Las claves, el historial, los ajustes y un registro de diagnóstico están en una carpeta de datos de tu usuario del sistema operativo, separada del ejecutable; la [política de privacidad](https://gonzaloapps.es/es/apps/license-generator/legal/privacy-policy/) indica dónde está en cada versión y qué contiene. Mover el ejecutable no mueve los datos. Exporta una copia de seguridad antes de cambiar de equipo, desinstalar o modificar claves.

License Generator es software propietario sujeto al [EULA](../../LICENSE) (en inglés). Las licencias generadas y las claves públicas pueden distribuirse con tus propias aplicaciones comerciales o gratuitas en esas condiciones, y los ejemplos de integración de `docs/examples` pueden copiarse en ellas.

Desarrollado por Gonzalo Martínez García · gonzalo.dev

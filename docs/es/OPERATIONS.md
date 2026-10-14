# Uso e integración

[Inicio](README.md) · [Guía completa en inglés](../../OPERATIONS.md) · [Contrato canónico](../../ARCHITECTURE.md)

1. Crea un producto en Gestión de aplicaciones.
2. Integra su `public.pem` y un verificador en tu aplicación. La clave privada se queda contigo.
3. Introduce el nombre del cliente y, si necesitas vinculación a un equipo, el HWID exacto que proporciona tu aplicación. Vacío significa sin vinculación.
4. Selecciona una fecha opcional: la licencia vence a las **00:00 UTC al inicio de ese día**. Para permitir todo el 10 de junio, elige el 11.
5. Deja UserDefined vacío para clientes antiguos o introduce texto como `features=export,reports` para clientes compatibles con formato 2.
6. Genera y entrega el código. El historial conserva la licencia emitida.

UserDefined es texto firmado que interpreta tu aplicación después de verificarlo. No almacenes secretos: Base64 no cifra los datos. No agregues el campo al formato anterior por tu cuenta.

El [verificador C# probado](../examples/LicenseVerifier.cs) recibe código, clave pública, producto esperado, HWID esperado y hora UTC. Comprueba firma, formato, producto, equipo y vencimiento. Vuelve a verificar al cargar la activación y al decidir el acceso. Para otros lenguajes implementa el mismo [contrato](../../ARCHITECTURE.md), sin convertir la fecha firmada a otra zona horaria.

Exporta una copia ZIP desde Ajustes antes de migrar o borrar claves. Incluye claves privadas **sin cifrar**. La importación conserva los productos y el historial existentes, añade los ausentes y rechaza claves o archivos de historial contradictorios. Los ajustes válidos importados sustituyen los actuales.

Los datos pertenecen al perfil del sistema operativo; mover únicamente el ejecutable no los mueve. Borrar una licencia del historial o un producto no revoca licencias entregadas. El sistema sin conexión tampoco aporta un reloj de confianza.

[Comunicar un problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoyar el desarrollo](https://ko-fi.com/gonzaloapps)

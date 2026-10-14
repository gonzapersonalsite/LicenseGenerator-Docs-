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

Para integrar el ejemplo, copia **ambos archivos**, [LicenseVerifier.cs](../examples/LicenseVerifier.cs) y [LicenseProtocol.cs](../examples/LicenseProtocol.cs), a tu proyecto. La [excepción de integración del EULA](../../LICENSE) permite usar, modificar y distribuir estos dos archivos con clientes comerciales o gratuitos conservando el aviso de copyright; no autoriza copiar el resto de la aplicación.

## Inspeccionar una licencia

Abre **Inspeccionar licencia** y pega el código completo, o abre una entrada desde el historial. La inspección no modifica el registro guardado. Elige expresamente la clave pública de un producto local o pega una clave pública PEM de una fuente de confianza. El contenido de la licencia nunca elige la clave; esta pantalla no acepta claves privadas.

Indica el producto esperado y, si quieres comprobar el equipo, su HWID exacto. Dejarlo vacío significa **sin comprobar** para una licencia vinculada; no confirma que pueda usarse en ese dispositivo. Una licencia sin vinculación no necesita esa comprobación.

Los campos solo aparecen después de verificar la firma. Producto, equipo y caducidad se comprueban por separado; una licencia auténtica puede estar caducada. Una firma incorrecta puede indicar otra clave o datos alterados, sin distinguir la causa. El resultado muestra el instante UTC de comprobación; cambiar entradas lo borra. Vuelve a verificar para comprobar otro instante. No se sube ni se guarda nada.

`TryVerify` mantiene su contrato. `LicenseVerifier.Inspect` devuelve `Rejected` sin campos, o `Authenticated` con campos firmados y resultados separados. Un HWID esperado `null` omite la comprobación: si existe vinculación, devuelve `NotChecked` e `IsValid = false`. `CheckFormat` solo comprueba formato y límites antes de leer claves; no autentica. Las tres API comparten el parser con la interfaz.

## Copias y migración

Exporta una copia ZIP desde Ajustes antes de migrar o borrar claves. Incluye claves privadas **sin cifrar**. La importación conserva los productos y el historial existentes, añade los ausentes y rechaza claves o archivos de historial contradictorios. Los ajustes válidos importados sustituyen los actuales.

Los datos pertenecen al perfil del sistema operativo; mover únicamente el ejecutable no los mueve. Borrar una licencia del historial o un producto no revoca licencias entregadas. El sistema sin conexión tampoco aporta un reloj de confianza.

[Comunicar un problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoyar el desarrollo](https://ko-fi.com/gonzaloapps)

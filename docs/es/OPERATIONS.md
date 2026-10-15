# Guía de uso

**Español** · [English](../../OPERATIONS.md) · [Deutsch](../de/OPERATIONS.md) · [Português](../pt/OPERATIONS.md) · [中文](../zh/OPERATIONS.md)

Esta guía trata del uso de la aplicación License Generator: crear productos,
emitir e inspeccionar licencias y mantener tus datos a salvo. Verificar licencias
en tu propia aplicación lo explica la [guía de integración](INTEGRATION.md), y el
formato exacto de la licencia, el [contrato de formato y seguridad](ARCHITECTURE.md).

## Crear un producto y emitir una licencia

Crea un producto en **Aplicaciones**. El generador guarda un par de claves RSA propio para él en su carpeta de datos. Tu aplicación cliente necesita el `public.pem` del producto; no distribuyas nunca `private.pem`.

Para vincular la licencia a un equipo, tu cliente calcula un identificador de hardware, muestra ese valor exacto a su usuario, y tú lo introduces en el campo **ID de Hardware (Opcional)** del generador. El generador no elige el método de identificación del hardware; la [guía de integración](INTEGRATION.md#elegir-un-identificador-de-hardware) describe lo que necesita uno bueno. Un HWID vacío a propósito produce una licencia sin vinculación al hardware.

Selecciona el producto, introduce el nombre de registro y el HWID opcional, y elige si quieres una fecha de caducidad. La licencia caduca a las **00:00 UTC del inicio de la fecha elegida**. Para permitir todo el 10 de junio, elige el 11 de junio. Deja la fecha vacía para una licencia perpetua.

Genera la licencia, copia el código resultante y entrégaselo al cliente. El historial registra las licencias emitidas. Borrar una entrada del historial solo elimina el registro local; borrar un producto elimina sus claves e impide emitir más licencias con ellas. Ninguna de las dos operaciones revoca licencias ya entregadas. Haz una copia de seguridad de las claves antes de borrar.

## Datos personalizados firmados: UserDefined

Deja UserDefined vacío para generar el formato heredado. En ese caso no se emite ninguna propiedad JSON nueva.

Para un cliente que implemente el formato 2, introduce texto plano como:

```text
features=export,reports;plan=professional
```

El texto exacto entra en la firma. El generador no le asigna ningún significado; tu cliente decide qué significa **después de la verificación criptográfica** ([guía de integración](INTEGRATION.md#usar-userdefined)). No pongas secretos en este campo: Base64 es una codificación, no un cifrado.

Cualquier texto personalizado no vacío usa el formato 2, que un cliente escrito para el formato heredado rechaza. Actualiza antes todos los clientes que deban aceptar la licencia.

## Inspeccionar una licencia

Abre **Inspeccionar licencia** y pega un código de licencia completo, o abre una entrada guardada desde el **Historial**. La inspección no modifica el registro guardado. Elige expresamente la clave pública de un producto local o pega una clave pública PEM obtenida de una fuente de confianza. El contenido de la licencia nunca elige la clave de confianza. Esta pantalla rechaza las claves privadas PEM.

Rellena **ID de producto esperado** y, si quieres, **ID de hardware esperado (opcional)** con el ID de hardware exacto del cliente. Un HWID esperado vacío significa **sin comprobar** para una licencia vinculada; no confirma que la licencia pueda usarse en ese dispositivo. Una licencia no vinculada no necesita comprobación de hardware.

La pantalla autentica la firma RSA antes de mostrar ningún campo de la licencia y después informa por separado de las comprobaciones de producto, hardware y caducidad. Las licencias auténticas caducadas pueden inspeccionarse, pero siguen caducadas. Una firma no coincidente puede deberse a una clave equivocada o a datos alterados de la licencia; no permite distinguir entre ambas causas. El resultado muestra la hora UTC de la comprobación. Cambiar cualquier valor introducido borra el resultado; vuelve a verificar para un instante posterior. La inspección no sube ni guarda nada.

## Tus datos

La aplicación guarda las claves de firma, el historial de licencias, los ajustes y un registro de diagnóstico en una carpeta de datos que pertenece a tu usuario del sistema operativo, separada del ejecutable. La [política de privacidad](https://gonzaloapps.es/es/apps/license-generator/legal/privacy-policy/) detalla qué contiene cada archivo y dónde está la carpeta en las versiones de Microsoft Store, Windows portable y Linux. Exporta una copia de seguridad antes de desinstalar la versión de Microsoft Store: Windows borra su carpeta de datos con ella.

Las claves privadas y las copias de seguridad **no están cifradas**. Quien obtenga una clave privada puede emitir licencias que tus aplicaciones aceptan.

## Copias de seguridad y migración

Usa **Ajustes** para exportar una copia de seguridad ZIP. Contiene claves privadas, historial y ajustes sin cifrar. Guárdala en un almacenamiento que controles; cífrala con la solución de copias que prefieras antes de compartirla o subirla.

La importación combina productos e historial. Las claves y el historial existentes se conservan. Si un producto importado ya existe con otras claves, o un archivo de historial con el mismo nombre tiene otro contenido, la importación falla en lugar de sobrescribirlo. Los ajustes importados válidos sustituyen a los actuales. Antes de importar se validan la estructura del archivo, su tamaño y los pares de claves RSA. Exporta antes una copia del perfil actual si quieres recuperar después sus ajustes. Una importación no revoca ni sustituye licencias ya entregadas a clientes.

Copiar solo el ejecutable a otro equipo no traslada las claves ni el historial, y la versión de Microsoft Store y la portable usan carpetas distintas: exporta una copia de seguridad e impórtala en la nueva instalación.

## Solución de problemas

| Síntoma | Comprobación |
| --- | --- |
| Un cliente rechaza una licencia | Sigue las comprobaciones del cliente en la [guía de integración](INTEGRATION.md#solución-de-problemas); inspecciona aquí la licencia con la misma clave pública, el mismo producto y el mismo HWID. |
| Caducó antes de lo esperado | La fecha elegida es el límite exclusivo a las 00:00 UTC; no es el último día permitido. |
| La importación informa de un conflicto | Conserva los datos actuales e inspecciona la copia; no sustituyas por accidente la clave privada de un producto. |
| Faltan datos tras mover el ejecutable o reinstalar | Los datos están en la carpeta del usuario actual para esa versión; importa la copia en la nueva instalación. |

La emisión sin conexión no ofrece revocación remota, un reloj de confianza ni protección frente a un cliente modificado. Consulta los [límites de seguridad](ARCHITECTURE.md#límites-de-seguridad) antes de elegir una política de integración.

[Comunicar un problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoyar el desarrollo](https://ko-fi.com/gonzaloapps)

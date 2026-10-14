# Arquitectura y seguridad

[Inicio](README.md) · [English: contrato canónico](../../ARCHITECTURE.md)

La aplicación usa Avalonia, MVVM y servicios para firma, almacenamiento, ajustes, idiomas e integración con el escritorio. El generador conserva las claves privadas; los clientes reciben únicamente las públicas.

El [contrato canónico](../../ARCHITECTURE.md) es la fuente única para tipos JSON, bytes firmados, formatos y validación. El [verificador C#](../examples/LicenseVerifier.cs) se compila y prueba junto con la aplicación.

- Las licencias son JSON UTF-8 codificado en Base64; no están cifradas.
- Las firmas usan RSA-2048, SHA-256 y PKCS#1 v1.5.
- UserDefined vacío conserva el formato anterior y omite las nuevas propiedades.
- UserDefined no vacío exige formato 2 y un cliente actualizado. La firma protege el texto completo; la aplicación decide su significado.
- Se rechazan versiones desconocidas y combinaciones de formato incoherentes. No se intenta validar como antiguo un código de formato 2 que haya fallado.
- El formato anterior rechaza `|` en los campos porque puede producir interpretaciones ambiguas.
- El vencimiento es a las **00:00 UTC al inicio de la fecha indicada**. Se comprueba en cada decisión de acceso, sin conservar un booleano válido indefinidamente.
- Un HWID vacío permite usar la licencia en cualquier equipo; uno no vacío debe coincidir exactamente.

Los datos se guardan en `<LocalApplicationData>/LicenseGenerator/`: claves en `Keys/`, historial en `Licenses/`, ajustes en `settings.json` y registros en `Logs/`. Las copias ZIP no están cifradas. La importación valida el contenido, añade datos ausentes, rechaza conflictos y sustituye únicamente ajustes válidos de forma atómica.

La firma no impide modificar el código cliente ni manipular su reloj. No existe revocación remota sin un mecanismo adicional. Protege las claves y consulta los [límites de seguridad](../../ARCHITECTURE.md#security-boundaries) y el [EULA](../../LICENSE).

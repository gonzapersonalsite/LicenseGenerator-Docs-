# Arquitectura y seguridad

[Inicio](README.md) · [English: contrato canónico](../../ARCHITECTURE.md)

La aplicación usa Avalonia, MVVM y servicios para firma, almacenamiento, ajustes, idiomas e integración con el escritorio. El generador conserva las claves privadas; los clientes reciben únicamente las públicas.

Construir una página no lee el almacén de licencias. La navegación muestra la página antes de esperar su carga cancelable y conserva su instancia para mantener los borradores. Los modelos de vista ejecutan las operaciones de almacenamiento en hilos de trabajo y actualizan el estado observable en el contexto de la interfaz. Se impiden operaciones duplicadas mientras hay una en curso. Una escritura iniciada termina; editar el formulario invalida el resultado mostrado sin perder la licencia guardada.

El servicio de almacenamiento lee productos e historial bajo el mismo bloqueo y devuelve esos datos en una instantánea `WorkspaceSnapshot`. El panel calcula sus totales con esa instantánea y avisa de entradas dañadas que se hayan omitido. Las listas conservan los resultados filtrados y los paginan sin volver a leer el disco.

`AdaptiveGrid` pertenece a la capa visual: mide los elementos visibles y adapta las columnas al ancho disponible y al tamaño del texto. Los diálogos de confirmación son modales, enfocan inicialmente Cancelar y restauran el foco al cerrarse. Los modelos de vista solicitan la confirmación mediante `IDialogService`, lo que permite probar ambas respuestas.

El [contrato canónico](../../ARCHITECTURE.md) es la fuente única para tipos JSON, bytes firmados, formatos y validación. El [verificador C#](../examples/LicenseVerifier.cs) y su [protocolo compartido](../examples/LicenseProtocol.cs) se compilan y prueban junto con la aplicación; copia ambos archivos al integrar el ejemplo.

- Las licencias son JSON UTF-8 codificado en Base64; no están cifradas.
- Las firmas usan RSA-2048, SHA-256 y PKCS#1 v1.5.
- UserDefined vacío conserva el formato anterior y omite las nuevas propiedades.
- UserDefined no vacío exige formato 2 y un cliente actualizado. La firma protege el texto completo; la aplicación decide su significado.
- Se rechazan versiones desconocidas y combinaciones de formato incoherentes. No se intenta validar como antiguo un código de formato 2 que haya fallado.
- El formato anterior rechaza `|` en los campos porque puede producir interpretaciones ambiguas.
- El vencimiento es a las **00:00 UTC al inicio de la fecha indicada**. Se comprueba en cada decisión de acceso, sin conservar un booleano válido indefinidamente.
- Un HWID vacío permite usar la licencia en cualquier equipo; uno no vacío debe coincidir exactamente.

El inspector usa ese mismo verificador. `TryVerify` mantiene su contrato booleano; `Inspect` devuelve un rechazo sin campos autenticados o datos autenticados con comprobaciones independientes de producto, equipo y caducidad. Omitir el HWID esperado deja una licencia vinculada al equipo como **sin comprobar**, nunca como válida. Una firma incorrecta no permite distinguir entre una clave equivocada y datos alterados. La clave pública debe elegirse expresamente entre los productos locales o pegarse desde una fuente de confianza; nunca se elige a partir del JSON recibido. La inspección no escribe historial, no contacta con servidores ni proporciona revocación.

Los datos se guardan en `<LocalApplicationData>/LicenseGenerator/`: claves en `Keys`, historial en `Licenses`, ajustes en `settings.json` y registros en `Logs`. Las copias ZIP no están cifradas. La importación valida el contenido, añade datos ausentes, rechaza conflictos y sustituye únicamente ajustes válidos de forma atómica.

La firma no impide modificar el código cliente ni manipular su reloj. No existe revocación remota sin un mecanismo adicional. Protege las claves y consulta los [límites de seguridad](../../ARCHITECTURE.md#security-boundaries) y el [EULA](../../LICENSE).

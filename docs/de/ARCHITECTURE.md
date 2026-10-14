# Architektur und Sicherheit

[Start](README.md) · [Englischer verbindlicher Vertrag](../../ARCHITECTURE.md)

Avalonia stellt die Oberfläche dar, MVVM koordiniert Befehle und Dienste übernehmen Signierung, Speicherung, Einstellungen und Desktop-Integration. Private Schlüssel verbleiben beim Herausgeber; Clients erhalten öffentliche Schlüssel.

Der [kanonische Vertrag](../../ARCHITECTURE.md) definiert JSON-Typen, signierte Bytes, Versionen und Prüfregeln. Das [C#-Beispiel](../examples/LicenseVerifier.cs) wird mit der Anwendung kompiliert und getestet.

- Lizenzen sind UTF-8-JSON in Base64, keine verschlüsselten Geheimnisse.
- Signaturen verwenden RSA-2048, SHA-256 und PKCS#1 v1.5.
- Leeres UserDefined erhält das bisherige Format ohne neue JSON-Eigenschaften.
- Nichtleeres UserDefined verwendet Format 2 und erfordert einen aktualisierten Client.
- Unbekannte Versionen oder widersprüchliche Felder werden abgelehnt. Bei einem Format-2-Fehler darf kein Rückfall auf das alte Format erfolgen.
- Das alte Format erlaubt kein `|` in Textfeldern, um mehrdeutige Signaturdaten zu verhindern.
- Ablaufzeit ist **00:00 UTC am Anfang des angegebenen Datums**. Jede Zugriffsentscheidung muss erneut prüfen.
- Eine leere HardwareId erlaubt alle Geräte; andernfalls muss sie exakt übereinstimmen.

Daten liegen in `<LocalApplicationData>/LicenseGenerator/`: `Keys/`, `Licenses/`, `settings.json` und `Logs/`. ZIP-Sicherungen sind unverschlüsselt. Import fügt fehlende Daten hinzu, lehnt Konflikte ab und ersetzt gültige Einstellungen atomar.

Offline-Signaturen verhindern weder Änderungen am Client noch eine manipulierte Systemuhr. Fernsperren benötigen zusätzliche Mechanismen. Siehe [Sicherheitsgrenzen](../../ARCHITECTURE.md#security-boundaries) und [EULA](../../LICENSE).

# Architektur und Sicherheit

[Start](README.md) · [Englischer verbindlicher Vertrag](../../ARCHITECTURE.md)

Avalonia stellt die Oberfläche dar, MVVM koordiniert Befehle und Dienste übernehmen Signierung, Speicherung, Einstellungen und Desktop-Integration. Private Schlüssel verbleiben beim Herausgeber; Clients erhalten öffentliche Schlüssel.

Beim Erstellen einer Seite wird der Lizenzspeicher nicht gelesen. Die Navigation zeigt die Seite vor dem abbrechbaren Laden an und behält ihre Instanz, damit Formulareingaben erhalten bleiben. ViewModels führen Speicheroperationen auf Hintergrundthreads aus und aktualisieren ihren beobachtbaren Zustand im UI-Kontext. Während einer laufenden Operation werden doppelte Änderungen verhindert. Ein bereits gestarteter Schreibvorgang wird abgeschlossen; geänderte Formulareingaben verwerfen das angezeigte Ergebnis, ohne die gespeicherte Lizenz zu verlieren.

Der Speicherlesedienst liest Produkte und Verlauf unter derselben Speichersperre und gibt diese Daten als `WorkspaceSnapshot` zurück. Das Dashboard berechnet seine Summen aus diesem gemeinsamen Stand und meldet übersprungene beschädigte Verlaufseinträge. Listen behalten gefilterte Ergebnisse und blättern darin, ohne den Speicher erneut zu lesen.

`AdaptiveGrid` gehört zur Darstellung: Es misst sichtbare Elemente und passt die Spalten an Breite und Schriftgröße an. Bestätigungsdialoge sind modal, fokussieren zunächst Abbrechen und stellen den Fokus anschließend wieder her. ViewModels fordern die Bestätigung über `IDialogService` an, sodass beide Antworten getestet werden können.

Der [kanonische Vertrag](../../ARCHITECTURE.md) definiert JSON-Typen, signierte Bytes, Versionen und Prüfregeln. Das [C#-Prüfbeispiel](../examples/LicenseVerifier.cs) und sein [gemeinsames Protokoll](../examples/LicenseProtocol.cs) werden mit der Anwendung kompiliert und getestet; kopieren Sie zur Integration beide Dateien.

- Lizenzen sind UTF-8-JSON in Base64, keine verschlüsselten Geheimnisse.
- Signaturen verwenden RSA-2048, SHA-256 und PKCS#1 v1.5.
- Leeres UserDefined erhält das bisherige Format ohne neue JSON-Eigenschaften.
- Nichtleeres UserDefined verwendet Format 2 und erfordert einen aktualisierten Client.
- Unbekannte Versionen oder widersprüchliche Felder werden abgelehnt. Bei einem Format-2-Fehler darf kein Rückfall auf das alte Format erfolgen.
- Das alte Format erlaubt kein `|` in Textfeldern, um mehrdeutige Signaturdaten zu verhindern.
- Ablaufzeit ist **00:00 UTC am Anfang des angegebenen Datums**. Jede Zugriffsentscheidung muss erneut prüfen.
- Eine leere HardwareId erlaubt alle Geräte; andernfalls muss sie exakt übereinstimmen.

Der lokale Prüfer verwendet denselben Verifikationscode. `TryVerify` behält seinen booleschen Vertrag; `Inspect` liefert entweder eine Ablehnung ohne authentifizierte Felder oder authentifizierte Daten mit unabhängigen Produkt-, Hardware- und Ablaufprüfungen. Ohne erwartete Hardware-ID bleibt eine hardwaregebundene Lizenz **nicht geprüft** und gilt nicht als gültig. Eine falsche Signatur unterscheidet nicht zwischen falschem Schlüssel und veränderten Daten. Der öffentliche Schlüssel muss ausdrücklich aus lokalen Produkten gewählt oder aus einer vertrauenswürdigen Quelle eingefügt werden; empfangenes JSON bestimmt niemals die Vertrauensquelle. Die Prüfung schreibt keinen Verlauf, kontaktiert keinen Server und bietet keine Sperrung.

Daten liegen in `<LocalApplicationData>/LicenseGenerator/`: `Keys`, `Licenses`, `settings.json` und `Logs`. ZIP-Sicherungen sind unverschlüsselt. Import fügt fehlende Daten hinzu, lehnt Konflikte ab und ersetzt gültige Einstellungen atomar.

Offline-Signaturen verhindern weder Änderungen am Client noch eine manipulierte Systemuhr. Fernsperren benötigen zusätzliche Mechanismen. Siehe [Sicherheitsgrenzen](../../ARCHITECTURE.md#security-boundaries) und [EULA](../../LICENSE).

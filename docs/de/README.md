# License Generator

[Español](../es/README.md) · [English](../../README.md) · **Deutsch** · [Português](../pt/README.md) · [中文](../zh/README.md)

Ein Desktop-Werkzeug zum Ausstellen, Prüfen und Verwalten von Offline-Softwarelizenzen. Erstellen Sie für jedes Produkt ein eigenes RSA-Schlüsselpaar, signieren Sie Lizenzen, prüfen Sie sie lokal mit einem vertrauenswürdigen öffentlichen Schlüssel und validieren Sie sie in Ihrer eigenen Anwendung.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Downloads](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/releases) · [Problem melden](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Entwicklung unterstützen](https://ko-fi.com/gonzaloapps)

## Funktionen

- RSA-2048-Signaturen mit SHA-256 und PKCS#1 v1.5; Standard-PEM-Schlüssel.
- Produkt, Kundenname, Hardwarekennung und optionales Ablaufdatum in jeder signierten Lizenz.
- Optionaler signierter **UserDefined**-Text für Funktionen, Tarife oder anwendungsspezifische Metadaten.
- Lokale Anwendungsverwaltung, durchsuchbarer Verlauf sowie ZIP-Sicherung und -Import.
- Lokale Lizenzprüfung mit einem ausdrücklich als vertrauenswürdig gewählten öffentlichen Schlüssel und getrennten Ergebnissen für Signatur, Produkt, Hardware und Ablauf.
- Oberfläche auf Englisch, Spanisch, Deutsch, brasilianischem Portugiesisch und vereinfachtem Chinesisch.
- Eigenständige Builds für Windows x64 und Linux x64; Endbenutzer müssen .NET nicht installieren. Linux benötigt weiterhin die Desktop-Systembibliotheken, die Avalonia voraussetzt.

## Erste Schritte

1. Legen Sie unter **Anwendungen** ein Produkt an.
2. Übernehmen Sie seine `public.pem` und fügen Sie Ihrer Client-Anwendung anhand des [Integrationsleitfadens](INTEGRATION.md) die Lizenzprüfung hinzu.
3. Geben Sie den Kundennamen, die von Ihrem Client gelieferte Hardwarekennung und optional ein Ablaufdatum ein.
4. Lassen Sie UserDefined für das bisherige Lizenzformat leer oder geben Sie benutzerdefinierten Text ein, nachdem Sie Ihren Client so aktualisiert haben, dass er Format 2 unterstützt.
5. Generieren und übergeben Sie den Lizenzcode. Halten Sie den privaten Schlüssel und die Sicherungen geheim.

UserDefined ist signierter Text, keine ausführbare Regel und keine eingebaute Funktionsverwaltung. Ihre Anwendung entscheidet **nach** der Prüfung der Signatur, was sein Inhalt bedeutet.

## Dokumentation

- [Bedienungsanleitung](OPERATIONS.md): Produkte, Ausstellung, Prüfung, Sicherungen und Ihre Daten.
- [Integrationsleitfaden](INTEGRATION.md): Lizenzen in Ihrer Anwendung prüfen, vom Aktivierungsbildschirm bis zur Hardwarekennung, mit Beispielen in C#, Python und Node.js.
- [Format- und Sicherheitsvertrag der Lizenz](ARCHITECTURE.md): exakte signierte Bytes, Prüfregeln und Sicherheitsgrenzen.
- Referenz-Verifizierer: [C#](../examples/LicenseVerifier.cs) mit seinem [gemeinsamen Protokoll](../examples/LicenseProtocol.cs), [Python](../examples/python/license_verifier.py) und [Node.js](../examples/node/license-verifier.mjs), alle gegen dieselben [Testvektoren](../examples/test-vectors.json) geprüft.
- [Änderungsprotokoll](../CHANGELOG.md), auf Englisch.

Lizenzen sind Base64-kodiertes JSON, keine verschlüsselten Geheimnisse. Hardwarebindung und Ablauf hängen von den Prüfungen ab, die Ihr Client umsetzt; Offline-Lizenzen können weder die Veränderung des Clients noch eine manipulierte Systemuhr verhindern.

## Daten und Lizenz

Schlüssel, Verlauf, Einstellungen und ein Diagnoseprotokoll liegen in einem Datenordner Ihres Benutzerkontos im Betriebssystem, getrennt von der ausführbaren Datei; die [Datenschutzerklärung](https://gonzaloapps.es/apps/license-generator/legal/privacy-policy/) (auf Englisch) nennt für jede Version, wo er liegt und was er enthält. Das Verschieben der ausführbaren Datei verschiebt die Daten nicht. Exportieren Sie eine Sicherung, bevor Sie den Computer wechseln, die Anwendung deinstallieren oder Schlüssel ändern.

License Generator ist proprietäre Software gemäß der [EULA](../../LICENSE) (auf Englisch). Generierte Lizenzen und öffentliche Schlüssel dürfen unter diesen Bedingungen mit Ihren eigenen kommerziellen oder kostenlosen Anwendungen weitergegeben werden, und die Integrationsbeispiele unter `docs/examples` dürfen in sie kopiert werden.

Entwickelt von Gonzalo Martínez García · gonzalo.dev

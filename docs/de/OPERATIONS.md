# Bedienung und Integration

[Start](README.md) · [Vollständige englische Anleitung](../../OPERATIONS.md) · [Kanonischer Vertrag](../../ARCHITECTURE.md)

1. Erstellen Sie ein Produkt in der Anwendungsverwaltung.
2. Integrieren Sie `public.pem` und die Lizenzprüfung in Ihren Client. Behalten Sie den privaten Schlüssel.
3. Geben Sie den Kundennamen und bei Hardwarebindung die exakte HWID des Clients ein. Leer bedeutet keine Hardwarebindung.
4. Ein optionales Ablaufdatum gilt als Grenze um **00:00 UTC am Anfang dieses Tages**. Für den gesamten 10. Juni wählen Sie den 11. Juni.
5. Lassen Sie UserDefined für ältere Clients leer. Für Format-2-Clients ist beispielsweise `features=export,reports` möglich.
6. Generieren und übermitteln Sie den Code; der Verlauf speichert die Lizenz.

Ihre Anwendung interpretiert UserDefined erst nach erfolgreicher Signaturprüfung. Base64 verschlüsselt nichts; verwenden Sie das Feld nicht für Geheimnisse und erweitern Sie nicht eigenmächtig die alte Signaturzeichenfolge.

Das [getestete C#-Beispiel](../examples/LicenseVerifier.cs) erhält Lizenzcode, öffentlichen Schlüssel, erwartetes Produkt, HWID und UTC-Zeit. Prüfen Sie beim Laden und bei Zugriffsentscheidungen erneut. Implementierungen in anderen Sprachen müssen den [gleichen Vertrag](../../ARCHITECTURE.md) erfüllen und das signierte Datum ohne Zeitzonenverschiebung verwenden.

Exportieren Sie vor Migration oder Schlüssellöschung eine ZIP-Sicherung. Sie enthält **unverschlüsselte private Schlüssel**. Import erhält bestehende Produkte und Verlaufsdaten, ergänzt fehlende Einträge und lehnt widersprüchliche Schlüssel oder Verlaufsdateien ab. Gültige importierte Einstellungen ersetzen die aktuellen.

Daten gehören zum Benutzerprofil; das Verschieben der Programmdatei überträgt sie nicht. Das Löschen eines Produkts oder Verlaufseintrags widerruft keine ausgelieferte Lizenz. Offline-Lizenzierung stellt keine vertrauenswürdige Uhr bereit.

[Problem melden](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Entwicklung unterstützen](https://ko-fi.com/gonzaloapps)

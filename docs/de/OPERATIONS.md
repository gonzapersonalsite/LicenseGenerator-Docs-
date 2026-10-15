# Bedienungsanleitung

[Español](../es/OPERATIONS.md) · [English](../../OPERATIONS.md) · **Deutsch** · [Português](../pt/OPERATIONS.md) · [中文](../zh/OPERATIONS.md)

Diese Anleitung behandelt die Bedienung der Anwendung License Generator: Produkte
anlegen, Lizenzen ausstellen und prüfen sowie Ihre Daten sicher aufbewahren. Wie
Sie Lizenzen in Ihrer eigenen Anwendung prüfen, erklärt der
[Leitfaden zur Client-Integration](INTEGRATION.md), das genaue Lizenzformat der
[Format- und Sicherheitsvertrag](ARCHITECTURE.md).

## Ein Produkt anlegen und eine Lizenz ausstellen

Legen Sie unter **Anwendungen** ein Produkt an. Der Generator verwahrt dafür ein eigenes RSA-Schlüsselpaar in seinem Datenordner. Ihre Client-Anwendung braucht die `public.pem` des Produkts; geben Sie `private.pem` niemals weiter.

Für eine Hardwarebindung berechnet Ihr Client eine Hardwarekennung, zeigt seinem Benutzer genau diesen Wert an, und Sie geben ihn im Feld **Hardwarekennung (optional)** des Generators ein. Der Generator wählt das Verfahren zur Ermittlung der Hardwarekennung nicht aus; der [Integrationsleitfaden](INTEGRATION.md#eine-hardwarekennung-wählen) beschreibt, was ein gutes Verfahren braucht. Eine bewusst leere HWID erzeugt eine Lizenz ohne Hardwarebindung.

Wählen Sie das Produkt, geben Sie den Registrierungsnamen und optional die HWID ein und wählen Sie bei Bedarf ein Ablaufdatum. Die Lizenz läuft um **00:00 UTC zu Beginn des gewählten Datums** ab. Soll die Lizenz den ganzen 10. Juni gelten, wählen Sie den 11. Juni. Lassen Sie das Datum leer, um eine unbefristete Lizenz zu erzeugen.

Generieren Sie die Lizenz und kopieren Sie den Code für den Kunden. Der **Verlauf** verzeichnet ausgestellte Lizenzen. Das Löschen eines Verlaufseintrags entfernt nur den lokalen Eintrag; das Löschen eines Produkts entfernt seine Schlüssel und verhindert, dass weitere Lizenzen damit ausgestellt werden. Keiner der beiden Vorgänge widerruft bereits ausgelieferte Lizenzen. Sichern Sie die Schlüssel vor dem Löschen.

## Signierte benutzerdefinierte Daten: UserDefined

Lassen Sie UserDefined leer, um das bisherige Format zu erzeugen. In diesem Fall werden keine neuen JSON-Eigenschaften ausgegeben.

Für einen Client, der Format 2 umsetzt, geben Sie einfachen Text ein, etwa:

```text
features=export,reports;plan=professional
```

Der genaue Text fließt in die Signatur ein. Der Generator gibt ihm keine Bedeutung; Ihr Client entscheidet **nach der kryptografischen Prüfung**, was er bedeutet ([Integrationsleitfaden](INTEGRATION.md#userdefined-verwenden)). Legen Sie in diesem Feld keine Geheimnisse ab: Base64 ist eine Kodierung, keine Verschlüsselung.

Jeder nicht leere benutzerdefinierte Text verwendet Format 2, das ein für das bisherige Format geschriebener Client ablehnt. Aktualisieren Sie zuerst jeden Client, der die Lizenz annehmen soll.

## Eine Lizenz prüfen

Öffnen Sie **Lizenz prüfen** und fügen Sie einen vollständigen Lizenzcode ein oder öffnen Sie einen gespeicherten Eintrag aus dem **Verlauf**. Die Prüfung ändert den gespeicherten Eintrag nicht. Wählen Sie ausdrücklich den öffentlichen Schlüssel eines lokalen Produkts oder fügen Sie einen öffentlichen PEM-Schlüssel aus einer vertrauenswürdigen Quelle ein. Der Inhalt der Lizenz wählt nie den vertrauenswürdigen Schlüssel aus. Private PEM-Schlüssel lehnt dieser Bildschirm ab.

Geben Sie die erwartete Produkt-ID und optional die genaue Hardwarekennung des Clients an. Eine leere erwartete HWID bedeutet bei einer hardwaregebundenen Lizenz **nicht geprüft**; sie bestätigt nicht, dass die Lizenz auf diesem Gerät verwendbar ist. Eine ungebundene Lizenz braucht keine Hardwareprüfung.

Der Bildschirm authentifiziert die RSA-Signatur, bevor er Felder der Lizenz anzeigt, und meldet danach die Prüfungen von Produkt, Hardware und Ablauf getrennt. Echte abgelaufene Lizenzen lassen sich prüfen, bleiben aber abgelaufen. Eine nicht übereinstimmende Signatur kann auf einen falschen Schlüssel oder veränderte Lizenzdaten hindeuten; diese Ursachen lassen sich nicht unterscheiden. Das Ergebnis zeigt die UTC-Zeit der Prüfung. Eine Änderung der Eingaben löscht das Ergebnis; prüfen Sie für einen späteren Zeitpunkt erneut. Bei der Prüfung wird nichts hochgeladen oder gespeichert.

## Ihre Daten

Die Anwendung bewahrt Signaturschlüssel, Lizenzverlauf, Einstellungen und ein Diagnoseprotokoll in einem Datenordner auf, der zu Ihrem Benutzerkonto im Betriebssystem gehört und von der ausführbaren Datei getrennt ist. Die [Datenschutzerklärung](https://gonzaloapps.es/apps/license-generator/legal/privacy-policy/) (auf Englisch) führt auf, was jede Datei enthält und wo sich der Ordner bei der Microsoft-Store-, der portablen Windows- und der Linux-Version befindet. Exportieren Sie eine Sicherung, bevor Sie die Microsoft-Store-Version deinstallieren: Windows entfernt ihren Datenordner mit ihr.

Private Schlüssel und Sicherungen sind **nicht verschlüsselt**. Wer einen privaten Schlüssel erhält, kann Lizenzen ausstellen, die Ihre Anwendungen annehmen.

## Sichern oder umziehen

Exportieren Sie unter **Einstellungen** eine ZIP-Sicherung. Sie enthält private Schlüssel, Verlauf und Einstellungen unverschlüsselt. Bewahren Sie sie in einem Speicher auf, den Sie kontrollieren; verschlüsseln Sie sie mit einer Sicherungslösung Ihrer Wahl, bevor Sie sie weitergeben oder hochladen.

Der Import führt Produkte und Verlauf zusammen. Vorhandene Schlüssel und Verlaufsdaten bleiben erhalten. Hat ein importiertes Produkt andere Schlüssel oder eine gleichnamige Verlaufsdatei einen anderen Inhalt, schlägt der Import fehl, statt etwas zu überschreiben. Gültige importierte Einstellungen ersetzen die aktuellen. Aufbau, Größe und RSA-Schlüsselpaare des Archivs werden vor dem Import geprüft. Exportieren Sie vorher eine Sicherung des aktuellen Profils, wenn Sie dessen Einstellungen später wiederherstellen möchten. Ein Import widerruft oder ersetzt keine bereits an Kunden ausgelieferten Lizenzen.

Das Kopieren nur der ausführbaren Datei auf einen anderen Computer verschiebt weder Schlüssel noch Verlauf: Exportieren Sie eine Sicherung und importieren Sie sie in der neuen Installation. Im selben Windows-Konto verwendet die Microsoft-Store-Version auch den Datenordner einer portablen Version. Sie zeigt dessen Produkte und Verlauf an, und wenn Sie eines davon in der Store-Version löschen, wird es aus diesem Ordner gelöscht; was die Store-Version anlegt, bleibt in ihrem eigenen Ordner, den die portable Version nicht sieht. Exportieren Sie vor dem Löschen des Ordners der portablen Version eine Sicherung in der Store-Version und importieren Sie sie danach erneut, damit diese Produkte und ihr Verlauf in der Store-Version erhalten bleiben.

## Fehlerbehebung

| Symptom | Prüfen |
| --- | --- |
| Ein Client lehnt eine Lizenz ab | Folgen Sie den Client-Prüfungen im [Integrationsleitfaden](INTEGRATION.md#fehlerbehebung); prüfen Sie die Lizenz hier mit demselben öffentlichen Schlüssel, Produkt und derselben HWID. |
| Unerwartet abgelaufen | Das gewählte Datum ist die exklusive Grenze um 00:00 UTC; es ist nicht der letzte erlaubte Tag. |
| Der Import meldet einen Konflikt | Behalten Sie die aktuellen Daten und prüfen Sie die Sicherung; ersetzen Sie nicht versehentlich den privaten Schlüssel eines Produkts. |
| Daten fehlen nach dem Verschieben der ausführbaren Datei oder einer Neuinstallation | Die Daten liegen im Ordner des aktuellen Benutzers für diese Version; importieren Sie die Sicherung in der neuen Installation. |

Die Offline-Ausstellung bietet keinen Fernwiderruf, keine vertrauenswürdige Uhr und keinen Schutz vor verändertem Client-Code. Lesen Sie die [Sicherheitsgrenzen](ARCHITECTURE.md#sicherheitsgrenzen), bevor Sie eine Integrationsstrategie wählen.

[Problem melden](https://github.com/gonzapersonalsite/license-generator-docs/issues) · [Entwicklung unterstützen](https://ko-fi.com/gonzaloapps)

# License Generator

[Español](../es/README.md) · [English](../../README.md) · **Deutsch** · [Português](../pt/README.md) · [中文](../zh/README.md)

Eine Desktop-Anwendung zum Erstellen und Verwalten von Offline-Softwarelizenzen. Jedes Produkt besitzt ein eigenes RSA-Schlüsselpaar; Ihre Anwendung prüft Lizenzen mit dem öffentlichen Schlüssel.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Problem melden](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Entwicklung unterstützen](https://ko-fi.com/gonzaloapps)

- RSA-2048, SHA-256 und PKCS#1 v1.5; PEM-Schlüssel.
- Produkt, Kundenname, optionale Hardwarebindung und optionales Ablaufdatum.
- Signierter **UserDefined**-Text für Funktionen, Tarife oder eigene Metadaten.
- Produktverwaltung, Verlauf und ZIP-Sicherung.
- Oberfläche auf Deutsch, Englisch, Spanisch, brasilianischem Portugiesisch und vereinfachtem Chinesisch.
- Eigenständige Windows-x64- und Linux-x64-Versionen ohne separate .NET-Installation. Linux benötigt passende Desktop-Systembibliotheken.

Erstellen Sie ein Produkt und integrieren Sie `public.pem` in Ihre Anwendung. Verteilen Sie niemals `private.pem`. Ein leeres UserDefined-Feld erhält das bisherige Lizenzformat; zusätzlicher Text benötigt einen Client mit Format-2-Unterstützung.

[Bedienung und Integration](OPERATIONS.md) · [Architektur und Sicherheit](ARCHITECTURE.md) · [C#-Prüfbeispiel](../examples/LicenseVerifier.cs)

Daten liegen im Benutzerprofil, getrennt von der ausführbaren Datei. Das Kopieren auf einen USB-Stick verschiebt keine Schlüssel oder Verlaufsdaten. ZIP-Sicherungen enthalten unverschlüsselte private Schlüssel.

Proprietäre Software gemäß [EULA](../../LICENSE). Generierte Lizenzen und öffentliche Schlüssel dürfen entsprechend diesen Bedingungen mit Ihren eigenen Produkten verteilt werden.

Entwickelt von Gonzalo Martínez García · gonzalo.dev

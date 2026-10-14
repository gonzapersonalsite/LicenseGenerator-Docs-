# Arquitetura e segurança

[Início](README.md) · [Contrato canônico em inglês](../../ARCHITECTURE.md)

Avalonia apresenta a interface; MVVM coordena comandos; serviços cuidam de assinatura, armazenamento, configurações e integração com o desktop. O emissor mantém as chaves privadas; os clientes recebem somente as públicas.

O [contrato canônico](../../ARCHITECTURE.md) define os tipos JSON, os bytes assinados, as versões e a validação. O [exemplo C#](../examples/LicenseVerifier.cs) é compilado e testado com o aplicativo.

- Licenças são JSON UTF-8 em Base64, sem criptografia.
- Assinaturas usam RSA-2048, SHA-256 e PKCS#1 v1.5.
- UserDefined vazio mantém o formato anterior e omite as novas propriedades.
- UserDefined preenchido exige formato 2 e cliente atualizado.
- Versões desconhecidas e combinações incoerentes são rejeitadas. Nunca tente o formato antigo após falha do formato 2.
- O formato antigo rejeita `|` nos campos para evitar interpretações ambíguas.
- A validade termina às **00:00 UTC no início da data informada**. Verifique novamente em cada decisão de acesso.
- HardwareId vazio permite qualquer máquina; caso contrário, exige correspondência exata.

Dados em `<LocalApplicationData>/LicenseGenerator/`: `Keys/`, `Licenses/`, `settings.json` e `Logs/`. Os backups ZIP não são criptografados. A importação adiciona dados ausentes, rejeita conflitos e substitui configurações válidas de forma atômica.

Assinaturas offline não impedem alterações no cliente nem manipulação do relógio. Revogação remota exige mecanismos adicionais. Consulte os [limites de segurança](../../ARCHITECTURE.md#security-boundaries) e o [EULA](../../LICENSE).

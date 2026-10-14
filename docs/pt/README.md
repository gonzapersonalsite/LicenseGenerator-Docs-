# License Generator

[Español](../es/README.md) · [English](../../README.md) · [Deutsch](../de/README.md) · **Português** · [中文](../zh/README.md)

Aplicativo de desktop para emitir, inspecionar e gerenciar licenças de software offline. Cada produto tem seu próprio par de chaves RSA; você pode inspecionar as licenças localmente com uma chave pública confiável e validá-las no seu aplicativo.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Relatar problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoiar o desenvolvimento](https://ko-fi.com/gonzaloapps)

- Assinaturas RSA-2048, SHA-256 e PKCS#1 v1.5; chaves PEM.
- Produto, nome do cliente, vínculo opcional ao hardware e validade opcional.
- Texto **UserDefined** assinado para recursos, planos ou metadados.
- Gerenciamento de produtos, histórico e backup ZIP.
- Inspeção local com chave pública escolhida explicitamente e verificações separadas de assinatura, produto, hardware e validade.
- Interface em português brasileiro, inglês, espanhol, alemão e chinês simplificado.
- Versões autossuficientes para Windows x64 e Linux x64, sem instalar .NET separadamente. Linux precisa das bibliotecas de desktop do sistema.

Crie um produto e integre `public.pem` ao seu aplicativo. Nunca distribua `private.pem`. UserDefined vazio mantém o formato anterior; texto adicional exige um cliente compatível com o formato 2.

[Uso e integração](OPERATIONS.md) · [Arquitetura e segurança](ARCHITECTURE.md) · [Verificador C#](../examples/LicenseVerifier.cs)

Os dados ficam no perfil do usuário, separados do executável. Copiar o programa para um USB não transfere chaves ou histórico. Backups ZIP contêm chaves privadas sem criptografia.

Software proprietário sujeito ao [EULA](../../LICENSE). Licenças geradas e chaves públicas podem ser distribuídas com seus produtos conforme esses termos.

Desenvolvido por Gonzalo Martínez García · gonzalo.dev

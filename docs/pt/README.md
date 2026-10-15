# License Generator

[Español](../es/README.md) · [English](../../README.md) · [Deutsch](../de/README.md) · **Português** · [中文](../zh/README.md)

Uma ferramenta de desktop para emitir, inspecionar e gerenciar licenças de software offline. Crie um par de chaves RSA próprio para cada produto, assine licenças, inspecione-as localmente com uma chave pública confiável e valide-as no seu próprio aplicativo.

[Microsoft Store](https://apps.microsoft.com/detail/9NR38VBCF8JK) · [Downloads](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/releases) · [Relatar um problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoiar o desenvolvimento](https://ko-fi.com/gonzaloapps)

## Recursos

- Assinaturas RSA-2048 com SHA-256 e PKCS#1 v1.5; chaves PEM padrão.
- Produto, nome do cliente, ID de hardware e data de expiração opcional em cada licença assinada.
- Texto **UserDefined** assinado opcional para recursos, planos ou metadados próprios do aplicativo.
- Gerenciamento local de aplicativos, histórico pesquisável e backup e importação em ZIP.
- Inspeção local de licenças com uma chave pública explicitamente confiável e resultados separados de assinatura, produto, hardware e expiração.
- Interface em inglês, espanhol, alemão, português do Brasil e chinês simplificado.
- Compilações autônomas para Windows x64 e Linux x64; os usuários não precisam instalar o .NET. No Linux ainda são necessárias as bibliotecas de desktop do sistema exigidas pelo Avalonia.

## Começar a usar

1. Crie um produto em **Aplicativos**.
2. Obtenha o `public.pem` dele e adicione a verificação de licenças ao seu aplicativo cliente com o [guia de integração](INTEGRATION.md).
3. Digite o nome do cliente, o ID de hardware fornecido pelo seu cliente e uma data de expiração opcional.
4. Deixe UserDefined vazio para o formato de licença existente, ou digite texto personalizado depois de atualizar seu cliente para suportar o formato 2.
5. Gere e entregue o código da licença. Mantenha em sigilo a chave privada e os backups.

UserDefined é texto assinado, não uma regra executável nem um gerenciador de recursos integrado. Seu aplicativo decide o que o conteúdo significa **depois** de verificar a assinatura.

## Documentação

- [Guia de uso](OPERATIONS.md): produtos, emissão, inspeção, backups e seus dados.
- [Guia de integração](INTEGRATION.md): verifique licenças no seu aplicativo, da tela de ativação ao identificador de hardware, com exemplos em C#, Python e Node.js.
- [Contrato de formato e segurança da licença](ARCHITECTURE.md): bytes assinados exatos, regras de verificação e limites de segurança.
- Verificadores de referência: [C#](../examples/LicenseVerifier.cs) com o [protocolo compartilhado](../examples/LicenseProtocol.cs), [Python](../examples/python/license_verifier.py) e [Node.js](../examples/node/license-verifier.mjs), todos verificados com os mesmos [vetores de teste](../examples/test-vectors.json).
- [Registro de alterações](../CHANGELOG.md), em inglês.

As licenças são JSON codificado em Base64, não segredos criptografados. O vínculo ao hardware e a expiração dependem das verificações que seu cliente implementa; licenças offline não impedem a modificação do cliente nem a manipulação do relógio do sistema.

## Dados e licença

As chaves, o histórico, as configurações e um log de diagnóstico ficam em uma pasta de dados do seu usuário do sistema operacional, separada do executável; a [política de privacidade](https://gonzaloapps.es/apps/license-generator/legal/privacy-policy/) (em inglês) informa onde ela fica em cada versão e o que contém. Mover o executável não move os dados. Exporte um backup antes de trocar de computador, desinstalar ou modificar chaves.

O License Generator é software proprietário sujeito ao [EULA](../../LICENSE). As licenças geradas e as chaves públicas podem ser distribuídas com seus próprios aplicativos comerciais ou gratuitos nessas condições, e os exemplos de integração de `docs/examples` podem ser copiados para eles.

Desenvolvido por Gonzalo Martínez García · gonzalo.dev

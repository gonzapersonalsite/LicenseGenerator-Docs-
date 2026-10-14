# Arquitetura e segurança

[Início](README.md) · [Contrato canônico em inglês](../../ARCHITECTURE.md)

Avalonia apresenta a interface; MVVM coordena comandos; serviços cuidam de assinatura, armazenamento, configurações e integração com o desktop. O emissor mantém as chaves privadas; os clientes recebem somente as públicas.

Construir uma página não lê o armazenamento de licenças. A navegação mostra a página antes de aguardar seu carregamento cancelável e mantém a instância para preservar os rascunhos. Os modelos de apresentação executam operações de armazenamento em threads de trabalho e atualizam o estado observável no contexto da interface. Operações duplicadas são impedidas enquanto outra está em andamento. Uma gravação já iniciada termina; editar o formulário invalida o resultado exibido sem perder a licença salva.

O serviço de armazenamento lê produtos e histórico sob o mesmo bloqueio e retorna esses dados em uma instância de `WorkspaceSnapshot`. O painel calcula seus totais com essa leitura consistente e informa as entradas corrompidas ignoradas. As listas mantêm os resultados filtrados e os paginam sem reler o armazenamento.

`AdaptiveGrid` pertence à camada visual: mede os elementos visíveis e adapta as colunas à largura disponível e ao tamanho do texto. Os diálogos de confirmação são modais, focam inicialmente Cancelar e restauram o foco ao fechar. Os modelos de apresentação solicitam a confirmação por `IDialogService`, permitindo testar ambas as respostas.

O [contrato canônico](../../ARCHITECTURE.md) define os tipos JSON, os bytes assinados, as versões e a validação. O [verificador C#](../examples/LicenseVerifier.cs) e seu [protocolo compartilhado](../examples/LicenseProtocol.cs) são compilados e testados com o aplicativo; copie os dois arquivos ao integrar o exemplo.

- Licenças são JSON UTF-8 em Base64, sem criptografia.
- Assinaturas usam RSA-2048, SHA-256 e PKCS#1 v1.5.
- UserDefined vazio mantém o formato anterior e omite as novas propriedades.
- UserDefined preenchido exige formato 2 e cliente atualizado.
- Versões desconhecidas e combinações incoerentes são rejeitadas. Nunca tente o formato antigo após falha do formato 2.
- O formato antigo rejeita `|` nos campos para evitar interpretações ambíguas.
- A validade termina às **00:00 UTC no início da data informada**. Verifique novamente em cada decisão de acesso.
- HardwareId vazio permite qualquer máquina; caso contrário, exige correspondência exata.

O inspetor usa esse mesmo verificador. `TryVerify` mantém seu contrato booleano; `Inspect` retorna uma rejeição sem campos autenticados ou dados autenticados com verificações independentes de produto, hardware e validade. Omitir o HWID esperado deixa uma licença vinculada ao hardware como **não verificada**, nunca como válida. Uma assinatura incorreta não distingue uma chave errada de dados alterados. A chave pública deve ser escolhida explicitamente entre os produtos locais ou colada de uma fonte confiável; nunca é escolhida pelo JSON recebido. A inspeção não grava histórico, não contata servidores nem oferece revogação.

Dados em `<LocalApplicationData>/LicenseGenerator/`: `Keys`, `Licenses`, `settings.json` e `Logs`. Os backups ZIP não são criptografados. A importação adiciona dados ausentes, rejeita conflitos e substitui configurações válidas de forma atômica.

Assinaturas offline não impedem alterações no cliente nem manipulação do relógio. Revogação remota exige mecanismos adicionais. Consulte os [limites de segurança](../../ARCHITECTURE.md#security-boundaries) e o [EULA](../../LICENSE).

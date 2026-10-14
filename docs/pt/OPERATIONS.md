# Uso e integração

[Início](README.md) · [Guia completo em inglês](../../OPERATIONS.md) · [Contrato canônico](../../ARCHITECTURE.md)

1. Crie um produto em Gerenciamento de aplicativos.
2. Integre `public.pem` e a verificação ao seu cliente. Guarde a chave privada.
3. Informe o nome do cliente e, se necessário, o HWID exato fornecido por ele. Vazio significa sem vínculo de hardware.
4. A data opcional vence às **00:00 UTC no início daquele dia**. Para permitir todo o dia 10 de junho, escolha 11 de junho.
5. Deixe UserDefined vazio para clientes antigos. Para clientes com formato 2, use texto como `features=export,reports`.
6. Gere e entregue o código. O histórico guarda a licença emitida.

Seu aplicativo interpreta UserDefined somente após verificar a assinatura. Base64 não é criptografia: não coloque segredos nesse campo nem acrescente texto ao contrato antigo por conta própria.

O [verificador C# testado](../examples/LicenseVerifier.cs) recebe código, chave pública, produto esperado, HWID e hora UTC. Verifique novamente ao carregar a ativação e ao decidir o acesso. Outras linguagens devem implementar o [mesmo contrato](../../ARCHITECTURE.md), sem converter a data assinada para outro fuso.

## Inspecionar uma licença

Abra **Inspecionar licença** e cole o código completo ou abra um registro do histórico. A inspeção não modifica o registro salvo. Escolha explicitamente a chave pública de um produto local ou cole uma chave pública PEM de uma fonte confiável. O conteúdo da licença nunca escolhe a chave. Chaves privadas não são aceitas nessa tela.

Informe o produto esperado e, opcionalmente, o ID exato do hardware. Um HWID esperado vazio significa **não verificado** para uma licença vinculada, sem confirmar seu uso nesse dispositivo. Uma licença sem vínculo não precisa dessa verificação.

Os campos só aparecem após a verificação da assinatura. Produto, hardware e validade são avaliados separadamente; uma licença autêntica pode estar expirada. Uma assinatura diferente pode significar chave incorreta ou dados alterados, sem distinguir a causa. O resultado mostra o instante UTC da verificação. Alterar as entradas limpa o resultado; verifique novamente para outro instante. Nada é enviado ou salvo.

`TryVerify` mantém seu contrato. `LicenseVerifier.Inspect` retorna `Rejected` sem campos ou `Authenticated` com campos assinados e resultados separados. Um HWID esperado `null` ignora a verificação: se houver vínculo, retorna `NotChecked` e `IsValid = false`. `CheckFormat` apenas verifica formato e limites antes de acessar chaves; não autentica. As três APIs compartilham o parser com a interface.

## Backup e migração

Exporte um backup ZIP antes de migrar ou apagar chaves. Ele contém **chaves privadas sem criptografia**. A importação preserva produtos e histórico existentes, adiciona itens ausentes e rejeita chaves ou arquivos de histórico conflitantes. Configurações válidas importadas substituem as atuais.

Os dados pertencem ao perfil do usuário; mover apenas o executável não os transfere. Excluir um produto ou registro de histórico não revoga licenças entregues. O sistema offline também não oferece um relógio confiável.

[Relatar problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoiar o desenvolvimento](https://ko-fi.com/gonzaloapps)

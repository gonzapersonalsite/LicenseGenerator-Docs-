# Guia de uso

[Español](../es/OPERATIONS.md) · [English](../../OPERATIONS.md) · [Deutsch](../de/OPERATIONS.md) · **Português** · [中文](../zh/OPERATIONS.md)

Este guia trata do uso do aplicativo License Generator: criar produtos, emitir e
inspecionar licenças e manter seus dados em segurança. Verificar licenças no seu
próprio aplicativo é assunto do [guia de integração](INTEGRATION.md), e o formato
exato da licença, do [contrato de formato e segurança](ARCHITECTURE.md).

## Criar um produto e emitir uma licença

Crie um produto em **Aplicativos**. O gerador guarda um par de chaves RSA próprio para ele na sua pasta de dados. Seu aplicativo cliente precisa do `public.pem` do produto; nunca distribua `private.pem`.

Para vincular a licença ao hardware, seu cliente calcula um identificador de hardware, mostra esse valor exato ao usuário, e você o digita no campo **ID de Hardware (Opcional)** do gerador. O gerador não escolhe o método de identificação do hardware; o [guia de integração](INTEGRATION.md#escolher-um-identificador-de-hardware) descreve o que um bom método exige. Um HWID vazio de propósito gera uma licença sem vínculo ao hardware.

Selecione o produto, digite o nome de registro e o HWID opcional e, se quiser, escolha uma data de expiração. A licença expira às **00:00 UTC no início da data escolhida**. Para permitir todo o dia 10 de junho, escolha 11 de junho. Deixe a data vazia para uma licença perpétua.

Gere a licença, copie o código resultante e entregue-o ao cliente. O **Histórico** registra as licenças emitidas. Excluir uma entrada do histórico remove apenas o registro local; excluir um produto remove suas chaves e impede emitir novas licenças com elas. Nenhuma das duas operações revoga licenças já entregues. Faça backup das chaves antes de excluir.

## Dados personalizados assinados: UserDefined

Deixe UserDefined vazio para gerar o formato legado. Nesse caso nenhuma propriedade JSON nova é emitida.

Para um cliente que implementa o formato 2, digite texto simples como:

```text
features=export,reports;plan=professional
```

O texto exato entra na assinatura. O gerador não lhe atribui nenhum significado; seu cliente decide o que ele significa **depois da verificação criptográfica** ([guia de integração](INTEGRATION.md#usar-userdefined)). Não coloque segredos neste campo: Base64 é codificação, não criptografia.

Qualquer texto personalizado não vazio usa o formato 2, que um cliente escrito para o formato legado rejeita. Atualize primeiro todos os clientes que devem aceitar a licença.

## Inspecionar uma licença

Abra **Inspecionar licença** e cole um código de licença completo, ou abra uma entrada salva no **Histórico**. A inspeção não altera o registro salvo. Escolha explicitamente a chave pública de um produto local ou cole uma chave pública PEM obtida de uma fonte confiável. O conteúdo da licença nunca escolhe a chave confiável. Esta tela rejeita chaves privadas PEM.

Informe o ID do produto esperado e, opcionalmente, o ID de hardware exato do cliente. Um HWID esperado vazio significa **não verificado** para uma licença vinculada; não confirma que a licença possa ser usada neste dispositivo. Uma licença sem vínculo não precisa de verificação de hardware.

A tela autentica a assinatura RSA antes de mostrar qualquer campo da licença e depois informa separadamente as verificações de produto, hardware e expiração. Licenças autênticas expiradas podem ser inspecionadas, mas continuam expiradas. Uma assinatura que não confere pode indicar outra chave ou dados da licença alterados; ela não distingue essas causas. O resultado mostra a hora UTC da verificação. Alterar os valores informados limpa o resultado; para saber a situação da licença em um momento posterior, verifique de novo. A inspeção não envia nem salva nada.

## Seus dados

O aplicativo guarda as chaves de assinatura, o histórico de licenças, as configurações e um log de diagnóstico em uma pasta de dados que pertence ao seu usuário do sistema operacional, separada do executável. A [política de privacidade](https://gonzaloapps.es/apps/license-generator/legal/privacy-policy/) (em inglês) detalha o que cada arquivo contém e onde fica a pasta na versão da Microsoft Store, na versão portátil para Windows e na versão para Linux. Exporte um backup antes de desinstalar a versão da Microsoft Store: o Windows apaga a pasta de dados junto com ela.

As chaves privadas e os backups **não são criptografados**. Quem obtiver uma chave privada pode emitir licenças que seus aplicativos aceitam.

## Backup e migração

Use **Configurações** para exportar um backup ZIP. Ele contém chaves privadas, histórico e configurações sem criptografia. Guarde-o em um armazenamento que você controla; criptografe-o com a solução de backup que preferir antes de compartilhá-lo ou enviá-lo.

A importação combina produtos e histórico. As chaves e o histórico existentes são preservados. Se um produto importado tiver outras chaves ou se já existir um arquivo de histórico com o mesmo nome e outro conteúdo, a importação falha em vez de sobrescrevê-lo. As configurações importadas válidas substituem as atuais. A estrutura e o tamanho do arquivo ZIP e os pares de chaves RSA são validados antes da importação. Exporte antes um backup do perfil atual se quiser restaurar as configurações dele depois. Uma importação não revoga nem substitui licenças já entregues aos clientes.

Copiar apenas o executável para outro computador não leva as chaves nem o histórico: exporte um backup e importe-o na nova instalação. Na mesma conta do Windows, a versão da Microsoft Store também usa a pasta de dados de uma versão portátil. Ela mostra os produtos e o histórico dessa pasta, e excluir um deles na versão da Store o exclui dessa pasta; o que a versão da Store cria fica na pasta dela, que a versão portátil não vê. Antes de excluir a pasta da versão portátil, exporte um backup na versão da Store e importe-o de novo depois, para manter esses produtos e o histórico na versão da Store.

## Solução de problemas

| Sintoma | Verificação |
| --- | --- |
| Um aplicativo cliente rejeita uma licença | Siga as verificações do cliente no [guia de integração](INTEGRATION.md#solução-de-problemas); inspecione a licença aqui com a mesma chave pública, produto e HWID. |
| Expirou antes do esperado | A data escolhida é o limite exclusivo às 00:00 UTC; não é o último dia permitido. |
| A importação informa um conflito | Mantenha os dados atuais e inspecione o backup; não substitua por acidente a chave privada de um produto. |
| Faltam dados depois de mover o executável ou reinstalar | Os dados estão na pasta de dados do usuário atual para essa versão; importe o backup na nova instalação. |

A emissão offline não oferece revogação remota, relógio confiável nem proteção contra código cliente modificado. Consulte os [limites de segurança](ARCHITECTURE.md#limites-de-segurança) antes de escolher uma política de integração.

[Relatar um problema](https://github.com/gonzapersonalsite/LicenseGenerator-Docs-/issues) · [Apoiar o desenvolvimento](https://ko-fi.com/gonzaloapps)

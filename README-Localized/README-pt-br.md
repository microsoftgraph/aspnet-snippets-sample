# <a name="microsoft-graph-snippets-sample-for-asp.net-4.6"></a>Exemplo de Trechos de Código do Microsoft Graph para ASP.NET 4.6

## <a name="table-of-contents"></a>Sumário

* [Pré-requisitos](#prerequisites)
* [Registrar o aplicativo](#register-the-application)
* [Criar e executar o exemplo](#build-and-run-the-sample)
* [Código da observação](#code-of-note)
* [Perguntas e comentários](#questions-and-comments)
* [Colaboração](#contributing)
* [Recursos adicionais](#additional-resources)

Este exemplo de projeto fornece um repositório de trechos de código que usa o Microsoft Graph para realizar tarefas comuns, como o envio de emails, gerenciamento de grupos e outras atividades diretamente de um aplicativo do ASP.NET MVC. O exemplo usa o [SDK de cliente do Microsoft Graph .NET](https://github.com/microsoftgraph/msgraph-sdk-dotnet) para trabalhar com dados retornados pelo Microsoft Graph. 

O exemplo usa a [Biblioteca de Autenticação da Microsoft (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) para autenticação. O SDK da MSAL fornece recursos para trabalhar com o [ponto de extremidade do Microsoft Azure AD versão 2.0](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview), que permite aos desenvolvedores gravar um único fluxo de código para tratar da autenticação de contas pessoais (Microsoft), corporativas ou de estudantes (Azure Active Directory).

Além disso, o exemplo mostra como solicitar tokens de forma incremental, um recurso compatível com o ponto de extremidade do Microsoft Azure AD versão 2.0. Os usuários permitem um conjunto inicial de escopos de permissão durante a conexão, mas podem permitir outros escopos posteriormente. No caso deste exemplo, qualquer usuário válido pode entrar, mas apenas administradores podem posteriormente permitir os escopos de administrador necessários para determinadas operações.

O exemplo usa o [middleware OWIN do OpenId Connect do ASP.NET](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) para entrar e durante a aquisição de token inicial. O exemplo também implementa o middleware Owin personalizado para compartilhar um código de autorização para tokens de acesso e de atualização fora do fluxo de entrada. O middleware personalizado chama a MSAL para compilar o URI de solicitação de autorização e manipula os redirecionamentos. Para saber mais sobre o consentimento incremental, confira o artigo [Integrar a identidade da Microsoft e o Microsoft Graph em um aplicativo Web usando o OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

 > **Observação** No momento, o SDK da MSAL encontra-se em pré-lançamento e como tal não deve ser usado no código de produção. O middleware personalizado e o cache de token têm limitações que os tornam impróprios para códigos de produção. Por exemplo, o middleware tem uma dependência difícil em cache e o cache é baseado em sessão. O código é usado apenas para fins ilustrativos

## <a name="prerequisites"></a>Pré-requisitos

Este exemplo requer o seguinte:  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * Uma [conta da Microsoft](https://www.outlook.com) ou uma [conta do Office 365 para empresas](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account). Uma conta de administrador do Office 365 é necessária para executar operações de administrador. Inscreva-se para [uma Assinatura de Desenvolvedor do Office 365](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account), que inclui os recursos necessários para que você comece a criar aplicativos.

## <a name="register-the-application"></a>Registrar o aplicativo

1. Entre no [Portal de Registro do Aplicativo](https://apps.dev.microsoft.com/) usando sua conta pessoal ou sua conta comercial ou escolar.

2. Escolha **Adicionar um aplicativo**.

3. Insira um nome para o aplicativo e escolha **Criar aplicativo**. 
    
   A página de registro é exibida, listando as propriedades do seu aplicativo.

4. Copie a ID do Aplicativo. Esse é o identificador exclusivo do aplicativo. 

5. Em **Segredos do Aplicativo**, escolha **Gerar Nova Senha**. Copie a senha da caixa de diálogo **Nova senha gerada**.

   Será preciso inserir os valores da ID do aplicativo e o segredo do aplicativo que você copiou para o exemplo de aplicativo. 

6. Em **Plataformas**, escolha **Adicionar plataforma**.

7. Escolha **Web**.

8. Verifique se a caixa de diálogo **Permitir Fluxo Implícito** está marcada e insira *https://localhost:44300/* como o URI de redirecionamento. 

   A opção **Permitir Fluxo Implícito** habilita o fluxo híbrido. Durante a autenticação, isso permite que o aplicativo receba informações de entrada (o id_token) e artefatos (neste caso, um código de autorização) que o aplicativo pode usar para obter um token de acesso.

9. Escolha **Salvar**.
 
 
## <a name="build-and-run-the-sample"></a>Criar e executar o exemplo

1. Baixe ou clone o Exemplo de Trechos de Código do Microsoft Graph para ASP.NET 4.6.

2. Abra a solução de exemplo no Visual Studio.

3. No arquivo Web.config no diretório raiz, substitua os valores dos espaços reservados **ida:AppId** e **ida:AppSecret** pelos valores copiados durante o registro do aplicativo.

4. Pressione F5 para criar e executar o exemplo. Isso restaurará as dependências do pacote NuGet e abrirá o aplicativo.

   >Caso receba mensagens de erro durante a instalação de pacotes, verifique se o caminho para o local onde você colocou a solução não é muito longo ou extenso. Talvez você consiga resolver esse problema colocando a solução junto à raiz da unidade.

5. Entre com sua conta pessoal (MSA) ou com sua conta comercial ou escolar e conceda as permissões solicitadas. 

6. Escolha uma categoria de trechos de código, como Usuários, Arquivos ou Email. 

7. Escolha uma operação que você queira executar. Observe o seguinte:
  - As operações que exigem um argumento (como ID) são desativadas até você executar um trecho de código que permita escolher uma entidade. 

  - Alguns trechos de código (marcados como *somente administradores*) exigem escopos de permissão comerciais que só podem ser concedidos por um administrador. Para executar esses trechos de código, entre como administrador e use o link na guia *Escopos de administrador* para permitir escopos em nível de administrador. Essa guia não está disponível para usuários que estão conectados com contas pessoais.
   
  - Se você estiver conectado com uma conta pessoal, os trechos de código sem suporte para contas da Microsoft serão desabilitados.
   
As informações de resposta são exibidas na parte inferior da página.

### <a name="how-the-sample-affects-your-account-data"></a>Como o exemplo afeta os dados da conta

Este exemplo cria, atualiza e exclui entidades e dados (como usuários ou arquivos). Dependendo do modo como você a usar, **poderá editar ou excluir dados e entidades reais** e deixar artefatos de dados. 

Para usar o exemplo sem modificar seus dados reais da conta, certifique-se de executar a atualização e excluir operações somente em entidades que são criadas pelo exemplo. 


## <a name="code-of-note"></a>Código da observação

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). Autentica o usuário atual e inicializa o cache de token do exemplo.

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). Armazena as informações de token do usuário. Você pode substituir pelo seu próprio cache de token personalizado. Saiba mais em [Armazenamento de tokens de acesso em cache em um aplicativo de vários locatários](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/).

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs). Implementa a interface IAuthProvider local e obtém acesso a um token usando o método **AcquireTokenSilentAsync**. Isso pode ser substituído pelo seu próprio provedor de autorização. 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs). Inicializa o **GraphServiceClient**, na [Biblioteca do Cliente .NET para Microsoft Graph](https://github.com/microsoftgraph/msgraph-sdk-dotnet), que é usado para interagir com o Microsoft Graph.

- Os seguintes controladores contêm métodos que usam o **GraphServiceClient** para criar e enviar chamadas para o serviço do Microsoft Graph e processar a resposta.
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- Os modos de exibição a seguir contêm a interface do usuário do exemplo.  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- Os arquivos a seguir contêm os modelos de exibição e exibição parcial usados para analisar e exibir dados do Microsoft Graph como objetos genéricos (para os fins deste exemplo). 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Os arquivos a seguir contêm o código usado para implementar o consentimento incremental. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)

## <a name="questions-and-comments"></a>Perguntas e comentários

Gostaríamos de saber sua opinião sobre este exemplo. Você pode nos enviar suas perguntas e sugestões por meio da seção [Issues](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) deste repositório.

Seus comentários são importantes para nós. Junte-se a nós na página [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Marque suas perguntas com [MicrosoftGraph].

## <a name="contributing"></a>Colaboração

Se quiser contribuir para esse exemplo, confira [CONTRIBUTING.md](CONTRIBUTING.md).

Este projeto adotou o [Código de Conduta do Código Aberto da Microsoft](https://opensource.microsoft.com/codeofconduct/). Para saber mais, confira as [Perguntas frequentes do Código de Conduta](https://opensource.microsoft.com/codeofconduct/faq/) ou contate [opencode@microsoft.com](mailto:opencode@microsoft.com) se tiver outras dúvidas ou comentários. 

## <a name="additional-resources"></a>Recursos adicionais

- [Outros exemplos de trechos de código do Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Visão geral do Microsoft Graph](http://graph.microsoft.io)
- [Exemplos de código para desenvolvedores do Office](http://dev.office.com/code-samples)
- [Centro de Desenvolvimento do Office](http://dev.office.com/)

## <a name="copyright"></a>Direitos autorais
Copyright © 2016 Microsoft. Todos os direitos reservados.

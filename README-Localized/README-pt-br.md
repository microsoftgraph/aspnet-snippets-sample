# Exemplo de Trechos de Código do Microsoft Graph para ASP.NET 4.6
<a id="microsoft-graph-snippets-sample-for-aspnet-46" class="xliff"></a>

## Sumário
<a id="table-of-contents" class="xliff"></a>

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

## Observação importante sobre a Visualização da MSAL
<a id="important-note-about-the-msal-preview" class="xliff"></a>

Esta biblioteca é adequada para uso em um ambiente de produção. Ela recebe o mesmo suporte de nível de produção que fornecemos às nossas bibliotecas de produção atuais. Durante a visualização, podemos fazer alterações na API, no formato de cache interno e em outros mecanismos desta biblioteca, que você será solicitado a implementar juntamente com correções de bugs ou melhorias de recursos. Isso pode impactar seu aplicativo. Por exemplo, uma alteração no formato de cache pode impactar seus usuários, exigindo que eles entrem novamente. Uma alteração na API pode requerer que você atualize seu código. Quando fornecermos a versão de Disponibilidade Geral, você será solicitado a atualizar a versão de Disponibilidade Geral no prazo de seis meses, pois os aplicativos escritos usando uma versão de visualização da biblioteca podem não funcionar mais.


## Pré-requisitos
<a id="prerequisites" class="xliff"></a>

Este exemplo requer o seguinte:  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * Uma [conta da Microsoft](https://www.outlook.com) ou uma [conta do Office 365 para empresas](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account). Uma conta de administrador do Office 365 é necessária para executar operações de administrador. Inscreva-se para [uma Assinatura de Desenvolvedor do Office 365](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account), que inclui os recursos necessários para que você comece a criar aplicativos.

## Registrar o aplicativo
<a id="register-the-application" class="xliff"></a>

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
 
 
## Criar e executar o exemplo
<a id="build-and-run-the-sample" class="xliff"></a>

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

### Como o exemplo afeta os dados da conta
<a id="how-the-sample-affects-your-account-data" class="xliff"></a>

Este exemplo cria, atualiza e exclui entidades e dados (como usuários ou arquivos). Dependendo do modo como você a usar, **poderá editar ou excluir dados e entidades reais** e deixar artefatos de dados. 

Para usar o exemplo sem modificar seus dados reais da conta, certifique-se de executar a atualização e excluir operações somente em entidades que são criadas pelo exemplo. 


## Código da observação
<a id="code-of-note" class="xliff"></a>

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

- Os arquivos a seguir contêm o código usado para fornecer suporte ao consentimento incremental. Neste exemplo, os usuários são solicitados a consentir um conjunto inicial de permissões durante a entrada e a consentir permissões de administrador separadamente. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs). Middleware personalizado que resgata um código de autorização para tokens de acesso e de atualização fora do fluxo de entrada. Confira https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2 para saber mais sobre como implementar o consentimento incremental.

## Perguntas e comentários
<a id="questions-and-comments" class="xliff"></a>

Gostaríamos de saber sua opinião sobre este exemplo. Você pode nos enviar suas perguntas e sugestões por meio da seção [Issues](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) deste repositório.

Seus comentários são importantes para nós. Junte-se a nós na página [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Marque suas perguntas com [MicrosoftGraph].

## Colaboração
<a id="contributing" class="xliff"></a>

Se quiser contribuir para esse exemplo, confira [CONTRIBUTING.md](CONTRIBUTING.md).

Este projeto adotou o [Código de Conduta do Código Aberto da Microsoft](https://opensource.microsoft.com/codeofconduct/). Para saber mais, confira as [Perguntas frequentes do Código de Conduta](https://opensource.microsoft.com/codeofconduct/faq/) ou contate [opencode@microsoft.com](mailto:opencode@microsoft.com) se tiver outras dúvidas ou comentários. 

## Recursos adicionais
<a id="additional-resources" class="xliff"></a>

- [Outros exemplos de trechos de código do Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Visão geral do Microsoft Graph](http://graph.microsoft.io)
- [Exemplos de código para desenvolvedores do Office](http://dev.office.com/code-samples)
- [Centro de Desenvolvimento do Office](http://dev.office.com/)

## Direitos autorais
<a id="copyright" class="xliff"></a>
Copyright © 2016 Microsoft. Todos os direitos reservados.

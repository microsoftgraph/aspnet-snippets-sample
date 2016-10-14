# <a name="microsoft-graph-snippets-sample-for-asp.net-4.6"></a>适用于 ASP.NET 4.6 的 Microsoft Graph 代码段示例

## <a name="table-of-contents"></a>目录

* [先决条件](#prerequisites)
* [注册应用程序](#register-the-application)
* [生成和运行示例](#build-and-run-the-sample)
* [注释代码](#code-of-note)
* [问题和意见](#questions-and-comments)
* [参与](#contributing)
* [其他资源](#additional-resources)

此示例项目提供使用 Microsoft Graph 执行常见任务的代码段存储库，例如发送电子邮件、管理组和 ASP.NET MVC 应用内的其他活动。它使用 [Microsoft Graph .NET Client SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet) 以结合使用由 Microsoft Graph 返回的数据。 

此示例使用 [Microsoft 身份验证库 (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) 进行身份验证。MSAL SDK 提供可使用 [Azure AD v2.0 终结点](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview)的功能，借助该终结点，开发人员可以编写单个代码流来处理对工作或学校 (Azure Active Directory) 帐户或个人 (Microsoft) 帐户的身份验证。

此外，该示例演示如何以增量方式请求令牌，这是受 Azure AD v2.0 终结点支持的功能。用户可在登录过程中同意一组初始权限范围，还可在稍后同意其他范围。在本示例中，任何有效的用户均可登录，但只有管理员可在稍后同意某些操作所需的管理员级别范围。

此示例在登录和初始令牌获取过程中使用 [ASP.NET OpenId Connect OWIN 中间件](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/)。此示例还实现自定义 Owin 中间件来交换登录流之外的访问令牌和刷新令牌的授权代码。自定义中间件调用 MSAL 来生成授权请求 URI 并处理重定向。有关增量同意的详细信息，请参阅[使用 OpenID Connect 将 Microsoft 标识和 Microsoft Graph 集成到 Web 应用程序中](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2)。

 > **注意** MSAL SDK 目前处于预发布状态，因此，不应该在生产代码中使用。自定义中间件和令牌缓存有局限性，使其不适合用于生产代码。例如，中间件在缓存上具有硬依赖项，并且缓存是基于会话的。在此处使用该代码仅用于说明目的。

## <a name="prerequisites"></a>先决条件

此示例需要以下各项：  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * [Microsoft 帐户](https://www.outlook.com)或 [Office 365 商业版帐户](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)。需要 Office 365 管理员帐户才能运行管理员级别的操作。可以注册 [Office 365 开发人员订阅](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)，其中包含你开始生成应用所需的资源。

## <a name="register-the-application"></a>注册应用程序

1. 使用个人或工作或学校帐户登录到 [应用注册门户](https://apps.dev.microsoft.com/)。

2. 选择“**添加应用**”。

3. 输入应用的名称，并选择“**创建应用程序**”。 
    
   将显示注册页，其中列出应用的属性。

4. 复制应用程序 ID。这是应用的唯一标识符。 

5. 在“**应用程序密码**”下，选择“**生成新密码**”。从“**生成的新密码**”对话框复制密码。

   需要输入你复制到示例应用中的应用 ID 和应用密码值。 

6. 在“**平台**”下，选择“**添加平台**”。

7. 选择“**Web**”。

8. 请确保已选中“**允许隐式流**”复选框，输入 *https://localhost:44300/* 作为重定向 URI。 

   “**允许隐式流**”选项启用混合流。在身份验证过程中，这可使应用同时接收登录信息 (id_token) 以及应用可用来获取访问令牌的项目（在这种情况下，项目为授权代码）。

9. 选择“**保存**”。
 
 
## <a name="build-and-run-the-sample"></a>生成和运行示例

1. 下载或克隆适用于 ASP.NET 4.6 的 Microsoft Graph 代码段示例。

2. 在 Visual Studio 中打开示例解决方案。

3. 在根目录的 Web.config 文件中，使用你在应用注册过程中复制的值来替换 **ida:AppId** 和 **ida:AppSecret** 占位符值。

4. 按 F5 生成和运行此示例。这将还原 NuGet 包依赖项并打开该应用。

   >如果在安装包时出现任何错误，请确保你放置该解决方案的本地路径并未太长/太深。将解决方案移动到更接近驱动器根目录的位置可能可以解决此问题。

5. 使用你的个人帐户 (MSA)、工作或学校帐户登录，并授予所请求的权限。 

6. 选择一个代码段类别，比如用户、文件或邮件。 

7. 选择你想要运行的操作。请注意以下事项：
  - 在运行代码段前要求禁用参数（如 ID）的操作允许你选择一个实体。 

  - 有些代码段（标记为*仅管理员*）只需由管理员授予的商业权限范围。若要运行这些代码段，需要以管理员身份登录，然后使用“*管理员范围*”选项卡上的链接来同意管理员级别的范围。此选项卡不适用于使用个人帐户登录的用户。
   
  - 如果你使用个人帐户登录，则会禁用 Microsoft 帐户不支持的代码段。
   
响应信息将在页面底部显示。

### <a name="how-the-sample-affects-your-account-data"></a>示例如何影响你的帐户数据

本示例创建、更新和删除实体和数据（如用户或文件）。具体取决于你使用本示例的方式，**可以编辑或删除实际的实体和数据**并保留数据项目。 

若要在不修改实际帐户数据的情况下使用本示例，请确保仅在此示例创建的实体上执行更新和删除操作。 


## <a name="code-of-note"></a>注释代码

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs).对当前用户进行身份验证并初始化该示例的令牌缓存。

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs).存储用户的令牌信息。可以使用你自己的自定义令牌缓存来替换此信息。从[在多租户应用程序中缓存访问令牌](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/)了解详细信息。

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs)。实现本地 IAuthProvider 接口，并通过使用 **AcquireTokenSilentAsync** 方法获取一个访问令牌。可以使用你自己的授权提供程序来替换此方法。 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs)。初始化来自用于与 Microsoft Graph 交互的 [Microsoft Graph .NET 客户端库](https://github.com/microsoftgraph/msgraph-sdk-dotnet)中的 **GraphServiceClient**。

- 以下控制器包含使用 **GraphServiceClient** 生成调用并发送到 Microsoft Graph 服务并处理响应的方法。
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- 以下视图包含示例的 UI。  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- 以下文件包含用于分析 Microsoft Graph 数据并将其显示为一般对象（出于本示例中的目的）的视图模型和分部视图。 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- 以下文件包含用于实现增量同意的代码。 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)

## <a name="questions-and-comments"></a>问题和意见

我们乐意倾听你有关此示例的反馈。你可以在该存储库中的[问题](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) 部分将问题和建议发送给我们。

你的反馈对我们意义重大。请在 [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph) 上与我们联系。使用 [MicrosoftGraph] 标记出你的问题。

## <a name="contributing"></a>参与

如果想要参与本示例，请参阅 [CONTRIBUTING.md](CONTRIBUTING.md)。

此项目采用 [Microsoft 开源行为准则](https://opensource.microsoft.com/codeofconduct/)。有关详细信息，请参阅 [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)（行为准则常见问题解答），有任何其他问题或意见，也可联系 [opencode@microsoft.com](mailto:opencode@microsoft.com)。 

## <a name="additional-resources"></a>其他资源

- [其他 Microsoft Graph 代码段示例](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Microsoft Graph 概述](http://graph.microsoft.io)
- [Office 开发人员代码示例](http://dev.office.com/code-samples)
- [Office 开发人员中心](http://dev.office.com/)

## <a name="copyright"></a>版权
版权所有 (c) 2016 Microsoft。保留所有权利。

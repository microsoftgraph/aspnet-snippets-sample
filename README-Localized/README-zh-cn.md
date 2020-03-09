---
page_type: sample
products:
- office-365
- office-outlook
- ms-graph
languages:
- csharp
- aspx
description: "此示例使用 Microsoft Graph .NET 客户端库来处理数据，并使用 Microsoft 身份验证库 (MSAL) 在Azure AD v2.0 终结点上进行身份验证。"
extensions:
  contentType: samples 
  technologies:
  - Microsoft Graph
  services:
  - Office 365
  - Outlook
  - Groups
  createdDate: 8/4/2016 10:31:51 AM
---
# 适用于 ASP.NET 4.6 的 Microsoft Graph 代码段示例

## 目录

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

> 此示例使用 ASP.NET MVC 4.6。有关使用 ASP.NET Core 的示例，请参阅以下两个示例之一： - [ASP.NET Core 2.1 的 Microsoft Graph 连接示例](https://github.com/microsoftgraph/aspnetcore-connect-sample) - [使 Web 应用能够登录用户并使用 Microsoft 标识平台为开发人员调用 API](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2)

## 有关 MSAL 预览版的重要说明

此库适用于生产环境。我们为此库提供的生产级支持与为当前生产库提供的支持相同。在预览期间，我们可能会更改 API、内部缓存格式和此库的其他机制，必须接受这些更改以及 bug 修复或功能改进。这可能会影响应用。例如，缓存格式更改可能会对用户造成影响，如要求用户重新登录。API 更改可能会要求更新代码。在我们提供通用版后，必须在 6 个月内更新到通用版，因为使用预览版库编写的应用可能不再可用。

## 先决条件

此示例要求如下：  

  * [Visual Studio](https://www.visualstudio.com/en-us/downloads) 
  * [Microsoft 帐户](https://www.outlook.com)或 [Office 365 商业版帐户](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)。需要 Office 365 管理员帐户才能运行管理员级别的操作。可以注册 [Office 365 开发人员订阅](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)，其中包含开始生成应用所需的资源。

## 注册 Web 应用

### 选择要在其中创建应用程序的 Azure AD 租户

第一步需要执行以下操作:

1. 使用工作/学校帐户或 Microsoft 个人帐户登录到 [Azure 门户](https://portal.azure.com)。
1. 如果你的帐户存在于多个 Azure AD 租户中，请在页面顶部菜单的右上角选择你的配置文件，然后**切换目录**。将门户会话更改为所需的 Azure AD 租户。

### 注册应用

1. 导航到“面向开发人员的 Microsoft 标识平台”[应用注册](https://go.microsoft.com/fwlink/?linkid=2083908)页面。
1. 选择“新注册”****。
1. 出现“注册应用程序页”****后，输入应用程序的注册信息：
   - 在“名称”****部分输入一个会显示给应用用户的有意义的应用程序名称。
   - 将“受支持的帐户类型”****更改为“任何组织目录中的帐户和 Microsoft 个人帐户”（例如，Skype、Xbox、Outlook.com）****。
     > 请注意，有多个重定向 URI。成功创建应用后，稍后需要从“身份验证”****选项卡中添加这些更新。
1. 选择“注册”****以创建应用程序。
1. 在应用的“**概述**”页上，查找“**应用程序(客户端) ID**”值，并稍后记录下来。你将需要它来为此项目配置 Visual Studio 配置文件。
1. 在应用的“概览”页中，选择“身份验证”****部分。
   - 在“重定向 URI”部分中，选择组合框中的“Web”****，然后输入以下重定向 URI。
       - `https://localhost:44300/`
       - `https://localhost:44300/signin-oidc`
   - 在“高级设置****”部分，将“注销 URL”****设置为“https://localhost:44300/signout-oidc”``
   - 在“高级设置**** | 隐式授予”****部分，请检查“ID 标记”****，因为此示例需要启用[隐式授予流](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow)以登录用户，并调用 API。
1. 选择“保存”****。
1. 在“证书 & 密钥”****页面的“客户端密钥”****部分中，选择“新建客户端密码”****：
   - 键入密钥说明（例如应用机密``），
   - 选择“1 年内”****、“2 年内”****或“永不过期”****的一个密钥持续时间。
   - 按“添加”****按钮时，将显示注册表项值。将值复制并保存在安全的位置。
   - 稍后将需要此密钥来配置 Visual Studio 中的项目。此键值将不再显示，任何其他方式也无法将其检索，因此在 Azure 门户中看到该键值后立即进行录制。
 
## 生成和运行示例

1. 下载或克隆适用于 ASP.NET 4.6 的 Microsoft Graph 代码段示例。

2. 在 Visual Studio 中打开示例解决方案。

3. 在根目录的 Web.config 文件中，使用你在应用注册过程中复制的值来替换 **ida:AppId** 和 **ida:AppSecret** 占位符值。

4. 按 F5 生成和运行此示例。这将还原 NuGet 包依赖项并打开该应用。

   >如果在安装包时出现任何错误，请确保你放置该解决方案的本地路径并未太长/太深。将解决方案移动到更接近驱动器根目录的位置可能可以解决此问题。

5. 使用你的个人帐户 (MSA)、工作或学校帐户登录，并授予所请求的权限。 

6. 选择一个代码段类别，比如用户、文件或邮件。 

7. 选择你想要运行的操作。请注意以下事项：
  - 在运行允许你选择实体的代码段之前，将禁用需要参数（如 ID）的操作。 
  - 某些代码段（标记为 *仅管理员*）需要只能由管理员授予的商业权限范围。若要运行这些代码段，需要以管理员身份登录 Azure 门户。然后，使用应用注册中的“API 权限”**部分，以同意管理员级别的范围。此选项卡不适用于使用个人帐户登录的用户。
  - 如果你使用个人帐户登录，则会禁用 Microsoft 帐户不支持的代码段。
   
响应信息将在页面底部显示。

### 示例如何影响你的帐户数据

本示例创建、更新和删除实体和数据（如用户或文件）。具体取决于你使用本示例的方式，**可以编辑或删除实际的实体和数据**并保留数据项目。 

若要在不修改实际帐户数据的情况下使用本示例，请确保仅在此示例创建的实体上执行更新和删除操作。 


## 注释代码

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs)。对当前用户进行身份验证，并初始化此示例的令牌缓存。

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs)。存储用户的令牌信息。可以使用你自己的自定义令牌缓存来替换此信息。从[多租户应用程序中缓存访问令牌](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/)了解详细信息。

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs)。实现本地 IAuthProvider 接口，并通过使用 **AcquireTokenSilentAsync** 方法获取一个访问令牌。可以使用你自己的授权提供程序来替换此方法。 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs)。初始化来自用于与 Microsoft Graph 交互的 **Microsoft Graph .NET 客户端库**中的 [GraphServiceClient](https://github.com/microsoftgraph/msgraph-sdk-dotnet)。

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
  - [\_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- 以下文件包含用于支持增量同意的代码。对于此示例，系统将分别提示用户同意登录期间的初始权限集和管理权限。 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)。自定义中间件兑现登录流之外的访问令牌和刷新令牌的授权代码。有关实现增量同意的详细信息，请参阅 Https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2。

## 问题和意见

我们乐意倾听你对此示例的反馈。你可以在该存储库中的[问题](https://github.com/microsoftgraph/aspnet-snippets-sample/issues)部分将问题和建议发送给我们。

我们非常重视你的反馈意见。请在[堆栈溢出](http://stackoverflow.com/questions/tagged/microsoftgraph)上与我们联系。使用 \[MicrosoftGraph] 标记出你的问题。

## 参与

如果想要参与本示例，请参阅 [CONTRIBUTING.md](CONTRIBUTING.md)。

此项目已采用 [Microsoft 开放源代码行为准则](https://opensource.microsoft.com/codeofconduct/)。有关详细信息，请参阅[行为准则常见问题解答](https://opensource.microsoft.com/codeofconduct/faq/)。如有其他任何问题或意见，也可联系 [opencode@microsoft.com](mailto:opencode@microsoft.com)。 

## 其他资源

- [其他 Microsoft Graph 代码段示例](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Microsoft Graph 概述](http://graph.microsoft.io)
- [Office 开发人员代码示例](http://dev.office.com/code-samples)
- [Office 开发人员中心](http://dev.office.com/)

## 版权信息
版权所有 (c) 2016 Microsoft。保留所有权利。

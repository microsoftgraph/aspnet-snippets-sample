# Microsoft Graph 程式碼片段範例 (適用於 ASP.NET 4.6)

## 目錄

* [必要條件](#必要條件)
* [註冊應用程式](#註冊應用程式)
* [建置及執行範例](#建置及執行範例)
* [附註的程式碼](#附註的程式碼)
* [問題和建議](#問題和建議)
* [參與](#參與)
* [其他資源](#其他資源)

此範例提供程式碼片段的儲存機制，使用 Microsoft Graph 以執行一般工作，例如傳送郵件、管理群組及其他 ASP.NET MVC 應用程式內的活動。 它會使用 [Microsoft Graph.NET 用戶端 SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet)，使用 Microsoft Graph 所傳回的資料。 

範例會使用 [Microsoft 驗證程式庫 (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) 進行驗證。 MSAL SDK 提供功能以使用 [v2.0 端點](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview)，可讓開發人員撰寫單一程式碼流程，控制工作或學校 (Azure Active Directory) 和個人 (Microsoft) 帳戶的驗證。

此外，此範例顯示如何以累加方式要求權杖 - v2.0 端點所支援的功能。 使用者在登入期間同意一組初始的權限範圍，但是稍後可以同意其他範圍。 在這個範例中，任何有效的使用者都可以登入，但是系統管理員稍後可以同意特定作業所需的系統管理層級範圍。

本範例使用 [ASP.NET OpenId Connect OWIN 中介軟體](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/)，以及在初始權杖擷取期間使用。 這個範例也會實作自訂的 Owin 中介軟體，交換登入流程外的存取和重新整理權杖的授權程式碼。 自訂中介軟體會呼叫 MSAL 以建置授權要求 URI 並且處理重新導向。 若要深入了解增量的同意，請參閱[使用 OpenID Connect 將 Microsoft 身分識別與 Microsoft Graph 整合至 Web 應用程式](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2)。

 > **附註** MSAL SDK 目前是發行前版本，因此不應該用於實際執行程式碼。 自訂中介軟體和權杖快取有限制，使它們不適合實際執行程式碼。 例如，中介軟體在快取上具有硬式相依性，而快取是以工作階段為基礎。 程式碼在這裡僅供說明目的使用。

## 必要條件

此範例需要下列項目：  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * [Microsoft 帳戶](https://www.outlook.com)或[ 商務用 Office 365 帳戶](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)。 需要 Office 365 系統管理員帳戶，才能執行系統管理層級的作業。 您可以註冊 [Office 365 開發人員訂用帳戶](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)，其中包含開始建置應用程式所需的資源。

## 註冊應用程式

1. 使用您的個人或工作或學校帳戶登入[應用程式註冊入口網站](https://apps.dev.microsoft.com/)。

2. 選擇 [新增應用程式]。

3. 為應用程式輸入名稱，然後選擇 [建立應用程式]。 
    
   [註冊] 頁面隨即顯示，列出您的應用程式的屬性。

4. 複製應用程式 ID。 這是您的應用程式的唯一識別碼。 

5. 在 [應用程式密碼] 底下，選擇 [產生新密碼] 從 [產生的新密碼] 對話方塊中複製密碼。

   您必須輸入您複製到範例應用程式的應用程式 ID 和應用程式密碼值。 

6. 在 [平台] 底下，選擇 [新增平台]。

7. 選擇 [Web]。

8. 請確定已選取 [允許隱含的流程] 核取方塊，然後輸入 *https://localhost:44300/* 做為重新導向 URI。 

   [允許隱含的流程] 選項會啟用混合式流程。 在驗證期間，這可讓應用程式收到登入資訊 (id_token) 和成品 (在這種情況下，是授權程式碼)，應用程式可以用來取得存取權杖。

9. 選擇 [儲存]。
 
 
## 建置及執行範例

1. 下載或複製 Microsoft Graph 程式碼片段範例 (適用於 ASP.NET 4.6)。

2. 在 Visual Studio 中開啟範例解決方案。

3. 在根目錄的 Web.config 檔案中，將 **ida:AppId** 和 **ida:AppSecret** 預留位置值取代為您在應用程式註冊期間複製的值。

4. 按 F5 以建置及執行範例。 這樣會還原 NuGet 封裝相依性，並開啟應用程式。

   >如果您在安裝封裝時看到任何錯誤，請確定您放置解決方案的本機路徑不會太長/太深。 將解決方案移靠近您的磁碟機根目錄可解決這個問題。

5. 登入您的個人帳戶 (MSA) 或工作或學校帳戶，並授與要求的權限。 

6. 選擇程式碼片段類別，例如使用者、檔案或郵件。 

7. 選擇您想要執行的作業。 注意下列事項：
  - 需要引數 (例如 ID) 的作業已停用，直到您執行程式碼片段，讓您選取實體。 

  - 某些程式碼片段 (標示為*僅限系統管理員*) 需要商業權限範圍，只能由系統管理員授與。 若要執行這些程式碼片段，您必須以系統管理員的身分登入，然後使用 [系統管理範圍]** 索引標籤上的連結，同意系統管理層級範圍。 此索引標籤不適用於以個人帳戶登入的使用者。
   
  - 如果您使用個人帳戶登入，程式碼片段不受支援，因為 Microsoft 帳戶已停用..
   
回應資訊會顯示在頁面底部。

### 範例如何影響帳戶資料

這個範例會建立、更新和刪除實體和資料 (例如使用者或檔案)。 根據您使用它的方式，**您可能編輯或刪除實際的實體和資料**並保留資料成品。 

若要使用範例，而不修改您的實際帳戶資料，請務必執行更新，並只刪除範例所建立的實體的作業。 


## 附註的程式碼

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). 驗證目前使用者，並初始化範例的權杖快取。

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). 儲存使用者的權杖資訊。 您可以將這個項目取代為您自己的自訂權杖快取。 在[多租用戶應用程式中的快取存取權杖](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/)中深入了解。

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs). 實作本機 IAuthProvider 介面，並取得存取權杖，方法是使用 **AcquireTokenSilentAsync** 方法。 您可以將這個項目取代為您自己的授權提供者。 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs). 從 [Microsoft Graph.NET 用戶端程式庫](https://github.com/microsoftgraph/msgraph-sdk-dotnet)初始化 **GraphServiceClient**用來與 Microsoft Graph 互動。

- 下列控制站包含方法，該方法使用 **GraphServiceClient** 以建置並傳送呼叫至 Microsoft Graph 服務，並且處理回應。
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- 下列檢視包含範例的 UI。  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- 下列檔案包含檢視模型和部分檢視，用來剖析和顯示 Microsoft Graph 資料為泛型物件 (針對此範例的目的)。 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- 下列檔案包含用來實作遞增同意的程式碼。 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)

## 問題和建議

我們很樂於收到您對於此範例的意見反應。 您可以在此儲存機制的[問題](https://github.com/microsoftgraph/aspnet-snippets-sample/issues)區段中，將您的問題及建議傳送給我們。

我們很重視您的意見。 請透過 [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph) 與我們連絡。 以 [MicrosoftGraph] 標記您的問題。

## 參與

如果您想要參與這個範例，請參閱 [CONTRIBUTING.md](CONTRIBUTING.md)。

此專案已採用 [Microsoft 開放原始碼執行](https://opensource.microsoft.com/codeofconduct/)。如需詳細資訊，請參閱[程式碼執行常見問題集](https://opensource.microsoft.com/codeofconduct/faq/)，如果有其他問題或意見，請連絡 [opencode@microsoft.com](mailto:opencode@microsoft.com)。 

## 其他資源

- [其他 Microsoft Graph 程式碼片段範例](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Microsoft Graph 概觀](http://graph.microsoft.io)
- [Office 開發人員程式碼範例](http://dev.office.com/code-samples)
- [Office 開發中心](http://dev.office.com/)

## 著作權
Copyright (c) 2016 Microsoft.著作權所有，並保留一切權利。
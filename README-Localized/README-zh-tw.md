---
page_type: sample
products:
- office-365
- office-outlook
- ms-graph
languages:
- csharp
- aspx
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
# Microsoft Graph 程式碼片段範例 (適用於 ASP.NET 4.6)

## 目錄

* [必要條件](#prerequisites)
* [註冊應用程式](#register-the-application)
* [建置及執行範例](#build-and-run-the-sample)
* [附註的程式碼](#code-of-note)
* [問題與意見](#questions-and-comments)
* [參與](#contributing)
* [其他資源](#additional-resources)

此範例專案提供程式碼片段的儲存機制，可使用 Microsoft Graph 執行一般工作，例如傳送電子郵件、管理群組及其他 ASP.NET MVC 應用程式內的活動。它會使用 [Microsoft Graph.NET 用戶端 SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet) 處理 Microsoft Graph 所傳回的資料。 

此範例會使用 [Microsoft Authentication Library (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) 進行驗證。MSAL SDK 會提供使用 [Azure AD v2.0 端點](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview)的功能，此端點可讓開發人員撰寫能夠處理公司或學校 (Azure Active Directory) 和個人 (Microsoft) 帳戶驗證的單一程式碼流程。

此外，此範例會顯示如何以累加方式要求權杖，這是 Azure AD v2.0 端點支援的一個功能。使用者在登入期間同意一組初始的權限範圍，但是稍後可以同意其他範圍。在這個範例中，任何有效的使用者都可以登入，但是系統管理員稍後可以同意特定作業所需的系統管理層級範圍。

此範例使用 [ASP.NET OpenId Connect OWIN 中介軟體](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/)，在初始權杖擷取期間登入。這個範例也會實作自訂的 Owin 中介軟體，以交換登入流程外的存取和重新整理權杖的授權程式碼。自訂中介軟體會呼叫 MSAL 以建置授權要求 URI 並且處理重新導向。若要深入了解增量同意，請參閱[使用 OpenID Connect 將 Microsoft 身分識別與 Microsoft Graph 整合至 Web 應用程式](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2)。

> 本範例使用 ASP.NET MVC 4.6。如需使用 ASP.NET Core 的範例，請參閱以下其中一個範例：
- [適用於 ASP.NET Core 2.1 的 Microsoft Graph Connect 範例](https://github.com/microsoftgraph/aspnetcore-connect-sample) (英文)
- [透過開發人員適用的 Microsoft 身分識別平台，讓 Web 應用程式登入使用者並呼叫 API](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2) (英文)

## MSAL 預覽相關的重要事項

這個程式庫適合在實際執行環境中使用。我們為我們目前的實際執行程式庫提供與此程式庫相同的實際執行層級支援。在預覽期間，我們可能會變更此程式庫的 API、內部快取格式和其他機制，您將必須對此程式庫進行錯誤修復或增強功能。這可能會影響您的應用程式。舉例來說，變更快取格式可能會影響您的使用者，例如需要使用者重新登入。API 變更可能需要更新您的程式碼。當我們提供「一般可用性」版本時，將要求您在六個月內更新至「一般可用性」版本，因為使用程式庫預覽版本所撰寫的應用程式可能無法運作。

## 必要條件

此範例需要下列項目：  

  * [Visual Studio](https://www.visualstudio.com/en-us/downloads) 
  * [Microsoft 帳戶](https://www.outlook.com)或[商務用 Office 365 帳戶](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)。需要 Office 365 系統管理員帳戶，才能執行系統管理層級的作業。您可以註冊 [Office 365 開發人員訂用帳戶](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)，其中包含開始建置應用程式所需的資源。

## 註冊 Web 應用程式

### 選擇您要建立應用程式所在的 Azure AD 租用戶

首先，您必須：

1. 使用公司或學校帳戶或個人的 Microsoft 帳戶，登入 [Azure 入口網站](https://portal.azure.com)。
1. 如果您的帳戶出現在多個 Azure AD 租用戶中，請在頁面頂端功能表的右上角選取您的設定檔，然後**切換目錄**。
將您的入口網站工作階段變更為所需 Azure AD 租用戶。

### 註冊應用程式

1. 瀏覽至開發人員適用的 Microsoft 身分識別平台的 \[應用程式註冊][](https://go.microsoft.com/fwlink/?linkid=2083908) 頁面。
1. 選取 \[新增註冊]。
1. 當 \[註冊應用程式] 頁面出現時，輸入您應用程式的註冊資訊：
   - 在 \[名稱] 區段中，輸入將對應用程式使用者顯示、且有意義的應用程式名稱。
   - 將 \[支援的帳戶類型] 變更為 \[任何組織目錄中的帳戶及個人的 Microsoft 帳戶 (例如，Skype、Xbox、Outlook.com)]。
     > 請注意，重新導向 URI 有好幾個。您必須之後在成功建立應用程式後，從 \[驗證] 索引標籤新增這些 URI。
1. 選取 \[註冊] 以建立應用程式。
1. 在應用程式 \[概觀] 頁面上，找到 \[應用程式 (用戶端) 識別碼] 值，並將它記下供稍後使用。您需要這個值，才能設定此專案的 Visual Studio 組態檔。
1. 從應用程式的 \[概觀] 頁面，選取 \[驗證] 區段。
   - 在 \[重新導向 URI] 區段中，選取下拉式方塊中的 \[Web]，然後輸入下列重新導向 URI。
       - `https://localhost:44300/`
       - `https://localhost:44300/signin-oidc`
   - 在 \[進階設定] 區段中，將 \[登出 URL] 設定為 `https://localhost:44300/signout-oidc`
   - 在 \[進階設定] | \[隱含授與] 區段中，核取 \[ID 權杖]，
   因為這個範例需要啟用 \[隱含授與流程][](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow)，
   才能登入使用者並呼叫 API。
1. 選取 \[儲存]。
1. 從 \[憑證和祕密] 頁面的 \[用戶端密碼] 區段中，選擇 \[新用戶端密碼]：
   - 輸入金鑰描述 (例如，`應用程式密碼`)，
   - 選取金鑰持續時間為 \[1 年]、\[2 年] 或 \[永不過期]。
   - 當您按下 \[新增] 按鈕時，將會顯示金鑰值。複製該值並將其儲存在安全的位置。
   - 您之後將需要這個金鑰，才能在 Visual Studio 中設定專案。系統將不會再次顯示此金鑰值，也不會以其他任何方式擷取，因此，
   請在可以從 Azure 入口網站看到時記錄下來。
 
## 建置及執行範例

1. 下載或複製 Microsoft Graph 程式碼片段範例 (適用於 ASP.NET 4.6)。

2. 在 Visual Studio 中開啟範例方案。

3. 在根目錄的 Web.config 檔案中，將 **ida:AppId** 和 **ida:AppSecret** 預留位置值取代為您在應用程式註冊期間複製的值。

4. 按 F5 以建置及執行範例。這樣會還原 NuGet 封裝相依性，並開啟應用程式。

   >如果您在安裝封裝時看到任何錯誤，請確定您放置解決方案的本機路徑不會太長/太深。將解決方案移靠近您的磁碟機根目錄可解決這個問題。

5. 登入您的個人帳戶 (MSA) 或工作或學校帳戶，並授與要求的權限。 

6. 選擇程式碼片段類別，例如使用者、檔案或郵件。 

7. 選擇您想要執行的作業。注意下列事項：
  - 需要引數 (例如 ID) 的作業已遭到停用，直到您執行可讓您選取實體的程式碼片段為止。 
  - 某些程式碼片段 (標示為*僅限系統管理員*) 需要只能由系統管理員授與的商業權限範圍。若要執行這些程式碼片段，您必須以系統管理員的身分，登入 Azure 入口網站。接著，使用應用程式註冊的 \[API 權限]** 區段，同意系統管理員層級的範圍。此索引標籤不適用於以個人帳戶登入的使用者。
  - 如果您使用個人帳戶登入，則會停用不支援 Microsoft 帳戶的程式碼片段。
   
回應資訊會顯示在頁面底部。

### 範例如何影響帳戶資料

這個範例會建立、更新和刪除實體和資料 (例如使用者或檔案)。根據您使用此範例的方式，**您可能會編輯或刪除實際的實體和資料**，並保留資料成品。 

若要使用範例，而不修改您的實際帳戶資料，請務必執行更新，並只刪除範例所建立之實體的作業。 


## 附註的程式碼

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs)。驗證目前使用者，並初始化範例的權杖快取。

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs)。儲存使用者的權杖資訊。您可以將這個項目取代為您自己的自訂權杖快取。在[多租用戶應用程式中的快取存取權杖](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/)中深入了解。

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs)。使用 **AcquireTokenSilentAsync** 方法實作本機 IAuthProvider 介面，並取得存取權杖。您可以將這個項目取代為您自己的授權提供者。 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs)。從 [Microsoft Graph.NET 用戶端程式庫](https://github.com/microsoftgraph/msgraph-sdk-dotnet)初始化用來與 Microsoft Graph 互動的 **GraphServiceClient**。

- 下列控制站包含的方法使用 **GraphServiceClient** 建置並傳送呼叫至 Microsoft Graph 服務，然後處理回應。
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

- 下列檔案包含檢視模型和部分檢視，可用來剖析和顯示 Microsoft Graph 資料作為泛型物件 (針對此範例的目的)。 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [\_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- 下列檔案包含用來支援增量同意的程式碼。在此範例中，系統會提示使用者在登入期間同意最初的一組權限，並另外授與系統管理員權限。 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)。自訂中介軟體，其可兌換授權碼以存取和重新整理登入流程之外的權杖。如需有關實作增量同意的詳細資訊，請參閱 https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2。

## 問題與意見

我們很樂於收到您對於此範例的意見反應。您可以在此儲存機制的 \[問題][](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) 區段中，將您的問題及建議傳送給我們。

我們很重視您的意見。請透過 [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph) 與我們連絡。以 \[MicrosoftGraph] 標記您的問題。

## 參與

如果您想要參與這個範例，請參閱 [CONTRIBUTING.md](CONTRIBUTING.md)。

此專案已採用 [Microsoft 開放原始碼管理辦法](https://opensource.microsoft.com/codeofconduct/)。如需詳細資訊，請參閱[管理辦法常見問題集](https://opensource.microsoft.com/codeofconduct/faq/)，如果有其他問題或意見，請連絡 [opencode@microsoft.com](mailto:opencode@microsoft.com)。 

## 其他資源

- [其他 Microsoft Graph 程式碼片段範例](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Microsoft Graph 概觀](http://graph.microsoft.io)
- [Office 開發人員程式碼範例](http://dev.office.com/code-samples)
- [Office 開發中心](http://dev.office.com/)

## 著作權
Copyright (c) 2016 Microsoft.著作權所有，並保留一切權利。

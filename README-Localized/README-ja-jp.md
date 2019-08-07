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
# ASP.NET 4.6 用 Microsoft Graph スニペットのサンプル

## 目次

* [前提条件](#prerequisites)
* [アプリケーションの登録](#register-the-application)
* [サンプルのビルドと実行](#build-and-run-the-sample)
* [ノートのコード](#code-of-note)
* [質問とコメント](#questions-and-comments)
* [投稿](#contributing)
* [追加情報](#additional-resources)

このサンプル プロジェクトには、ASP.NET MVC アプリ内からのメール送信、グループ管理、および他のアクティビティなどの一般的なタスクを実行するために Microsoft Graph を使用する、コード スニペットのリポジトリが用意されています。[Microsoft Graph .NET クライアント SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet) を使用して、Microsoft Graph が返すデータを操作します。 

サンプルでは認証に [Microsoft 認証ライブラリ (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) を使用します。MSAL SDK には、[Azure AD v2 0 エンドポイント](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview)を操作するための機能が用意されており、開発者は職場または学校 (Azure Active Directory) アカウント、および個人用 (Microsoft) アカウントの両方に対する認証を処理する 1 つのコード フローを記述することができます。

またサンプルでは、トークンを段階的に要求する方法を示します。この方法は Azure AD v2.0 エンドポイントによってサポートされている機能です。ユーザーは、サインイン中にアクセス許可の適用範囲の最初のセットに同意することになりますが、後で他の適用範囲にも同意することができます。このサンプルの場合、すべての有効なユーザーがサインインできますが、管理者は後で特定の操作に必要な管理レベルの適用範囲に同意することができます。

サンプルでは、サインインと最初のトークン取得中に [ASP.NET OpenId Connect OWIN ミドルウェア](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/)を使用します。またサンプルでは、カスタム Owin ミドルウェアも実装して、アクセスの認証コードを交換し、サインイン フローの外部のトークンを更新します。カスタム ミドルウェアは、MSAL を呼び出して承認要求 URI を作成して、リダイレクトを処理します。段階的な同意の詳細については、「[OpenID Connect を使用して、Microsoft Identity と Microsoft Graph を Web アプリケーションに統合する](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2)」を参照してください。

> このサンプルでは、ASP.NET MVC 4.6 を使用します。ASP.NET Core を使用するサンプルについては、次の 2 つのサンプルのいずれかを参照してください
- [ASP.NET Core 2.1 用 Microsoft Graph Connect のサンプル](https://github.com/microsoftgraph/aspnetcore-connect-sample)
- [開発者向けの Microsoft ID プラットフォームを使用して Web Apps でユーザーをサインインさせ API を呼び出せるようにする](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2)

## MSAL プレビューに関する重要な注意事項

このライブラリは、運用環境での使用に適しています。このライブラリに対しては、現在の運用ライブラリと同じ運用レベルのサポートを提供します。プレビュー中にこのライブラリの API、内部キャッシュの形式、および他のメカニズムを変更する場合があります。これは、バグの修正や機能強化の際に実行する必要があります。これは、アプリケーションに影響を与える場合があります。例えば、キャッシュ形式を変更すると、再度サインインが要求されるなどの影響をユーザーに与えます。API を変更すると、コードの更新が要求される場合があります。一般提供リリースが実施されると、プレビュー バージョンを使って作成されたアプリケーションは動作しなくなるため、6 か月以内に一般提供バージョンに更新することが求められます。

## 前提条件

このサンプルを実行するには次のものが必要です。  

  * [Visual Studio](https://www.visualstudio.com/en-us/downloads) 
  * [Microsoft アカウント](https://www.outlook.com)または [Office 365 for Business アカウント](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)のいずれか。管理レベルの操作を実行するには、Office 365 の管理者アカウントが必要です。アプリのビルドを開始するために必要なリソースを含む、[Office 365 Developer サブスクリプション](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account)にサインアップできます。

## Web アプリの登録

### アプリケーションを作成する Azure AD テナントの選択

まず、次のことを行う必要があります。

1. 職場または学校のアカウントか、個人の Microsoft アカウントを使用して、[Azure portal](https://portal.azure.com)にサインインします。
1. ご利用のアカウントが複数の Azure AD テナントに存在する場合は、ページ上部のメニューの右上隅にあるプロフィールを選択してから、
\[**ディレクトリの切り替え**]を選択します。ポータルのセッションを目的の<b> </b>Azure AD テナントに変更します。

### アプリの登録

1. 開発者向けの Microsoft ID プラットフォーム \[[アプリの登録](https://go.microsoft.com/fwlink/?linkid=2083908)] ページに移動します。
1. \[**新規登録**] を選択します。
1. \[**アプリケーションの登録ページ**] が表示されたら、以下のアプリケーションの登録情報を入力します。
   - \[**名前**] セクションに、アプリのユーザーに表示されるわかりやすいアプリケーション名を入力します。
   - \[**サポートされているアカウントの種類**] を \[**任意の組織のディレクトリ内のアカウントと個人用の Microsoft アカウント (例: Skype、Xbox、Outlook.com)**] に変更します。
     > 複数のリダイレクト URI があることに注意してください。アプリが正常に作成された後、\[**認証**] タブからそれらを追加する必要があります。
1. \[**登録**] を選択して、アプリケーションを作成します。
1. アプリの \[**概要**] ページで、\[**Application (client) ID**] (アプリケーション (クライアント) ID) の値を確認し、後で使用するために記録します。この情報は、このプロジェクトで Visual Studio 構成ファイルを設定するのに必要になります。
1. アプリの \[概要] ページで、\[**認証**] セクションを選択します。
   - \[リダイレクト URI] セクションで、コンボ ボックスの \[**Web**] を選択し、次のリダイレクト URI を入力します。
       - `https://localhost:44300/`
       - `https://localhost:44300/signin-oidc`
   - \[**詳細設定**] セクションの \[**ログアウト URL**] を「`https://localhost:44300/signout-oidc`」に設定します。
   - \[**詳細設定**] または \[**暗黙的な許可**] セクションで、このサンプルが
   [暗黙的な許可のフロー](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow)を有効にしてユーザーのサインインができるように、\[**ID トークン**]
   をチェック ボックスをオンにし、API を呼び出します。
1. \[**保存**] を選択します。
1. \[**証明書とシークレット**] ページの \[**クライアント シークレット**] セクションで、\[**新しいクライアント シークレット**]を選択します。
   - キーの説明を入力します (例 : `アプリ シークレット`)。
   - \[**1 年**]、\[**2 年**]、または \[**有効期限なし**] からキーの期間を選択します。
   - \[**追加**] ボタンを押すと、キー値が表示されます。値をコピーして安全な場所に保存します。
   - Visual Studio でプロジェクトを構成するには、このキーが必要になります。このキー値は二度と表示されず、他の方法で取得することもできませんので、
   Azure portal で表示されたらすぐに記録してください。
 
## サンプルのビルドと実行

1. ASP.NET 4.6 用 Microsoft Graph スニペットのサンプルをダウンロードするか、クローンを作成します。

2. Visual Studio でサンプル ソリューションを開きます。

3. ルート ディレクトリの Web.config ファイルで、**ida:AppId** と **ida:AppSecret** のプレースホルダ―の値をアプリの登録時にコピーした値と置き換えます。

4. F5 キーを押して、サンプルをビルドして実行します。これにより、NuGet パッケージの依存関係が復元され、アプリが開きます。

   >パッケージのインストール中にエラーが発生した場合は、ソリューションを保存したローカル パスが長すぎたり深すぎたりしていないかご確認ください。ドライブのルート近くにソリューションを移動すると問題が解決する場合があります。

5. 個人用アカウント (MSA) あるいは職場または学校アカウントでサインインし、要求されたアクセス許可を付与します。 

6. ユーザー、ファイル、メールなどのスニペットのカテゴリを選択します。 

7. 実行する操作を選択します。以下の点に注意してください:
  - 引数 (ID など) を必要とする操作は、エンティティを選択することができるスニペットを実行するまで無効になっています。 
  - 一部のスニペット (*管理者のみ*としてマークされている) には、管理者だけが付与できる商用のアクセス許可の適用範囲が必要です。このスニペットを実行するには、管理者として Azure Portal にサインインする必要があります。次に、アプリの登録の \[*API のアクセス許可*] セクションを使用して、管理レベルの範囲に同意します。このタブは、個人用アカウントでログインしているユーザーに対しては使用できません。
  - 個人用アカウントでログインした場合は、Microsoft アカウントでサポートされていないスニペットは無効になっています。
   
応答情報は、ページの下部に表示されます。

### サンプルによるアカウント データへの影響

このサンプルでは、エンティティとデータ (ユーザーまたはファイルなど) を作成、更新、および削除します。使用方法によっては、**実際のエンティティとデータを編集または削除して**、データの成果物をそのまま残す場合があります。 

実際のアカウント データを変更せずにサンプルを使用するには、必ずサンプルで作成されるエンティティ上でのみ操作の更新と削除を実行します。 


## ノートのコード

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs)。現在のユーザーを認証して、サンプルのトークン キャッシュを初期化します。

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs)。ユーザーのトークン情報を保存します。これを独自のカスタム トークン キャッシュと置き換えることができます。詳細については、「[マルチテナント アプリケーションのアクセス トークンのキャッシュ](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/)」を参照してください。

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs)。ローカルの IAuthProvider インターフェイスを実装して、**AcquireTokenSilentAsync** メソッドを使用してアクセス トークンを取得します。これを独自の承認プロバイダーと置き換えることができます。 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs)。Microsoft Graph との対話に使用される [Microsoft Graph .NET クライアント ライブラリ](https://github.com/microsoftgraph/msgraph-sdk-dotnet)の **GraphServiceClient** を初期化します。

- 次のコントローラーには、呼び出しを構築して Microsoft Graph サービスに送信し、その応答を処理するために **GraphServiceClient** を使用するメソッドが含まれています。
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- 次のビューにはサンプルの UI が含まれています。  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- 次のファイルには、汎用オブジェクトとして Microsoft Graph データを解析して表示する (このサンプルの目的用) ために使用されるビュー モデルと部分的なビューが含まれています。 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [\_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- 次のファイルには、段階的な同意をサポートするために使用されるコードが含まれています。このサンプルで、ユーザーはサインイン中にアクセス許可の初期セットへの同意を求められ、管理者アクセス許可への同意は別途求められます。 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)。アクセスの認証コードを使い、サインイン フローの外部のトークンを更新するカスタム ミドルウェアです。段階的な同意の実装の詳細については、https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2 を参照してください。

## 質問とコメント

このサンプルに関するフィードバックをお寄せください。質問や提案につきましては、このリポジトリの「[問題](https://github.com/microsoftgraph/aspnet-snippets-sample/issues)」セクションで送信できます。

お客様からのフィードバックを重視しています。[スタック オーバーフロー](http://stackoverflow.com/questions/tagged/microsoftgraph)でご連絡ください。ご質問には \[MicrosoftGraph] のタグを付けてください。

## 投稿

このサンプルに投稿する場合は、[CONTRIBUTING.md](CONTRIBUTING.md) を参照してください。

このプロジェクトでは、[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/) が採用されています。詳細については、「[Code of Conduct の FAQ](https://opensource.microsoft.com/codeofconduct/faq/)」を参照してください。また、その他の質問やコメントがあれば、[opencode@microsoft.com](mailto:opencode@microsoft.com) までお問い合わせください。 

## 追加情報

- [他の Microsoft Graph スニペットのサンプル](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Microsoft Graph の概要](http://graph.microsoft.io)
- [Office 開発者向けコード サンプル](http://dev.office.com/code-samples)
- [Office デベロッパー センター](http://dev.office.com/)

## 著作権
Copyright (c) 2016 Microsoft.All rights reserved.

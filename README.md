---
page_type: sample
products:
- office-365
- office-onedrive
- office-outlook
- office-teams
- m365
- ms-graph
languages:
- csharp
- aspx
description: "This sample uses the Microsoft Graph .NET Client Library to work with data, and the Microsoft Identity Web for authentication on the Microsoft identity platform v2.0 endpoint."
extensions:
  contentType: samples
  technologies:
  - Microsoft Graph
  - Microsoft identity platform
  services:
  - Office 365
  - OneDrive
  - Outlook
  - Groups
  - Microsoft identity platform
  - Microsoft Teams
  createdDate: 8/4/2016 10:31:51 AM
---
# Microsoft Graph snippets sample for ASP.NET Core 3.1

## Table of contents

- [Prerequisites](#prerequisites)
- [Register the application](#register-the-application)
- [Build and run the sample](#build-and-run-the-sample)
- [Questions and comments](#questions-and-comments)
- [Contributing](#contributing)
- [Additional resources](#additional-resources)

This sample project provides a repository of code snippets that use the Microsoft Graph to perform common tasks, such as sending email, managing groups, and other activities from within an ASP.NET Core MVC app. It uses the [Microsoft Graph .NET Client SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet) to work with data returned by the Microsoft Graph.

The sample uses the [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web) library for authentication. This library provides features for working with the [Microsoft identity platform (v2.0)](https://docs.microsoft.com/azure/active-directory/develop/v2-overview), which enables developers to write a single auth flow that handles authentication for both work or school (Azure Active Directory) and personal (Microsoft) accounts.

In addition, the sample shows how to request tokens incrementally. Users consent to an initial set of permission scopes during sign in, but can consent to other scopes later. In the case of this sample, any valid user can sign in, but admininstrators can later consent to the admin-level scopes required for certain operations.

## Important note about the Microsoft Identity Web preview

The Microsoft Identity Web library is in preview. Please review the [support SLA](https://github.com/AzureAD/microsoft-identity-web#support-sla) before taking a dependency on this library in production code.

## Prerequisites

This sample requires the following:

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1 or later
- A text editor to work with the code files. While you can use any editor, we recommend [Visual Studio Code](https://code.visualstudio.com/). This repository includes configuration files to enable debugging the sample in Visual Studio Code.
- A Microsoft account (personal or work/school). If you don't have a Microsoft account, there are a couple of options to get a free account:
  - You can [sign up for a new personal Microsoft account](https://signup.live.com/signup?wa=wsignin1.0&rpsnv=12&ct=1454618383&rver=6.4.6456.0&wp=MBI_SSL_SHARED&wreply=https://mail.live.com/default.aspx&id=64855&cbcxt=mai&bk=1454618383&uiflavor=web&uaid=b213a65b4fdc484382b6622b3ecaa547&mkt=E-US&lc=1033&lic=1).
  - You can [sign up for the Office 365 Developer Program](https://developer.microsoft.com/office/dev-program) to get a free Office 365 subscription.

## Register the application

1. Open a browser and navigate to the [Azure Active Directory admin center](https://aad.portal.azure.com). Login using a **personal account** (aka: Microsoft Account) or **Work or School Account**.

1. Select **Azure Active Directory** in the left-hand navigation, then select **App registrations** under **Manage**.

1. Select **New registration**. On the **Register an application** page, set the values as follows.

    - Set **Name** to `ASP.NET Core Graph Snippets App`.
    - Set **Supported account types** to **Accounts in any organizational directory and personal Microsoft accounts**.
    - Under **Redirect URI**, set the first drop-down to `Web` and set the value to `https://localhost:5001/`.

1. Select **Register**. On the **ASP.NET Core Graph Snippets App** page, copy the value of the **Application (client) ID** and save it, you will need it in the next step.

1. Select **Authentication** under **Manage**. Under **Redirect URIs** add a URI with the value `https://localhost:5001/signin-oidc`.

1. Set the **Logout URL** to `https://localhost:5001/signout-oidc`.

1. Locate the **Implicit grant** section and enable **ID tokens**. Select **Save**.

1. Select **Certificates & secrets** under **Manage**. Select the **New client secret** button. Enter a value in **Description** and select one of the options for **Expires** and select **Add**.

1. Copy the client secret value before you leave this page. You will need it in the next step.

    > **IMPORTANT:** This client secret is never shown again, so make sure you copy it now.

## Build and run the sample

1. Open your command-line interface (CLI) in the **./SnippetsApp** directory and run the following command to restore dependencies.

    ```Shell
    dotnet restore
    ```

1. Run the following commands to add your application ID and secret into the [.NET Secret Manager](https://docs.microsoft.com/aspnet/core/security/app-secrets). The Secret Manager is for development purposes only, production apps should use a trusted secret manager for storing secrets. Replace `YOUR_APP_ID` with your application ID, and `YOUR_APP_SECRET` with your application secret.

    ```Shell
    dotnet user-secrets init
    dotnet user-secrets set "AzureAd:ClientId" "YOUR_APP_ID"
    dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_APP_SECRET"
    ```

1. Run the sample:

    - From your CLI: `dotnet run`
    - From Visual Studio Code: Press **F5** or select the **Run** menu, then **Start Debugging**.

1. Sign in with your personal account (MSA) or your work or school account, and grant the requested permissions.

1. Choose a snippets category in the navigation bar, such as **Users**, **Files**, or **Mail**.

    > **NOTE:** If you logged in with a personal account, snippets that aren't supported for Microsoft accounts are removed from the navigation bar.

### How the sample affects your account data

This sample creates, updates, and deletes entities and data (such as users or files). Depending on how you use it, **you might edit or delete actual entities and data** and leave data artifacts.

To use the sample without modifying your actual account data, be sure to perform update and delete operations only on entities that are created by the sample.

## Questions and comments

We'd love to get your feedback about this sample. You can send us your questions and suggestions in the [Issues](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) section of this repository.

Your feedback is important to us. Connect with us on [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Tag your questions with [MicrosoftGraph].

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Additional resources

- [Microsoft Graph ASP.NET Core tutorial](https://docs.microsoft.com/graph/tutorials/aspnet-core)
- [Microsoft Graph overview](https://docs.microsoft.com/graph/overview)
- [Microsoft Graph API reference](https://docs.microsoft.com/graph/api/overview)

## Copyright

Copyright (c) 2020 Microsoft. All rights reserved.

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
# Microsoft Graph-Codeausschnittbeispiel für ASP.NET 4.6 

## Inhaltsverzeichnis

* [Voraussetzungen](#prerequisites)
* [Registrieren der App](#register-the-application)
* [Erstellen und Ausführen des Beispiels](#build-and-run-the-sample)
* [Relevanter Code](#code-of-note)
* [Fragen und Kommentare](#questions-and-comments)
* [Mitwirkung](#contributing)
* [Zusätzliche Ressourcen](#additional-resources)

Dieses Beispielprojekt enthält ein Repository von Codeausschnitten, die Microsoft Graph verwenden, um allgemeine Aufgaben, z. B. das Senden von E-Mails, das Verwalten von Gruppen und andere Aktivitäten, aus einer ASP.NET-MVC-App heraus auszuführen. Es verwendet das [Microsoft Graph .NET-Client-SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet), um mit Daten zu arbeiten, die vom Microsoft Graph zurückgegeben werden. 

Das Beispiel verwendet die [Microsoft-Authentifizierungsbibliothek (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) für die Authentifizierung. Das MSAL-SDK bietet Features für die Arbeit mit dem [Azure AD v2.0-Endpunkt](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview), der es Entwicklern ermöglicht, einen einzelnen Codefluss zu schreiben, der die Authentifizierung sowohl für Geschäfts- oder Schulkonten (Azure Active Directory) als auch für persönliche Konten (Microsoft) verarbeitet. 

Darüber hinaus wird in dem Beispiel veranschaulicht, wie Token inkrementell angefordert werden – ein vom Azure AD v2.0-Endpunkt unterstütztes Feature. Der Benutzer stimmt während der Anmeldung einem anfänglichen Satz von Berechtigungsbereichen zu, es ist jedoch möglich, später auch anderen Bereichen zuzustimmen. Bei diesem Beispiel können sich alle gültigen Benutzer anmelden, Administratoren können jedoch später Bereichen auf Administratorebene zustimmen, die für bestimmte Vorgänge erforderlich sind.

Das Beispiel verwendet die [ASP.NET OpenId Connect OWIN-Middleware](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) zum Anmelden und während der anfänglichen Tokenerfassung. Das Beispiel implementiert auch benutzerdefinierte OWIN-Middleware, um einen Autorisierungscode für Zugriffs- und Aktualisierungstoken außerhalb des Anmeldungsflusses auszutauschen. Die benutzerdefinierte Middleware ruft MSAL auf, um den Anforderungs-URI für die Autorisierung zu erstellen, und verarbeitet die Umleitungen. Weitere Informationen zur inkrementellen Zustimmung finden Sie unter [Integrieren einer Microsoft-Identität und von Microsoft Graph in eine Webanwendung mithilfe von OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

> In diesem Beispiel wird ASP.NET MVC 4.6 verwendet. Beispiele, in denen ASP.NET Core verwendet wird, finden Sie hier:
– [Microsoft Graph Connect-Beispiel für ASP.NET Core 2.1](https://github.com/microsoftgraph/aspnetcore-connect-sample)
– [Enable your Web Apps to sign-in users and call APIs with the Microsoft identity platform for developers](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2) (Aktivieren Ihrer Webanwendungen zum Anmelden von Benutzern und Aufrufen von APIs mit der Microsoft Identity-Platform für Entwickler).

## Wichtiger Hinweis zur MSAL-Vorschau

Diese Bibliothek eignet sich für die Verwendung in einer Produktionsumgebung. Wir bieten für diese Bibliothek den gleichen Support auf Produktionsebene wie für alle anderen aktuellen Produktionsbibliotheken. Während der Vorschau nehmen wir möglicherweise Änderungen an der API, dem internen Cacheformat und anderen Mechanismen dieser Bibliothek vor, die Sie zusammen mit Fehlerbehebungen oder Funktionsverbesserungen übernehmen müssen. Dies kann sich auf Ihre Anwendung auswirken. So kann sich eine Änderung des Cacheformats beispielsweise auf die Benutzer auswirken, indem sie sich z. B. erneut anmelden müssen. Eine Änderung der API kann dazu führen, dass Sie den Code aktualisieren müssen. Wenn wir das allgemein verfügbare Release bereitstellen, müssen Sie innerhalb von sechs Monaten auf die allgemein verfügbare Version aktualisieren, da Anwendungen, die mit einer Vorschauversion der Bibliothek erstellt wurden, möglicherweise nicht mehr funktionieren.

## Voraussetzungen

Für dieses Beispiel ist Folgendes erforderlich:  

  * [Visual Studio](https://www.visualstudio.com/en-us/downloads) 
  * Entweder ein [Microsoft-Konto](https://www.outlook.com) oder ein [Office 365 for Business-Konto](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account). Ein Office 365-Administratorkonto ist erforderlich, um die Vorgänge auf Administratorebene auszuführen. Sie können sich für ein [Office 365-Entwicklerabonnement](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account) registrieren. Dieses umfasst die Ressourcen, die Sie zum Erstellen von Apps benötigen.

## Registrieren der Web-App

### Wählen Sie den Azure AD-Mandanten aus, auf dem Sie die Anwendung erstellen möchten.

Im ersten Schritt müssen Sie Folgendes tun:

1. Melden Sie sich mit einem Geschäfts-, Schul- oder Unikonto oder mit einem persönlichen Microsoft-Konto beim [Azure-Portal](https://portal.azure.com) an.
1. Wenn Ihr Konto auf mehreren Azure AD-Mandanten vorhanden ist, wählen Sie Ihr Profil in der oberen rechten Ecke im Menü oben auf der Seite aus, und **wechseln Sie dann in das entsprechende Verzeichnis**.
Wechseln Sie mit Ihrer Portalsitzung auf den gewünschten Azure AD-Mandanten.

### Registrieren der App

1. Navigieren Sie zur Seite [App-Registrierungen](https://go.microsoft.com/fwlink/?linkid=2083908) der Microsoft Identity Platform für Entwickler.
1. Wählen Sie **Neue Registrierungen** aus.
1. Geben Sie auf der daraufhin angezeigten Seite **Registrieren einer Anwendung** die Registrierungsinformationen für Ihre Anwendung ein:
   - Geben Sie im Abschnitt **Name** einen aussagekräftigen Anwendungsnamen ein, der den Benutzern der App angezeigt wird.
   - Ändern Sie **Unterstützte Kontotypen** in **Konten in allen Organisationsverzeichnissen und persönliche Microsoft-Konten (z. B. Skype, Xbox, Outlook.com)**.
     > Beachten Sie, dass es mehrere Umleitungs-URIs gibt. Sie müssen diese später über die Registerkarte **Authentifizierung** hinzufügen, nachdem die App erfolgreich erstellt wurde.
1. Wählen Sie **Registrieren** aus, um die Anwendung zu erstellen.
1. Auf der Seite **Übersicht** der App können Sie den **Anwendungs-ID (Client-ID)**-Wert finden und für später notieren. Sie benötigen diesen zum Konfigurieren der Visual Studio-Konfigurationsdatei für dieses Projekt.
1. Wählen Sie auf der Seite "Übersicht" der App den Abschnitt **Authentifizierung**.
   - Wählen Sie im Abschnitt "Umleitungs-URIs" im Kombinationsfeld die Option **Web** aus, und geben Sie die folgenden Umleitungs-URIs ein.
       - `https://localhost:44300/`
       - `https://localhost:44300/signin-oidc`
   - Legen Sie im Abschnitt **Erweiterte Einstellungen** die Option **Abmelde-URL** auf `https://localhost:44300/signout-oidc` fest.
   - Aktivieren Sie im Abschnitt **Erweiterte Einstellungen** | **Implizite Gewährung** die Option **ID-Token**, da für dieses Beispiel
   [Impliziter Gewährungsablauf](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow) aktiviert sein muss, um den Benutzer anzumelden und
   eine API aufzurufen.
1. Wählen Sie **Speichern** aus.
1. Wählen Sie auf der Seite **Zertifikate und Geheimnisse** im Abschnitt **Geheimer Clientschlüssel** den Eintrag **Neuer geheimer Clientschlüssel** aus:
   - Geben Sie eine Schlüsselbeschreibung ein (z. B. `Geheimer App-Schlüssel`).
   - Wählen Sie als Schlüsseldauer entweder **In 1 Jahr**, **In 2 Jahren** oder **Läuft nie ab** aus.
   - Wenn Sie auf die Schaltfläche **Hinzufügen** klicken, wird der Schlüsselwert angezeigt. Kopieren Sie den Wert, und speichern Sie ihn an einem sicheren Ort.
   - Sie benötigen diesen Schlüssel später zum Konfigurieren des Projekts in Visual Studio. Dieser Schlüsselwert wird nicht mehr angezeigt, und er kann auch nicht auf andere Weise abgerufen werden, daher sollten Sie ihn aufzeichnen, sobald er aus dem 
   Azure-Portal angezeigt wird.
 
## Erstellen und Ausführen des Beispiels

1. Laden Sie das Microsoft Graph-Codeausschnittbeispiel für ASP.NET 4.6 herunter.

2. Öffnen Sie die Projektmappe in Visual Studio.

3. Ersetzen Sie in der Datei "Web.config" im Stammverzeichnis die Platzhalterwerte ida: **ida:AppId** und **ida:AppSecret** durch die Werte, die Sie während der App-Registrierung kopiert haben.

4. Drücken Sie zum Erstellen und Ausführen des Beispiels F5. Dadurch werden NuGet-Paketabhängigkeiten wiederhergestellt, und die App wird geöffnet.

   >Wenn beim Installieren der Pakete Fehler angezeigt werden, müssen Sie sicherstellen, dass der lokale Pfad, unter dem Sie die Projektmappe abgelegt haben, weder zu lang noch zu tief ist. Dieses Problem lässt sich beheben, indem Sie den Pfad auf Ihrem Laufwerk verkürzen.

5. Melden Sie sich mit Ihrem persönlichen Konto (MSA) oder mit Ihrem Geschäfts- oder Schulkonto an, und gewähren Sie die erforderlichen Berechtigungen. 

6. Wählen Sie eine Codeausschnittkategorie, z. B. Benutzer, Dateien oder E-Mail, aus. 

7. Wählen Sie einen Vorgang aus, den Sie ausführen möchten. Beachten Sie Folgendes:
  - Vorgänge, die ein Argument erfordern (z. B. als ID) werden deaktiviert, bis Sie einen Codeausschnitt ausführen, mit dem Sie eine Entität auswählen können. 
  - Einige Codeausschnitte (als *nur Administrator* markiert) erfordern kommerzielle Berechtigungsbereiche, die nur von einem Administrator erteilt werden können. Um diese Codeausschnitte ausführen zu können, müssen Sie sich beim Azure-Portal als Administrator anmelden. Verwenden Sie dann den Abschnitt *API -Berechtigungen* der App-Registrierung, um den Bereichen auf Administratorebene zuzustimmen. Diese Registerkarte ist nicht für Benutzer verfügbar, die mit persönlichen Konten angemeldet sind.
  - Wenn Sie sich mit einem persönlichen Konto angemeldet haben, werden Codeausschnitte, die nicht für Microsoft-Konten unterstützt werden, deaktiviert.
   
Antwortinformationen werden am unteren Rand der Seite angezeigt.

### Wie sich das Beispiel auf Ihre Mandantendaten auswirkt

In diesem Beispiel werden Entitäten und Daten erstellt, aktualisiert und gelöscht (z. B. Benutzer oder Dateien). Je nachdem, wie Sie das Beispiel verwenden, **bearbeiten oder löschen Sie tatsächliche Entitäten und Daten** und hinterlassen Datenartefakte. 

Um das Beispiel zu verwenden, ohne die tatsächlichen Kontodaten zu ändern, müssen Sie unbedingt Vorgänge nur für Entitäten aktualisieren und löschen, die von dem Beispiel erstellt werden. 


## Relevanter Code

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). Authentifiziert den aktuellen Benutzer und initialisiert den Tokencache des Beispiels.

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). Speichert die Tokeninformationen des Benutzers. Sie können diesen durch Ihren eigenen benutzerdefinierten Tokencache ersetzen. Weitere Informationen finden Sie unter [Zwischenspeichern von Zugriffstoken in einer Anwendung für mehrere Mandanten](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/).

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs). Implementiert die lokale IAuthProvider-Schnittstelle und ruft ein Zugriffstoken mithilfe der **AcquireTokenSilentAsync**-Methode ab. Sie können dies durch Ihren eigenen Autorisierungsanbieter ersetzen. 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs). Initialisiert den **GraphServiceClient** aus der [Microsoft Graph .NET-Clientbibliothek](https://github.com/microsoftgraph/msgraph-sdk-dotnet), die für die Interaktion mit dem Microsoft Graph verwendet wird.

- Die folgenden Controller enthalten Methoden, die den **GraphServiceClient** zum Erstellen und Senden von Aufrufen des Microsoft Graph-Diensts und zum Verarbeiten der Antwort verwenden.
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- Die folgenden Ansichten enthalten die Benutzeroberfläche des Beispiels.  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- Die folgenden Dateien enthalten die Ansichtsmodell sowie die Teilansichten, die (zum Zwecke dieses Beispiels) zum Analysieren und Anzeigen von Microsoft Graph-Daten als generische Objekte verwendet werden. 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [\_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Die folgenden Dateien enthalten Code zur Unterstützung der inkrementellen Zustimmung. In diesem Beispiel werden Benutzer aufgefordert, einem anfänglichen Satz von Berechtigungen während der Anmeldung zu Administratorberechtigungen separat zuzustimmen. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs). Benutzerdefinierte Middleware, um einen Autorisierungscode für Zugriffs- und Aktualisierungstoken außerhalb des Anmeldungsflusses einzulösen. Weitere Informationen zum Implementieren der inkrementellen Zustimmung finden Sie unter „https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2“.

## Fragen und Kommentare

Wir schätzen Ihr Feedback hinsichtlich dieses Beispiels. Sie können uns Ihre Fragen und Vorschläge über den Abschnitt [Probleme](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) dieses Repositorys senden.

Ihr Feedback ist uns wichtig. Nehmen Sie unter [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph) Kontakt mit uns auf. Taggen Sie Ihre Fragen mit \[MicrosoftGraph].

## Mitwirkung

Wenn Sie einen Beitrag zu diesem Beispiel leisten möchten, finden Sie unter [CONTRIBUTING.md](CONTRIBUTING.md) weitere Informationen.

In diesem Projekt wurden die [Microsoft Open Source-Verhaltensregeln](https://opensource.microsoft.com/codeofconduct/) übernommen. Weitere Informationen finden Sie unter [Häufig gestellte Fragen zu Verhaltensregeln](https://opensource.microsoft.com/codeofconduct/faq/), oder richten Sie Ihre Fragen oder Kommentare an [opencode@microsoft.com](mailto:opencode@microsoft.com). 

## Zusätzliche Ressourcen

- [Weitere Codeausschnittbeispiele für Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Microsoft Graph-Übersicht](http://graph.microsoft.io)
- [Office-Entwicklercodebeispiele](http://dev.office.com/code-samples)
- [Office Dev Center](http://dev.office.com/)

## Copyright
Copyright (c) 2016 Microsoft. Alle Rechte vorbehalten.

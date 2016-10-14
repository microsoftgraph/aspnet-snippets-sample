# <a name="microsoft-graph-snippets-sample-for-asp.net-4.6"></a>Microsoft Graph-Codeausschnittbeispiel für ASP.NET 4.6 

## <a name="table-of-contents"></a>Inhalt

* [Voraussetzungen](#prerequisites)
* [Registrieren der App](#register-the-application)
* [Erstellen und Ausführen des Beispiels](#build-and-run-the-sample)
* [Relevanter Code](#code-of-note)
* [Fragen und Kommentare](#questions-and-comments)
* [Mitwirkung](#contributing)
* [Weitere Ressourcen](#additional-resources)

Dieses Beispielprojekt enthält ein Repository von Codeausschnitten, die Microsoft Graph verwenden, um allgemeine Aufgaben, z. B. das Senden von E-Mails, das Verwalten von Gruppen und andere Aktivitäten, aus einer ASP.NET-MVC-App heraus auszuführen. Es verwendet das [Microsoft Graph .NET-Client-SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet), um mit Daten zu arbeiten, die vom Microsoft Graph zurückgegeben werden. 

Das Beispiel verwendet die [Microsoft-Authentifizierungsbibliothek (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) für die Authentifizierung. Das MSAL-SDK bietet Features für die Arbeit mit dem [Azure AD v2.0-Endpunkt](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview), der es Entwicklern ermöglicht, einen einzelnen Codefluss zu schreiben, der die Authentifizierung sowohl für Geschäfts- oder Schulkonten (Azure Active Directory) als auch für persönliche Konten (Microsoft) verarbeitet. 

Darüber hinaus wird in dem Beispiel veranschaulicht, wie Token inkrementell angefordert werden - ein vom Azure AD v2.0-Endpunkt unterstütztes Feature. Der Benutzer stimmt während der Anmeldung einem anfänglichen Satz von Berechtigungsbereichen zu, es ist jedoch möglich, später auch anderen Bereichen zuzustimmen. Bei diesem Beispiel können sich alle gültigen Benutzer anmelden, Administratoren können jedoch später Bereichen auf Administratorebene zustimmen, die für bestimmte Vorgänge erforderlich sind.

Das Beispiel verwendet die [ASP.NET OpenId Connect OWIN-Middleware](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) zum Anmelden und während der anfänglichen Tokenerfassung. Das Beispiel implementiert auch benutzerdefinierte OWIN-Middleware, um einen Autorisierungscode für Zugriffs- und Aktualisierungstoken außerhalb des Anmeldungsflusses auszutauschen. Die benutzerdefinierte Middleware ruft MSAL auf, um den Anforderungs-URI für die Autorisierung zu erstellen, und verarbeitet die Umleitungen. Weitere Informationen zur inkrementellen Zustimmung finden Sie unter [Integrieren einer Microsoft-Identität und von Microsoft Graph in eine Webanwendung mithilfe von OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

 > **Hinweis** Das MSAL-SDK befindet sich derzeit in der Vorabversion und sollte daher nicht in Produktionscode verwendet werden. Die benutzerdefinierte Middleware und der Tokencache haben Einschränkungen, aufgrund der sie für Produktionscode nicht geeignet sind. Die Middleware weist beispielsweise eine harte Abhängigkeit vom Cache auf und der Cache ist sitzungsbasiert. Der Code dient hier nur zur Veranschaulichung.

## <a name="prerequisites"></a>Anforderungen

Für dieses Beispiel ist Folgendes erforderlich:  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * Entweder ein [Microsoft-Konto](https://www.outlook.com) oder ein [Office 365 for Business-Konto](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account). Ein Office 365-Administratorkonto ist erforderlich, um die Vorgänge auf Administratorebene auszuführen. Sie können sich für ein [Office 365-Entwicklerabonnement](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account) registrieren. Dieses umfasst die Ressourcen, die Sie zum Erstellen von Apps benötigen.

## <a name="register-the-application"></a>Registrieren der App

1. Melden Sie sich beim [App-Registrierungsportal](https://apps.dev.microsoft.com/) entweder mit Ihrem persönlichen oder geschäftlichen Konto oder mit Ihrem Schulkonto an.

2. Klicken Sie auf **App hinzufügen**.

3. Geben Sie einen Namen für die App ein, und wählen Sie **Anwendung erstellen** aus. 
    
   Die Registrierungsseite wird angezeigt wird, und die Eigenschaften der App werden aufgeführt.

4. Kopieren Sie die Anwendungs-ID: Dies ist der eindeutige Bezeichner für Ihre App. 

5. Wählen Sie unter **Anwendungsgeheimnisse** die Option **Neues Kennwort generieren** aus. Kopieren Sie das Kennwort aus dem Dialogfeld **Neues Kennwort wurde generiert**.

   Sie müssen die Werte der App-ID und des App-Geheimnisses eingeben, die Sie in die Beispiel-App kopiert haben. 

6. Wählen Sie unter **Plattformen** die Option **Plattform hinzufügen** aus.

7. Wählen Sie **Web** aus.

8. Stellen Sie sicher, dass das Kontrollkästchen **Impliziten Fluss zulassen** aktiviert ist, und geben Sie *https://localhost:44300 /* als Umleitungs-URI ein. 

   Die Option **Impliziten Fluss zulassen** ermöglicht den Hybridfluss. Während der Authentifizierung ermöglicht dies der App, sowohl Anmeldeinformationen (das id_token) als auch Artefakte (in diesem Fall ein Autorisierungscode) abzurufen, den die App zum Abrufen eines Zugriffstokens verwenden kann.

9. Wählen Sie **Speichern** aus.
 
 
## <a name="build-and-run-the-sample"></a>Erstellen und Ausführen des Beispiels

1. Laden Sie das Microsoft Graph-Codeausschnittbeispiel für ASP.NET 4.6 herunter.

2. Öffnen Sie die Projektmappe in Visual Studio.

3. Ersetzen Sie in der Datei „Web.config“ im Stammverzeichnis die Platzhalterwerte **ida: AppId** und **ida: AppSecret** durch die Werte, die Sie während der App-Registrierung kopiert haben.

4. Drücken Sie zum Erstellen und Ausführen des Beispiels F5. Dadurch werden NuGet-Paketabhängigkeiten wiederhergestellt, und die App wird geöffnet.

   >Wenn beim Installieren der Pakete Fehler angezeigt werden, müssen Sie sicherstellen, dass der lokale Pfad, unter dem Sie die Projektmappe abgelegt haben, weder zu lang noch zu tief ist. Dieses Problem lässt sich beheben, indem Sie den Pfad auf Ihrem Laufwerk verkürzen.

5. Melden Sie sich mit Ihrem persönlichen Konto (MSA) oder mit Ihrem Geschäfts- oder Schulkonto an, und gewähren Sie die erforderlichen Berechtigungen. 

6. Wählen Sie eine Codeausschnittkategorie, z. B. Benutzer, Dateien oder E-Mail, aus. 

7. Wählen Sie einen Vorgang aus, den Sie ausführen möchten. Beachten Sie Folgendes:
  - Vorgänge, die ein Argument erfordern (z. B. als ID) werden deaktiviert, bis Sie einen Codeausschnitt ausführen, mit dem Sie eine Entität auswählen können. 

  - Einige Codeausschnitte (als *nur Administrator* markiert) erfordern kommerzielle Berechtigungsbereiche, die nur von einem Administrator erteilt werden können. Um diese Codeausschnitte ausführen zu können, müssen Sie sich als Administrator anmelden und dann den Link auf der Registerkarte *Administratorbereiche* verwenden, um den Bereichen auf Administratorebene zuzustimmen. Diese Registerkarte ist nicht für Benutzer verfügbar, die mit persönlichen Konten angemeldet sind.
   
  - Wenn Sie sich mit einem persönlichen Konto angemeldet haben, werden Codeausschnitte, die nicht für Microsoft-Konten unterstützt werden, deaktiviert.
   
Antwortinformationen werden am unteren Rand der Seite angezeigt.

### <a name="how-the-sample-affects-your-account-data"></a>Wie sich das Beispiel auf Ihre Mandantendaten auswirkt

In diesem Beispiel werden Entitäten und Daten erstellt, aktualisiert und gelöscht (z. B. Benutzer oder Dateien). Je nachdem, wie Sie das Beispiel verwenden, **bearbeiten oder löschen Sie tatsächliche Entitäten und Daten** und hinterlassen Datenartefakte. 

Um das Beispiel zu verwenden, ohne die tatsächlichen Kontodaten zu ändern, müssen Sie unbedingt Vorgänge nur für Entitäten aktualisieren und löschen, die von dem Beispiel erstellt werden. 


## <a name="code-of-note"></a>Relevanter Code

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). Authentifiziert den aktuellen Benutzer und initialisiert den Tokencache des Beispiels.

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). Speichert die Tokeninformationen des Benutzers. Sie können dies durch Ihren eigenen benutzerdefinierten Tokencache ersetzen. Weitere Informationen finden Sie unter [Zwischenspeichern von Zugriffstoken in einer Anwendung für mehrere Mandanten](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/).

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
  - [_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Die folgenden Dateien enthalten Code, der zum Implementieren der inkrementellen Zustimmung verwendet wird. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)

## <a name="questions-and-comments"></a>Fragen und Kommentare

Wir schätzen Ihr Feedback hinsichtlich dieses Beispiels. Sie können uns Ihre Fragen und Vorschläge über den Abschnitt [Probleme](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) dieses Repositorys senden.

Ihr Feedback ist uns wichtig. Nehmen Sie unter [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph) Kontakt mit uns auf. Taggen Sie Ihre Fragen mit [MicrosoftGraph].

## <a name="contributing"></a>Mitwirkung

Wenn Sie einen Beitrag zu diesem Beispiel leisten möchten, finden Sie unter [CONTRIBUTING.md](CONTRIBUTING.md) weitere Informationen.

In diesem Projekt wurden die [Microsoft Open Source-Verhaltensregeln](https://opensource.microsoft.com/codeofconduct/) übernommen. Weitere Informationen finden Sie unter [Häufig gestellte Fragen zu Verhaltensregeln](https://opensource.microsoft.com/codeofconduct/faq/), oder richten Sie Ihre Fragen oder Kommentare an [opencode@microsoft.com](mailto:opencode@microsoft.com). 

## <a name="additional-resources"></a>Zusätzliche Ressourcen

- [Weitere Codeausschnittbeispiele für Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Microsoft Graph-Übersicht](http://graph.microsoft.io)
- [Office-Entwicklercodebeispiele](http://dev.office.com/code-samples)
- [Office Dev Center](http://dev.office.com/)

## <a name="copyright"></a>Copyright
Copyright (c) 2016 Microsoft. Alle Rechte vorbehalten.

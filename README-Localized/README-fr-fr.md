# <a name="microsoft-graph-snippets-sample-for-aspnet-46"></a>Exemple d’extraits de code Microsoft Graph pour ASP.NET 4.6

## <a name="table-of-contents"></a>Sommaire

* [Conditions préalables](#prerequisites)
* [Inscription de l’application](#register-the-application)
* [Création et exécution de l’exemple](#build-and-run-the-sample)
* [Code de note](#code-of-note)
* [Questions et commentaires](#questions-and-comments)
* [Contribution](#contributing)
* [Ressources supplémentaires](#additional-resources)

Cet exemple de projet constitue un référentiel des extraits de code qui utilisent Microsoft Graph pour effectuer des tâches courantes, telles que l’envoi des messages électroniques, la gestion des groupes et d’autres activités au sein d’une application ASP.NET MVC. Il utilise le [kit de développement logiciel Microsoft Graph .NET Client](https://github.com/microsoftgraph/msgraph-sdk-dotnet) pour fonctionner avec les données renvoyées par Microsoft Graph. 

L’exemple utilise la [bibliothèque d’authentification Microsoft (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) pour l’authentification. Le kit de développement logiciel (SDK) MSAL offre des fonctionnalités permettant d’utiliser le [point de terminaison Azure AD v2.0](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview), qui permet aux développeurs d’écrire un flux de code unique qui gère l’authentification des comptes professionnels ou scolaires (Azure Active Directory) et personnels (Microsoft).

En outre, l’exemple montre comment demander des jetons de façon incrémentielle, une fonctionnalité prise en charge par le point de terminaison Azure AD v2.0. Les utilisateurs consentent à un ensemble initial d’étendues d’autorisations lors de la connexion, mais peuvent consentir à d’autres étendues ultérieurement. Dans le cas de cet exemple, tout utilisateur valide peut se connecter, mais les administrateurs peuvent ultérieurement consentir aux étendues de niveau administrateur obligatoire pour certaines opérations.

L’exemple utilise l’[intergiciel ASP.NET OpenId Connect OWIN](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) pour la connexion et pendant l’acquisition initiale des jetons. L’exemple implémente également un intergiciel Owin personnalisé pour échanger un code d’autorisation pour les jetons d’accès et d’actualisation en dehors du flux de connexion. L’intergiciel personnalisé appelle MSAL pour générer l’URI de demande d’autorisation et gère les redirections. Pour plus d’informations sur le consentement incrémentiel, voir [Intégrer l’identité Microsoft et Microsoft Graph dans une application web avec OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

 > **Remarque** Le kit de développement logiciel MSAL se trouve actuellement dans la version préliminaire et en tant que tel, il ne doit pas être utilisé dans le code de production. L’intergiciel personnalisé et le cache de jetons présentent des limitations qui les rendent inappropriés pour le code de production. Par exemple, l’intergiciel contient une dépendance dure sur le cache et le cache est basé sur la session. Le code est utilisé ici à titre indicatif uniquement.

## <a name="prerequisites"></a>Conditions préalables

Cet exemple nécessite les éléments suivants :  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * Soit un [compte Microsoft](https://www.outlook.com), soit un [compte Office 365 pour entreprise](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account). Un compte d’administrateur Office 365 est obligatoire pour exécuter des opérations de niveau administrateur. Vous pouvez vous inscrire à [Office 365 Developer](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account) pour accéder aux ressources dont vous avez besoin pour commencer à créer des applications.

## <a name="register-the-application"></a>Inscription de l’application

1. Connectez-vous au [portail d’inscription des applications](https://apps.dev.microsoft.com/) en utilisant votre compte personnel, professionnel ou scolaire.

2. Choisissez **Ajouter une application**.

3. Entrez un nom pour l’application, puis choisissez **Créer une application**. 
    
   La page d’inscription s’affiche, répertoriant les propriétés de votre application.

4. Copiez l’ID de l’application. Il s’agit de l’identificateur unique de votre application. 

5. Sous **Secrets de l'application**, choisissez **Générer un nouveau mot de passe**. Copiez le mot de passe à partir de la boîte de dialogue **Nouveau mot de passe créé**.

   Vous devez saisir l’ID d’application et la question secrète de l’application que vous avez copiés dans l’exemple d’application. 

6. Sous **Plateformes**, choisissez **Ajouter une plateforme**.

7. Choisissez **Web**.

8. Assurez-vous que la case à cocher **Autoriser un flux implicite** est activée, puis entrez *https://localhost:44300/* comme URI de redirection. 

   L’option **Autoriser le flux implicite** active le flux hybride. Lors de l’authentification, cela permet à l’application de recevoir les informations de connexion (id_token) et les artefacts (dans ce cas, un code d’autorisation) qui servent à obtenir un jeton d’accès.

9. Cliquez sur **Enregistrer**.
 
 
## <a name="build-and-run-the-sample"></a>Création et exécution de l’exemple

1. Téléchargez ou clonez l’exemple d’extraits de code Microsoft Graph pour ASP.NET 4.6.

2. Ouvrez l’exemple de solution dans Visual Studio.

3. Dans le fichier Web.config dans le répertoire racine, remplacez les valeurs d’espace réservé **ida:AppId** et **ida:AppSecret** par les valeurs que vous avez copiées lors de l’inscription de l’application.

4. Appuyez sur F5 pour créer et exécuter l’exemple. Cela entraîne la restauration des dépendances du package NuGet et l’ouverture de l’application.

   >Si vous constatez des erreurs pendant l’installation des packages, vérifiez que le chemin d’accès local où vous avez sauvegardé la solution n’est pas trop long/profond. Pour résoudre ce problème, vous pouvez déplacer la solution dans un dossier plus près du répertoire racine de votre lecteur.

5. Connectez-vous à votre compte personnel (MSA) ou à votre compte professionnel ou scolaire, puis accordez les autorisations demandées. 

6. Choisissez une catégorie d’extraits de code, par exemple Utilisateurs, Fichiers ou Courrier. 

7. Choisissez une opération à exécuter. Remarques :
  - Les opérations qui nécessitent un argument (par exemple, ID) sont désactivées jusqu’à ce que vous exécutiez un extrait de code vous permettant de sélectionner une entité. 

  - Certains extraits (marqués comme *administration seule*) nécessitent des étendues d’autorisation commerciales qui ne peuvent être accordées que par un administrateur. Pour exécuter ces extraits de code, vous devez vous connecter en tant qu’administrateur, puis utiliser le lien de l’onglet *Étendues d’administration* pour consentir aux étendues de niveau administrateur. Cet onglet n’est pas disponible pour les utilisateurs qui sont connectés avec des comptes personnels.
   
  - Si vous êtes connecté avec un compte personnel, les extraits de code qui ne sont pas pris en charge pour les comptes Microsoft sont désactivés.
   
Les informations de réponse s’affichent en bas de la page.

### <a name="how-the-sample-affects-your-account-data"></a>Impact de l’exemple sur les données de votre compte

Cet exemple crée, met à jour et supprime des entités et données (par exemple, des utilisateurs ou fichiers). En fonction de votre mode d’utilisation, **vous pouvez modifier ou supprimer des entités réelles et des données** et laisser des artefacts de données. 

Pour utiliser l’exemple sans modifier vos données de compte réelles, veillez à effectuer les opérations de mise à jour et de suppression uniquement sur les entités créées par l’exemple. 


## <a name="code-of-note"></a>Code de note

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). Authentifie l’utilisateur actuel et initialise le cache de jetons de l’exemple.

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). Stocke les informations du jeton de l’utilisateur. Vous pouvez le remplacer par votre propre cache de jetons personnalisé. Pour plus d’informations, voir [Mise en cache des jetons d’accès dans une application mutualisée](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/).

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs). Implémente l’interface IAuthProvider locale et obtient un jeton d’accès à l’aide de la méthode **AcquireTokenSilentAsync**. Vous pouvez utiliser, à la place, votre propre fournisseur d’autorisation. 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs). Initialise **GraphServiceClient** à partir de la [bibliothèque client Microsoft Graph .NET](https://github.com/microsoftgraph/msgraph-sdk-dotnet) qui sert à interagir avec Microsoft Graph.

- Les contrôleurs suivants contiennent des méthodes qui utilisent **GraphServiceClient** pour créer et envoyer les appels au service Microsoft Graph et traiter la réponse.
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- Les vues suivantes contiennent l’interface utilisateur de l’exemple.  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- Les fichiers suivants contiennent les modèles d’affichage et la vue partielle qui sont utilisés pour analyser et afficher les données Microsoft Graph en tant qu’objets génériques (dans le cadre de cet exemple). 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Les fichiers suivants contiennent le code utilisé pour prendre en charge le consentement incrémentiel. Pour cet exemple, les utilisateurs sont invités à consentir à un ensemble initial d’autorisations au cours de la connexion et à des autorisations d’administrateur séparément. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs). Intergiciel personnalisé qui échange un code d’autorisation pour les jetons d’accès et d’actualisation en dehors du flux de connexion. Pour plus d’informations sur l’implémentation du consentement incrémentiel, reportez-vous à https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2.

## <a name="questions-and-comments"></a>Questions et commentaires

Nous serions ravis de connaître votre opinion sur cet exemple. Vous pouvez nous faire part de vos questions et suggestions dans la rubrique [Problèmes](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) de ce référentiel.

Votre avis compte beaucoup pour nous. Communiquez avec nous sur [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Posez vos questions avec la balise [MicrosoftGraph].

## <a name="contributing"></a>Contribution

Si vous souhaitez contribuer à cet exemple, voir [CONTRIBUTING.MD](CONTRIBUTING.md).

Ce projet a adopté le [code de conduite Microsoft Open Source](https://opensource.microsoft.com/codeofconduct/). Pour plus d’informations, reportez-vous à la [FAQ relative au code de conduite](https://opensource.microsoft.com/codeofconduct/faq/) ou contactez [opencode@microsoft.com](mailto:opencode@microsoft.com) pour toute question ou tout commentaire. 

## <a name="additional-resources"></a>Ressources supplémentaires

- [Autres exemples d’extraits de code Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Présentation de Microsoft Graph](http://graph.microsoft.io)
- [Exemples de code du développeur Office](http://dev.office.com/code-samples)
- [Centre de développement Office](http://dev.office.com/)

## <a name="copyright"></a>Copyright
Copyright (c) 2016 Microsoft. Tous droits réservés.

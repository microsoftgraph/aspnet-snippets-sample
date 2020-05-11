---
page_type: sample
products:
- office-365
- office-outlook
- ms-graph
languages:
- csharp
- aspx
description: "Cet exemple utilise la Bibliothèque de client .NET Microsoft Graph pour travailler avec les données et la bibliothèque d’authentification Microsoft (MSAL) pour l’authentification sur le point de terminaison Azure AD v 2.0"
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
# Exemple d’extraits de code Microsoft Graph pour ASP.NET 4.6

## Table des matières

* [Conditions préalables](#prerequisites)
* [Inscription de l’application](#register-the-application)
* [Création et exécution de l’exemple](#build-and-run-the-sample)
* [Code de note](#code-of-note)
* [Questions et commentaires](#questions-and-comments)
* [Contribution](#contributing)
* [Ressources supplémentaires](#additional-resources)

Cet exemple de projet constitue un référentiel des extraits de code qui utilisent Microsoft Graph pour effectuer des tâches courantes, telles que l’envoi des messages électroniques, la gestion des groupes et d’autres activités au sein d’une application ASP.NET MVC. Il utilise le [kit de développement logiciel Microsoft Graph .NET Client](https://github.com/microsoftgraph/msgraph-sdk-dotnet) pour fonctionner avec les données renvoyées par Microsoft Graph. 

L’exemple utilise la [bibliothèque d’authentification Microsoft (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) pour l’authentification. Le kit de développement logiciel (SDK) MSAL offre des fonctionnalités permettant d’utiliser le [point de terminaison Azure AD v2.0](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview), qui permet aux développeurs d’écrire un flux de code unique qui gère l’authentification des comptes professionnels ou scolaires (Azure Active Directory) et personnels (Microsoft).

En outre, l’exemple montre comment demander des jetons de façon incrémentielle, une fonctionnalité prise en charge par le point de terminaison Azure AD v2.0. Les utilisateurs consentent à un ensemble initial d’étendues d’autorisations lors de la connexion, mais peuvent consentir à d’autres étendues ultérieurement. Dans le cas de cet exemple, tout utilisateur valide peut se connecter, mais les administrateurs peuvent ultérieurement consentir aux étendues de niveau administrateur obligatoires pour certaines opérations.

L’exemple utilise l’[intergiciel ASP.NET OpenId Connect OWIN](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) pour la connexion et pendant l’acquisition initiale des jetons. L’exemple implémente également un intergiciel Owin personnalisé pour échanger un code d’autorisation pour les jetons d’accès et d’actualisation en dehors du flux de connexion. L’intergiciel personnalisé appelle MSAL pour générer l’URI de demande d’autorisation et gère les redirections. Pour plus d’informations sur le consentement incrémentiel, voir [Intégrer une identité Microsoft et Microsoft Graph dans une application web à l’aide d’OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

> Cet exemple utilise ASP.NET MVC 4.6. Pour les exemples qui utilisent ASP.net Core, reportez-vous à l’un de ces deux exemples : - [Exemple de connexion avec Microsoft Graph pour ASP.NET Core 2.1](https://github.com/microsoftgraph/aspnetcore-connect-sample) - [Activez vos applications web pour connecter les utilisateurs et appeler des API avec la plateforme d’identités Microsoft pour développeurs](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2)

## Remarque importante à propos de la préversion MSAL

La bibliothèque peut être utilisée dans un environnement de production. Nous fournissons la même prise en charge du niveau de production pour cette bibliothèque que pour nos bibliothèques de production actuelles. Lors de la version d’essai, nous pouvons apporter des modifications à l’API, au format de cache interne et à d’autres mécanismes de cette bibliothèque que vous devrez prendre en compte avec les correctifs de bogues ou les améliorations de fonctionnalités. Cela peut avoir un impact sur votre application. Par exemple, une modification du format de cache peut avoir un impact sur vos utilisateurs. Par exemple, il peut leur être demandé de se connecter à nouveau. Une modification de l’API peut vous obliger à mettre à jour votre code. Lorsque nous fournissons la version de disponibilité générale, vous devez effectuer une mise à jour vers la version de disponibilité générale dans un délai de six mois, car les applications écrites à l’aide de la version d’évaluation de la bibliothèque ne fonctionneront plus.

## Conditions préalables

Cet exemple nécessite les éléments suivants :  

  * [Visual Studio](https://www.visualstudio.com/en-us/downloads) 
  * Soit un [compte Microsoft](https://www.outlook.com), soit un [compte Office 365 Business](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account). Un compte d’administrateur Office 365 est obligatoire pour exécuter des opérations de niveau administrateur. Vous pouvez vous inscrire à [un abonnement développeur Office 365](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account) pour accéder aux ressources dont vous avez besoin pour commencer à créer des applications.

## Inscription de l’application web

### Sélectionnez le locataire Azure AD où vous voulez créer l’application.

Pour commencer, vous devez effectuer les opérations suivantes :

1. Connectez-vous au [portail Microsoft Azure](https://portal.azure.com) à l’aide d’un compte professionnel ou scolaire, ou d’un compte Microsoft personnel.
1. Si votre compte est présent dans plusieurs locataires Azure AD, sélectionnez votre profil dans le coin supérieur droit du menu en haut de la page, puis **changez de répertoire.** Remplacez votre session de portail par le locataire Azure AD souhaité.

### Inscription de l’application

1. Accédez à la page d’[inscriptions d’applications](https://go.microsoft.com/fwlink/?linkid=2083908) de la plateforme d’identités Microsoft pour développeurs.
1. Sélectionnez **Nouvelle inscription**.
1. Lorsque la **page Inscrire une application** s’affiche, saisissez les informations d’inscription de votre application :
   - Dans la section **Nom**, saisissez un nom d’application cohérent qui s’affichera pour les utilisateurs de l’application.
   - Remplacez **Types de comptes pris en charge** par **Comptes dans un annuaire organisationnel et comptes personnels Microsoft (par ex. Skype, Xbox, Outlook.com)**.
     > Notez qu’il existe plusieurs URI de redirection. Vous devrez les ajouter à partir de l'onglet **Authentification** une fois l’application créée avec succès.
1. Sélectionnez **S’inscrire** pour créer l’application.
1. Sur la page **Vue d’ensemble** de l’application, notez la valeur **ID d’application (client)** et conservez-la pour plus tard. Vous en aurez besoin pour paramétrer le fichier de configuration de Visual Studio pour ce projet.
1. Sélectionnez la section **Authentification** sur la page Vue d’ensemble de l’application.
   - Dans la section URI de redirection, sélectionnez **Web** dans la zone de liste déroulante et entrez les URI de redirection suivants.
       - `https://localhost:44300/`
       - `https://localhost:44300/signin-oidc`
   - Dans la section **Paramètres avancés**, définissez **URL de déconnexion** sur `https://localhost:44300/signout-oidc`
   - Dans la section **Paramètres avancés** | **Octroi implicite**, cochez **Jetons d’ID** car cet exemple nécessite que le [flux d’octroi implicite](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow) soit activé pour connecter l’utilisateur et appeler une API.
1. Sélectionnez **Enregistrer**.
1. Dans la page **Certificats et clés secrètes**, dans la section **Clés secrètes de clients**, sélectionnez **Nouvelle clé secrète client** :
   - Tapez une description de clé (par exemple `clé secrète de l’application`),
   - Sélectionnez une durée de clé : **Dans 1 an**, **Dans 2 ans** ou **N’expire jamais**.
   - Lorsque vous appuyez sur le bouton **Ajouter**, la valeur de la clé s’affiche. Copiez et enregistrez la valeur dans un endroit sûr.
   - Vous aurez besoin de cette clé ultérieurement pour configurer le projet dans Visual Studio. Cette valeur de clé ne sera plus affichée et ne pourra pas être récupérée par d’autres moyens. Par conséquent, enregistrez-la dès qu’elle est visible depuis le portail Microsoft Azure.
 
## Création et exécution de l’exemple

1. Téléchargez ou clonez l’exemple d’extraits de code Microsoft Graph pour ASP.NET 4.6.

2. Ouvrez l’exemple de solution dans Visual Studio.

3. Dans le fichier Web.config dans le répertoire racine, remplacez les valeurs d’espace réservé **ida:AppId** et **ida:AppSecret** par les valeurs que vous avez copiées lors de l’inscription de l’application.

4. Appuyez sur F5 pour créer et exécuter l’exemple. Cela entraîne la restauration des dépendances du package NuGet et l’ouverture de l’application.

   >Si vous constatez des erreurs pendant l’installation des packages, vérifiez que le chemin d’accès local où vous avez sauvegardé la solution n’est pas trop long/profond. Pour résoudre ce problème, vous pouvez déplacer la solution dans un dossier plus près du répertoire racine de votre lecteur.

5. Connectez-vous à votre compte personnel (MSA) ou à votre compte professionnel ou scolaire, puis accordez les autorisations demandées. 

6. Choisissez une catégorie d’extraits de code, par exemple Utilisateurs, Fichiers ou Courrier. 

7. Choisissez une opération à exécuter. Remarques :
  - Les opérations qui nécessitent un argument (par exemple, ID) sont désactivées jusqu’à ce que vous exécutiez un extrait de code vous permettant de sélectionner une entité. 
  - Certains extraits (marqués comme *administration seule*) nécessitent des étendues d’autorisation commerciales qui ne peuvent être accordées que par un administrateur. Pour exécuter ces extraits de code, vous devez vous connecter au portail Microsoft Azure en tant qu’administrateur. Utilisez ensuite la section *Autorisations d’API* de l’inscription de l’application pour consentir aux étendues de niveau administrateur. Cet onglet n’est pas disponible pour les utilisateurs qui sont connectés avec des comptes personnels.
  - Si vous êtes connecté avec un compte personnel, les extraits de code qui ne sont pas pris en charge pour les comptes Microsoft sont désactivés.
   
Les informations de réponse s’affichent en bas de la page.

### Impact de l’exemple sur les données de votre compte

Cet exemple crée, met à jour et supprime des entités et données (par exemple, des utilisateurs ou fichiers). En fonction de votre mode d’utilisation, **vous pouvez modifier ou supprimer des entités réelles et des données** et laisser des artefacts de données. 

Pour utiliser l’exemple sans modifier vos données de compte réelles, veillez à effectuer les opérations de mise à jour et de suppression uniquement sur les entités créées par l’exemple. 


## Code de note

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
  - [\_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Les fichiers suivants contiennent le code utilisé pour prendre en charge le consentement incrémentiel. Pour cet exemple, les utilisateurs sont invités à consentir à un ensemble initial d’autorisations au cours de la connexion et à des autorisations d’administrateur séparément. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs). Intergiciel personnalisé qui échange un code d’autorisation pour les jetons d’accès et d’actualisation en dehors du flux de connexion. Pour plus d’informations sur l’implémentation du consentement incrémentiel, reportez-vous à https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2.

## Questions et commentaires

Nous aimerions connaître votre opinion sur cet exemple. Vous pouvez nous faire part de vos questions et suggestions dans la rubrique [Problèmes](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) de ce référentiel.

Votre avis compte beaucoup pour nous. Communiquez avec nous sur [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Posez vos questions avec la balise \[MicrosoftGraph].

## Contribution

Si vous souhaitez contribuer à cet exemple, voir [CONTRIBUTING.md](CONTRIBUTING.md).

Ce projet a adopté le [code de conduite Open Source de Microsoft](https://opensource.microsoft.com/codeofconduct/). Pour en savoir plus, reportez-vous à la [FAQ relative au code de conduite](https://opensource.microsoft.com/codeofconduct/faq/) ou contactez [opencode@microsoft.com](mailto:opencode@microsoft.com) pour toute question ou tout commentaire. 

## Ressources supplémentaires

- [Autres exemples d’extraits de code Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Présentation de Microsoft Graph](http://graph.microsoft.io)
- [Exemples de code du développeur Office](http://dev.office.com/code-samples)
- [Centre de développement Office](http://dev.office.com/)

## Copyright
Copyright (c) 2016 Microsoft. Tous droits réservés.

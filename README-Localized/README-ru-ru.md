# Пример фрагментов кода Microsoft Graph для ASP.NET 4.6

## Содержание

* [Необходимые компоненты](#Необходимые-компоненты)
* [Регистрация приложения](#Регистрация-приложения)
* [Сборка и запуск приложения](#Сборка-и-запуск-приложения)
* [Полезный код](#Полезный-код)
* [Вопросы и комментарии](#Вопросы-и-комментарии)
* [Участие](#Участие)
* [Дополнительные ресурсы](#Дополнительные-ресурсы)

В этом примере представлены фрагменты кода, использующие Microsoft Graph для отправки электронной почты, управления группами и выполнения других стандартных задач из приложения ASP.NET MVC. Для работы с данными, возвращаемыми Microsoft Graph, используется [клиентский пакет SDK .NET Microsoft Graph](https://github.com/microsoftgraph/msgraph-sdk-dotnet). 

Для проверки подлинности в этом примере используется [Microsoft Authentication Library (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/). MSAL SDK предоставляет функции для работы с [конечной точкой версии 2.0 ](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview), которая позволяет разработчикам создать один блок кода для проверки подлинности как рабочих или учебных (Azure Active Directory), так и личных учетных записей (Майкрософт).

Кроме того, в примере показано, как запрашивать токены пошагово. Эта функция поддерживается конечной точкой версии 2.0. Пользователи предоставляют начальный набор разрешений при входе, но могут предоставить другие разрешения позже. Любой действительный пользователь может войти в это приложение, но администраторы могут позже предоставить разрешения, необходимые для определенных операций.

В примере используется [ПО промежуточного слоя ASP.NET OpenId Connect OWIN](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) для входа и при первом получении токена. В примере также используется специальное ПО промежуточного слоя Owin для обмена кода авторизации на токены доступа и обновления после входа. Специальное ПО промежуточного слоя вызывает MSAL для создания URI запроса на авторизацию и обрабатывает перенаправления. Дополнительные сведения о предоставлении дополнительных разрешений см. в статье [Интеграция идентификатора Майкрософт и Microsoft Graph в веб-приложении с помощью OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

 > **Примечание**. Сейчас доступна предварительная версия SDK MSAL, поэтому его не следует использовать в рабочем коде. Из-за ряда ограничений специальное ПО промежуточного слоя и кэш токенов непригодны для рабочего кода. Например, ПО промежуточного слоя сильно зависит от кэша, а кэш очищается после сеанса. Код используется здесь только в демонстрационных целях.

## Необходимые компоненты

Для этого примера требуется следующее:  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * [Учетная запись Майкрософт](https://www.outlook.com) или [учетная запись Office 365 для бизнеса](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account). Для выполнения административных операций требуется учетная запись администратора Office 365. Вы можете подписаться на [план Office 365 для разработчиков](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account), который включает ресурсы, необходимые для создания приложений.

## Регистрация приложения

1. Войдите на [портал регистрации приложений](https://apps.dev.microsoft.com/) с помощью личной, рабочей или учебной учетной записи.

2. Нажмите кнопку **Добавить приложение**.

3. Введите имя приложения и нажмите кнопку **Создать приложение**. 
    
   Откроется страница регистрации со свойствами приложения.

4. Скопируйте код приложения. Это уникальный идентификатор приложения. 

5. В разделе **Секреты приложения** нажмите кнопку **Создать новый пароль**. Скопируйте пароль из диалогового окна **Новый пароль создан**.

   Вам потребуется ввести скопированные код приложения и пароль в пример приложения. 

6. В разделе **Платформы** нажмите кнопку **Добавление платформы**.

7. Выберите **Веб**.

8. Убедитесь, что установлен флажок **Разрешить неявный поток**, и введите URI перенаправления *https://localhost:44300/*. 

   Параметр **Разрешить неявный поток** включает гибридный поток. Благодаря этому при проверке подлинности приложение может получить данные для входа (id_token) и артефакты (в данном случае — код авторизации), которые оно может использовать, чтобы получить токен доступа.

9. Нажмите кнопку **Сохранить**.
 
 
## Сборка и запуск приложения

1. Скачайте или клонируйте пример фрагментов кода Microsoft Graph для ASP.NET 4.6.

2. Откройте решение в Visual Studio.

3. В корневом каталоге файла Web.config замените заполнители **ida:AppId** и **ida:AppSecret** значениями, которые вы скопировали при регистрации приложения.

4. Нажмите клавишу F5 для сборки и запуска примера. При этом будут восстановлены зависимости пакета NuGet и откроется приложение.

   >Если при установке пакетов возникают ошибки, убедитесь, что локальный путь к решению не слишком длинный. Чтобы устранить эту проблему, переместите решение ближе к корню диска.

5. Войдите с помощью личной, рабочей или учебной учетной записи и предоставьте необходимые разрешения. 

6. Выберите категорию фрагментов, например "Пользователи", "Файлы" или "Почта". 

7. Выберите необходимую операцию. Обратите внимание на следующее:
  - Операции, для которых требуется аргумент (например, идентификатор), будут отключены до запуска фрагмента, который позволяет выбрать объект. 

  - Для некоторых фрагментов (помеченные *только для администраторов*) требуются платные разрешения, которые может предоставить только администратор. Для запуска этих фрагментов войдите в учетную запись администратора и воспользуйтесь ссылкой на вкладке *Admin scopes*, чтобы предоставить необходимые разрешения. Эта вкладка недоступна для пользователей, которые вошли с помощью личных учетных записей.
   
  - Если вы вошли с помощью личной учетной записи, фрагменты, которые не поддерживаются для учетных записей Майкрософт, будут отключены.
   
Сведения об ответе отображаются в нижней части страницы.

### Как пример влияет на данные учетной записи

Этот пример создает, обновляет и удаляет объекты и данные (например, пользователей или файлы). Используя его, **вы можете изменить или удалить объекты и данные**, а также оставить артефакты данных. 

Чтобы этого избежать, обновляйте и удаляйте только те объекты, которые созданы примером. 


## Полезный код

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). Проверяет подлинность текущего пользователя и инициализирует кэш токенов примера.

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). Хранит информацию о токене пользователя. Вы можете заменить его на собственный кэш токенов. Дополнительные сведения см. в статье [Кэширование токенов доступа в мультитенантном приложении](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/).

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs). Реализует локальный интерфейс IAuthProvider и получает токен доступа с помощью метода **AcquireTokenSilentAsync**. Вы можете заменить его на собственного поставщика услуг авторизации. 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs). Инициализирует класс **GraphServiceClient** из [клиентской библиотеки .NET Microsoft Graph ](https://github.com/microsoftgraph/msgraph-sdk-dotnet), используемой для связи с Microsoft Graph.

- Указанные ниже контроллеры содержат методы, которые используют класс **GraphServiceClient** для создания и отправки вызовов в службу Microsoft Graph и обработки ответа.
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- Указанные ниже представления содержат пользовательский интерфейс примера.  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- Указанные ниже файлы содержат представления по умолчанию и частичное представление, которые используются для анализа и отображения данных Microsoft Graph в виде общих объектов (в этом примере). 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Указанные ниже файлы содержат код, используемый для предоставления дополнительных разрешений. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)

## Вопросы и комментарии

Мы будем рады узнать ваше мнение об этом примере. Вы можете отправлять нам вопросы и предложения на вкладке [Issues](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) этого репозитория.

Ваше мнение важно для нас. Для связи с нами используйте сайт [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Помечайте свои вопросы тегом [MicrosoftGraph].

## Добавление кода

Если вы хотите добавить код в этот пример, просмотрите статью [CONTRIBUTING.md](CONTRIBUTING.md).

Этот проект соответствует [правилам поведения Майкрософт, касающимся обращения с открытым кодом](https://opensource.microsoft.com/codeofconduct/). Читайте дополнительные сведения в [разделе вопросов и ответов по правилам поведения](https://opensource.microsoft.com/codeofconduct/faq/) или отправляйте новые вопросы и замечания по адресу [opencode@microsoft.com](mailto:opencode@microsoft.com). 

## Дополнительные ресурсы

- [Другие примеры фрагментов кода Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Общие сведения о Microsoft Graph](http://graph.microsoft.io)
- [Примеры кода приложений для Office](http://dev.office.com/code-samples)
- [Центр разработки для Office](http://dev.office.com/)

## Авторское право
(c) Корпорация Майкрософт (Microsoft Corporation), 2016. Все права защищены.
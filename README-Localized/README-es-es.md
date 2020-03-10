---
page_type: sample
products:
- office-365
- office-outlook
- ms-graph
languages:
- csharp
- aspx
description: "Este ejemplo utiliza la biblioteca cliente .NET de Microsoft Graph para trabajar con los datos, y la biblioteca de autenticación de Microsoft (MSAL) para la autenticación en el extremo de Azure AD v2.0"
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
# Ejemplo de fragmentos de código de muestra de Microsoft Graph para ASP.NET 4.6

## Tabla de contenido

* [Requisitos previos](#prerequisites)
* [Registrar la aplicación](#register-the-application)
* [Compilar y ejecutar el ejemplo](#build-and-run-the-sample)
* [Código de nota](#code-of-note)
* [Preguntas y comentarios](#questions-and-comments)
* [Colaboradores](#contributing)
* [Recursos adicionales](#additional-resources)

Este proyecto de ejemplo proporciona un repositorio de fragmentos de código que usa Microsoft Graph para realizar tareas comunes, como enviar correos electrónicos, administrar grupos y otras actividades desde una aplicación de ASP.NET MVC. Usa el [SDK del cliente de Microsoft Graph .NET](https://github.com/microsoftgraph/msgraph-sdk-dotnet) para trabajar con los datos devueltos por Microsoft Graph. 

El ejemplo usa la [biblioteca de autenticación de Microsoft (MSAL)](https://www.nuget.org/packages/Microsoft.Identity.Client/) para la autenticación. El SDK de MSAL ofrece características para trabajar con el [punto de conexión v2.0 de Azure AD](https://azure.microsoft.com/en-us/documentation/articles/active-directory-appmodel-v2-overview), lo que permite a los desarrolladores escribir un flujo de código único que controla la autenticación para las cuentas profesionales, educativas (Azure Active Directory) o las cuentas personales (Microsoft).

Además, el ejemplo muestra cómo solicitar tokens gradualmente, una característica compatible con el punto de conexión v2.0 de Azure AD. Los usuarios consienten un conjunto inicial de ámbitos de permiso durante el inicio de sesión, pero pueden consentir otros ámbitos más adelante. En el caso de este ejemplo, cualquier usuario válido puede iniciar sesión, pero los administradores pueden consentir más tarde los ámbitos de nivel de administrador necesarios para determinadas operaciones.

El ejemplo usa el [software intermedio OWIN OpenId Connect de ASP.NET](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) para el inicio de sesión y durante la adquisición del token inicial. El ejemplo también implementa el software intermedio Owin personalizado para intercambiar un código de autorización para acceder a los token y actualizarlos fuera del flujo de inicio de sesión. El software intermedio personalizado llama a MSAL para crear el URI de la solicitud de autorización y controla las redirecciones. Para obtener más información acerca del consentimiento incremental, vea [Integrar la identidad de Microsoft y Microsoft Graph en una aplicación web usando OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

> Este ejemplo usa ASP.NET MVC 4.6. Para obtener ejemplos que usan ASP.NET Core, vea uno de estos dos ejemplos:
 - [Microsoft Graph Connect sample for ASP.NET Core 2.1](https://github.com/microsoftgraph/aspnetcore-connect-sample) (Ejemplo de Microsoft Graph Connect para ASP.NET Core 2.1)
 - [Enable your Web Apps to sign-in users and call APIs with the Microsoft identity platform for developers](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2) (Permitir que las aplicaciones web inicien sesión y llamen a las API con la Plataforma de identidad de Microsoft para desarrolladores)

## Nota importante acerca de la vista previa MSAL

Esta biblioteca es apta para utilizarla en un entorno de producción. Ofrecemos la misma compatibilidad de nivel de producción de esta biblioteca que la de las bibliotecas de producción actual. Durante la vista previa podemos realizar cambios en la API, el formato de caché interna y otros mecanismos de esta biblioteca, que deberá tomar junto con correcciones o mejoras. Esto puede afectar a la aplicación. Por ejemplo, un cambio en el formato de caché puede afectar a los usuarios, como que se les pida que vuelvan a iniciar sesión. Un cambio de API puede requerir que actualice su código. Cuando ofrecemos la versión de disponibilidad General, deberá actualizar a la versión de disponibilidad General dentro de seis meses, ya que las aplicaciones escritas mediante una versión de vista previa de biblioteca puede que ya no funcionen.

## Requisitos previos

Este ejemplo necesita lo siguiente:  

  * [Visual Studio](https://www.visualstudio.com/en-us/downloads) 
  * Una [cuenta de Microsoft](https://www.outlook.com) o bien una [cuenta de Office 365 para empresas](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account) Es necesaria una cuenta de administrador de Office 365 para ejecutar operaciones de nivel de administrador. Puede realizar [una suscripción a Office 365 Developer](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account) que incluye los recursos que necesita para comenzar a crear aplicaciones.

## Registrar la aplicación web

### Elija el espacio empresarial de Azure AD donde quiera crear la aplicación

Como primer paso, tendrá que:

1. Iniciar sesión en [Microsoft Azure Portal](https://portal.azure.com) con una cuenta personal, profesional o educativa de Microsoft.
1. Si su cuenta se encuentra en más de un espacio empresarial de Azure AD, seleccione su perfil en la esquina superior derecha en el menú de la parte superior de la página y, después, **cambie el directorio**.
   Cambie la sesión del portal al inquilino de Azure AD deseado.

### Registrar la aplicación

1. Vaya a la página [Registros de aplicaciones](https://go.microsoft.com/fwlink/?linkid=2083908) de plataforma de identidad de Microsoft para desarrolladores.
1. Seleccione **Nuevo registro**.
1. Cuando aparezca la **página Registrar una aplicación**, escriba la información de registro de la aplicación:
   - En la sección **Nombre**, escriba un nombre significativo para la aplicación, que se mostrará a los usuarios de la aplicación.
   - Cambie **Tipos de cuenta admitidos** a **Cuentas en cualquier directorio de organización y cuentas personales de Microsoft (por ejemplo, Skype, Xbox, Outlook.com)**.
     > Tenga en cuenta que hay más de un URI de redirección. Tendrá que agregarlos desde la pestaña **Autenticación** más tarde cuando la aplicación se haya creado correctamente.
1. Seleccione **Registrar** para crear la aplicación.
1. En la página **Información general** de la aplicación, busque el valor **Id. de la aplicación (cliente)** y guárdelo para más tarde. Lo necesitará para configurar el archivo de configuración de Visual Studio para este proyecto.
1. Desde la página Introducción de la aplicación, seleccione la sección **Autenticación**.
   - En la sección URI de redirección, seleccione **Web** en el cuadro combinado y escriba los siguientes URI de redirección.
       - `https://localhost:44300/`
       - `https://localhost:44300/signin-oidc`
   - En la sección **Configuración avanzada**, establezca **Dirección URL de cierre de sesión** en `https://localhost:44300/signout-oidc`
   - En la sección **Configuración avanzada** | Sección **Concesiones implícitas**, marque **Tokens de ID** ya que este ejemplo requiere que el [Flujo de concesiones implícitas](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow) se habilite para iniciar sesión por el usuario y llamar a una API.
1. Seleccione **Guardar**.
1. En la página **Certificados y secretos**, en la sección **Secretos de cliente**, elija **Nuevo secreto de cliente**:
   - Escriba una descripción de clave (por ejemplo, `secreto de la aplicación`).
   - Seleccione una duración de clave de **En un año**, **En 2 años** o **Nunca expira**.
   - Cuando haga clic en el botón **Agregar**, se mostrará el valor de clave. Copie y guarde el valor en una ubicación segura.
   - Necesitará esta clave más tarde para configurar el proyecto en Visual Studio. Este valor de clave no se volverá a mostrar, ni se podrá recuperar por cualquier otro medio, por lo que deberá registrarlo tan pronto como sea visible desde Azure Portal.
 
## Compilar y ejecutar el ejemplo

1. Descargue o clone el ejemplo de fragmentos de código de Microsoft Graph para ASP.NET 4.6

2. Abra la solución del ejemplo en Visual Studio.

3. En el archivo Web.config en el directorio raíz, reemplace los valores de los marcadores de posición **ida:AppId** e **ida:AppSecret** con los valores que ha copiado durante el registro de la aplicación.

4. Pulse F5 para compilar y ejecutar el ejemplo. Esto restaurará las dependencias de paquetes de NuGet y abrirá la aplicación.

   >Si observa algún error durante la instalación de los paquetes, asegúrese de que la ruta de acceso local donde colocó la solución no es demasiado larga o profunda. Para resolver este problema, mueva la solución más cerca de la raíz de la unidad.

5. Inicie sesión con su cuenta personal (MSA) o la cuenta profesional o educativa, y conceda los permisos solicitados. 

6. Elija una categoría de fragmentos de código, como usuarios, archivos o correo. 

7. Seleccione una operación que desee ejecutar. Tenga en cuenta lo siguiente:
  - Las operaciones que requieren un argumento (como Id.) están deshabilitadas hasta que se ejecuta un fragmento de código que le permita seleccionar una entidad. 
  - Algunos fragmentos de código (marcados como *Solo administrador*) requieren ámbitos de permiso comercial que solo puede conceder un administrador. Para ejecutar estos miniprogramas, tiene que iniciar sesión en Azure Portal como administrador. Luego, use la sección de *Permisos de la API* del registro de la aplicación para dar el consentimiento a los ámbitos de nivel de administrador. Esta pestaña no está disponible para los usuarios que han iniciado sesión con cuentas personales.
  - Si ha iniciado sesión con una cuenta personal, se deshabilitan los fragmentos de código que no son compatibles con las cuentas de Microsoft.
   
La información de la respuesta se muestra en la parte inferior de la página.

### Repercusión de la muestra en los datos de la cuenta

Este ejemplo crea, actualiza y elimina entidades y datos, como por ejemplo, usuarios o archivos. Dependiendo del uso que haga, **puede modificar o eliminar entidades reales y datos** y dejar artefactos de datos. 

Para usar el ejemplo sin modificar los datos reales de la cuenta, asegúrese de realizar la actualización y eliminar operaciones solo en las entidades que se crean a través del ejemplo. 


## Código de nota

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). Autentica al usuario actual e inicializa la memoria caché de token del ejemplo.

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). Almacena información de token del usuario. Se puede reemplazar por su memoria caché de token personalizada. Obtenga más información en [Almacenamiento en la memoria caché de los tokens de acceso en una aplicación de varios inquilinos](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/).

- [SampleAuthProvider.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SampleAuthProvider.cs). Implementa la interfaz IAuthProvider local y obtiene un token de acceso usando el método **AcquireTokenSilentAsync**. Se puede reemplazar por su propio proveedor de autorización. 

- [SDKHelper.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Helpers/SDKHelper.cs). Inicializa **GraphServiceClient** de la [biblioteca de cliente de Microsoft Graph .NET](https://github.com/microsoftgraph/msgraph-sdk-dotnet) que se usa para interactuar con Microsoft Graph.

- Los controladores siguientes contienen métodos que usan **GraphServiceClient** para crear y enviar llamadas al servicio Microsoft Graph y procesar la respuesta.
  - [UsersController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/UsersController.cs) 
  - [MailController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/MailController.cs)
  - [EventsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/EventsController.cs) 
  - [FilesController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/FilesController.cs)  
  - [GroupsController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/GroupsController.cs) 

- Las vistas siguientes contienen la IU del ejemplo.  
  - [Users.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Users/Users.cshtml)  
  - [Mail.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Mail/Mail.cshtml)
  - [Events.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Events/Events.cshtml) 
  - [Files.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Files/Files.cshtml)  
  - [Groups.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Groups/Groups.cshtml)

- Los siguientes archivos contienen los modelos de vista y la vista parcial que se usan para analizar y mostrar los datos de Microsoft Graph como objetos genéricos (para esta muestra). 
  - [ResultsViewModel.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Models/ResultsViewModel.cs)
  - [\_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Los siguientes archivos contienen el código usado para admitir el consentimiento incremental. En este ejemplo, se solicita a los usuarios su consentimiento para usar un conjunto inicial de permisos al iniciar sesión y, por otro lado, permisos de administración. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs). Software intermedio personalizado que permite canjear un código de autorización para acceder y actualizar tokens ajenos al flujo de inicio de sesión. Consulte https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2 para obtener más información sobre la implementación del consentimiento incremental.

## Preguntas y comentarios

Nos encantaría recibir sus comentarios sobre este ejemplo. Puede enviarnos sus preguntas y sugerencias a través de la sección [Problemas](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) de este repositorio.

Su opinión es importante para nosotros. Conecte con nosotros en [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Etiquete sus preguntas con \[MicrosoftGraph].

## Colaboradores

Si le gustaría contribuir a este ejemplo, vea [CONTRIBUTING.md](CONTRIBUTING.md).

Este proyecto ha adoptado el [Código de conducta de código abierto de Microsoft](https://opensource.microsoft.com/codeofconduct/). Para obtener más información, vea [Preguntas frecuentes sobre el código de conducta](https://opensource.microsoft.com/codeofconduct/faq/) o póngase en contacto con [opencode@microsoft.com](mailto:opencode@microsoft.com) si tiene otras preguntas o comentarios. 

## Recursos adicionales

- [Otros ejemplos de fragmentos de código de Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Información general de Microsoft Graph](http://graph.microsoft.io)
- [Ejemplos de código de Office Developer](http://dev.office.com/code-samples)
- [Centro para desarrolladores de Office](http://dev.office.com/)

## Derechos de autor
Copyright (c) 2016 Microsoft. Todos los derechos reservados.

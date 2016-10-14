# <a name="microsoft-graph-snippets-sample-for-asp.net-4.6"></a>Ejemplo de fragmentos de código de muestra de Microsoft Graph para ASP.NET 4.6

## <a name="table-of-contents"></a>Tabla de contenido

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

El ejemplo usa el [software intermedio OWIN OpenId Connect de ASP.NET](https://www.nuget.org/packages/Microsoft.Owin.Security.OpenIdConnect/) para el inicio de sesión y durante la adquisición del token inicial. El ejemplo también implementa el software intermedio Owin personalizado para intercambiar un código de autorización para acceder a los token y actualizarlos fuera del flujo de inicio de sesión. El software intermedio personalizado llama a MSAL para crear el URI de la solicitud de autorización y controla las redirecciones. Para obtener más información acerca del consentimiento incremental, consulte [Integrar la identidad de Microsoft y Microsoft Graph en una aplicación web usando OpenID Connect](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2).

 > **Nota** El SDK de MSAL está actualmente en versión preliminar y, por tanto, no debe usarse en código de producción. El software intermedio personalizado y la memoria caché de token tienen limitaciones que los convierten en inadecuados para el código de producción. Por ejemplo, el software intermedio tiene una dependencia fuerte de la memoria caché y la memoria caché está basada en la sesión. El código se usa aquí únicamente con fines ilustrativos.

## <a name="prerequisites"></a>Requisitos previos

Este ejemplo necesita lo siguiente:  

  * [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads) 
  * Una [cuenta de Microsoft](https://www.outlook.com) o bien una [cuenta de Office 365 para empresas](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account) Es necesaria una cuenta de administrador de Office 365 para ejecutar operaciones de nivel de administrador. Puede registrarse para [una suscripción a Office 365 Developer](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment#bk_Office365Account), que incluye los recursos que necesita para comenzar a crear aplicaciones.

## <a name="register-the-application"></a>Registrar la aplicación

1. Inicie sesión en el [Portal de registro de la aplicación](https://apps.dev.microsoft.com/) mediante su cuenta personal, profesional o educativa.

2. Seleccione **Agregar una aplicación**.

3. Escriba un nombre para la aplicación y seleccione **Crear aplicación**. 
    
   Se muestra la página de registro, indicando las propiedades de la aplicación.

4. Copie el Id. de aplicación. Se trata del identificador único para su aplicación. 

5. En **Secretos de aplicación**, seleccione **Generar nueva contraseña**. Copie la contraseña del cuadro de diálogo de **Nueva contraseña generada**.

   Deberá escribir los valores secretos de la aplicación y el Id. de la aplicación que ha copiado en la aplicación del ejemplo. 

6. En **Plataformas**, seleccione **Agregar plataforma**.

7. Seleccione **Web**.

8. Asegúrese de que la casilla **Permitir flujo implícita** está activada e introduzca *https://localhost:44300 /* como el URI de redirección. 

   La opción **Permitir flujo implícito** permite el flujo híbrido. Durante la autenticación, esto permite que la aplicación reciba la información de inicio de sesión (id_token) y artefactos (en este caso, un código de autorización) que la aplicación puede usar para obtener un token de acceso.

9. Seleccione **Guardar**.
 
 
## <a name="build-and-run-the-sample"></a>Compilar y ejecutar el ejemplo

1. Descargue o clone el ejemplo de fragmentos de código de Microsoft Graph para ASP.NET 4.6

2. Abra la solución del ejemplo en Visual Studio.

3. En el archivo Web.config en el directorio raíz, reemplace los valores de los marcadores de posición **ida:AppId** e **ida:AppSecret** con los valores que ha copiado durante el registro de la aplicación.

4. Pulse F5 para compilar y ejecutar el ejemplo. Esto restaurará las dependencias de paquetes de NuGet y abrirá la aplicación.

   >Si observa algún error durante la instalación de los paquetes, asegúrese de que la ruta de acceso local donde colocó la solución no es demasiado larga o profunda. Para resolver este problema, mueva la solución más cerca de la raíz de la unidad.

5. Inicie sesión con su cuenta personal (MSA) o la cuenta profesional o educativa, y conceda los permisos solicitados. 

6. Elija una categoría de fragmentos de código, como usuarios, archivos o correo. 

7. Seleccione una operación que desee ejecutar. Tenga en cuenta lo siguiente:
  - Las operaciones que requieren un argumento (como Id.) están deshabilitadas hasta que se ejecuta un fragmento de código que le permita seleccionar una entidad. 

  - Algunos fragmentos de código (marcados como *Solo administrador*) requieren ámbitos de permiso comercial que solo puede conceder un administrador. Para ejecutar estos fragmentos de código, debe iniciar sesión como administrador y, después, usar el vínculo de la pestaña *Ámbitos de administración* para dar su consentimiento a los ámbitos de nivel de administrador. Esta pestaña no está disponible para los usuarios que han iniciado sesión con cuentas personales.
   
  - Si ha iniciado sesión con una cuenta personal, se deshabilitan los fragmentos de código que no son compatibles con las cuentas de Microsoft.
   
La información de la respuesta se muestra en la parte inferior de la página.

### <a name="how-the-sample-affects-your-account-data"></a>Repercusión de la muestra en los datos de la cuenta

Este ejemplo crea, actualiza y elimina entidades y datos, como por ejemplo, usuarios o archivos. Dependiendo del uso que haga, **puede modificar o eliminar entidades reales y datos** y dejar artefactos de datos. 

Para usar el ejemplo sin modificar los datos reales de la cuenta, asegúrese de realizar la actualización y eliminar operaciones solo en las entidades que se crean a través del ejemplo. 


## <a name="code-of-note"></a>Código de nota

- [Startup.Auth.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/App_Start/Startup.Auth.cs). Autentica al usuario actual e inicializa la memoria caché de token del ejemplo.

- [SessionTokenCache.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/TokenStorage/SessionTokenCache.cs). Almacena información de token del usuario. Se puede reemplazar por su memoria caché de token personalizada. Más información en [Almacenamiento en la memoria caché de los tokens de acceso en una aplicación de varios inquilinos](https://azure.microsoft.com/en-us/documentation/articles/guidance-multitenant-identity-token-cache/).

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
  - [_ResultsPartial.cshtml](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Views/Shared/_ResultsPartial.cshtml)  

- Los archivos siguientes contienen el código usado para implementar el consentimiento incremental. 
  - [AdminController.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Controllers/AdminController.cs)
  - [OAuth2CodeRedeemerMiddleware.cs](/Graph-ASPNET-46-Snippets/Microsoft%20Graph%20ASPNET%20Snippets/Utils/OAuth2CodeRedeemerMiddleware.cs)

## <a name="questions-and-comments"></a>Preguntas y comentarios

Nos encantaría recibir sus comentarios acerca de este ejemplo. Puede enviarnos sus preguntas y sugerencias a través de la sección [Problemas](https://github.com/microsoftgraph/aspnet-snippets-sample/issues) de este repositorio.

Sus comentarios son importantes para nosotros. Conecte con nosotros en [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Etiquete sus preguntas con [MicrosoftGraph].

## <a name="contributing"></a>Colaboradores

Si le gustaría contribuir a este ejemplo, consulte [CONTRIBUTING.md](CONTRIBUTING.md).

Este proyecto ha adoptado el [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/) (Código de conducta de código abierto de Microsoft). Para obtener más información, consulte las [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) (Preguntas más frecuentes del código de conducta) o póngase en contacto con [opencode@microsoft.com](mailto:opencode@microsoft.com) con otras preguntas o comentarios. 

## <a name="additional-resources"></a>Recursos adicionales

- [Otros ejemplos de fragmentos de código de Microsoft Graph](https://github.com/MicrosoftGraph?utf8=%E2%9C%93&query=snippets)
- [Información general de Microsoft Graph](http://graph.microsoft.io)
- [Ejemplos de código de Office Developer](http://dev.office.com/code-samples)
- [Centro de desarrollo de Office](http://dev.office.com/)

## <a name="copyright"></a>Copyright
Copyright (c) 2016 Microsoft. Todos los derechos reservados.

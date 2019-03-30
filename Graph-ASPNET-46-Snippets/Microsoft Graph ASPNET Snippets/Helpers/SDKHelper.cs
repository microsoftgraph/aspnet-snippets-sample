/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.TokenStorage;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System.Web;
using System.Configuration;

namespace Microsoft_Graph_ASPNET_Snippets.Helpers
{
    public class SDKHelper
    {
        // Properties used to get and manage an access token.
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string nonAdminScopes = ConfigurationManager.AppSettings["ida:NonAdminScopes"];
        private static string adminScopes = ConfigurationManager.AppSettings["ida:AdminScopes"];

        // Get an authenticated Microsoft Graph Service client.
        public static GraphServiceClient GetAuthenticatedClient()
        {
            if (!redirectUri.EndsWith("/")) redirectUri = redirectUri + "/";
            bool? isAdmin = HttpContext.Current.Session["IsAdmin"] as bool?;
            string allScopes = nonAdminScopes;
            if (isAdmin.GetValueOrDefault())
            {
                allScopes += " " + adminScopes;
            }
            string[] scopes = allScopes.Split(new char[] { ' ' });

            HttpContextBase context = HttpContext.Current.GetOwinContext().Environment["System.Web.HttpContextBase"] as HttpContextBase;
            SessionTokenCacheProvider sessionTokenCacheProvider = new SessionTokenCacheProvider(context);

            IConfidentialClientApplication cca = AuthorizationCodeProvider.CreateClientApplication(appId, redirectUri, new ClientCredential(appSecret), sessionTokenCacheProvider);

            return new GraphServiceClient(new AuthorizationCodeProvider(cca, scopes));
        }

    }
}
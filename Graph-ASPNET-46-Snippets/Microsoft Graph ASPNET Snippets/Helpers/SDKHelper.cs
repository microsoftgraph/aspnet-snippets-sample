/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.TokenStorage;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System.Web;
using System.Security.Claims;
using System.Configuration;
using System.Linq;

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

            var cca = ConfidentialClientApplicationBuilder.Create(appId)
                                .WithRedirectUri(redirectUri)
                                .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                                .WithClientSecret(appSecret)
                                .Build();

            //cca.UserTokenCache
            var sessionTokenCache = new SessionTokenStore(cca.UserTokenCache, HttpContext.Current,
                ClaimsPrincipal.Current);

            //return new GraphServiceClient(new AuthorizationCodeProvider(cca, scopes));

            return new GraphServiceClient(new DelegateAuthenticationProvider(async (request) => 
            {
                try
                {
                    var accounts = await cca.GetAccountsAsync();
                    var result = await cca.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();

                    request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }));
        }

    }
}
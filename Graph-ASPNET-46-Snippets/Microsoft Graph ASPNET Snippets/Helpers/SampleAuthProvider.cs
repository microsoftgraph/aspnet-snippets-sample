/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft_Graph_ASPNET_Snippets.TokenStorage;
using Resources;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System;

namespace Microsoft_Graph_ASPNET_Snippets.Helpers
{
    public sealed class SampleAuthProvider : IAuthProvider
    {

        // Properties used to get and manage an access token.
        private string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private string nonAdminScopes = ConfigurationManager.AppSettings["ida:NonAdminScopes"];
        private string adminScopes = ConfigurationManager.AppSettings["ida:AdminScopes"];
        private TokenCache tokenCache { get; set; }
        private string url { get; set; }

        private static readonly SampleAuthProvider instance = new SampleAuthProvider();
        private SampleAuthProvider() { }

        public static SampleAuthProvider Instance
        {
            get
            {
                return instance;
            }
        }

        // Gets an access token and its expiration date. First tries to get the token from the token cache.
        public async Task<string> GetUserAccessTokenAsync()
        {

            // Initialize the cache.
            HttpContextBase context = HttpContext.Current.GetOwinContext().Environment["System.Web.HttpContextBase"] as HttpContextBase;
            tokenCache = new SessionTokenCache(
                ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value,
                context).GetMsalCacheInstance();
            //var cachedItems = tokenCache.ReadItems(appId); // see what's in the cache

            if (!redirectUri.EndsWith("/")) redirectUri = redirectUri + "/";
            string[] segments = context.Request.Path.Split(new char[] { '/' });
            ConfidentialClientApplication cca = new ConfidentialClientApplication(
                appId,
                redirectUri + segments[1],
                new ClientCredential(appSecret),
                tokenCache,
                null);
            bool? isAdmin = HttpContext.Current.Session["IsAdmin"] as bool?;

            string allScopes = nonAdminScopes;
            if (isAdmin.GetValueOrDefault())
            {
                allScopes += " " + adminScopes;
            }
            string[] scopes = allScopes.Split(new char[] { ' ' });
            try
            {
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes,cca.Users.First());
                return result.AccessToken;
            }

            // Unable to retrieve the access token silently.
            catch (Exception)
            {
                HttpContext.Current.Request.GetOwinContext().Authentication.Challenge(
                  new AuthenticationProperties() { RedirectUri = redirectUri + segments[1] },
                  OpenIdConnectAuthenticationDefaults.AuthenticationType);

                throw new ServiceException(
                    new Error
                    {
                        Code = GraphErrorCode.AuthenticationFailure.ToString(),
                        Message = Resource.Error_AuthChallengeNeeded,
                    });
            }
        }
    }
}

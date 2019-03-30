/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft_Graph_ASPNET_Snippets.TokenStorage;
using Microsoft_Graph_ASPNET_Snippets.Utils;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{
    public class AdminController : Controller
    {

        public static string clientId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string adminScopes = ConfigurationManager.AppSettings["ida:AdminScopes"];
        // GET: Admin
        public async Task<ActionResult> Index()
        {
            string[] scopes = adminScopes.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            SessionTokenCacheProvider sessionTokenCacheProvider = new SessionTokenCacheProvider(this.HttpContext);
            IConfidentialClientApplication cca = AuthorizationCodeProvider.CreateClientApplication(clientId, redirectUri, new ClientCredential(appKey), sessionTokenCacheProvider);

            try
            {
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes, (await cca.GetAccountsAsync()).FirstOrDefault());
            }
            catch (Exception)
            {
                try
                {// when failing, manufacture the URL and assign it
                    string authReqUrl = await OAuth2RequestManager.GenerateAuthorizationRequestUrl(scopes, cca as ConfidentialClientApplication, this.HttpContext, Url);
                    ViewBag.AuthorizationRequest = authReqUrl;
                }
                catch (Exception ee)
                {

                }
            }

            return View("Admin");

        }
    }
}
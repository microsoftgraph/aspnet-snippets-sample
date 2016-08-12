/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
            // try to get token silently
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            SessionTokenCache theCache = new SessionTokenCache(signedInUserID, this.HttpContext);

            ConfidentialClientApplication cca = new ConfidentialClientApplication(clientId, redirectUri,
                new ClientCredential(appKey), theCache);
            string[] scopes = adminScopes.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes);
            }
            catch (MsalSilentTokenAcquisitionException)
            {
                try
                {// when failing, manufacture the URL and assign it
                    string authReqUrl = await OAuth2RequestManager.GenerateAuthorizationRequestUrl(scopes, cca, this.HttpContext, Url);
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
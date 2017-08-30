using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using Resources;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{
    [Authorize]
    public class ExtensionsController : Controller
    {
        ExtensionsService extensionsService = new ExtensionsService();

        // Open extensions sample built around https://developer.microsoft.com/en-us/graph/docs/concepts/extensibility_open_users
        public ActionResult Index()
        {
            return View("Extensions");
        }

        private static readonly string extensionName = "com.contoso.roamingSettings";
        public async Task<ActionResult> AddOpenExtensionToMe()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                results.Items = await extensionsService.AddOpenExtensionToMe(graphClient,
                    extensionName,
                    new Dictionary<string, object>()
                            { {"theme", "dark"}, {"color","purple"}, {"lang","Japanese"}});
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }

            return View("Extensions", results);
        }

        public async Task<ActionResult> GetOpenExtensionsForMe()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                results.Items = await extensionsService.GetOpenExtensionsForMe(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }

            return View("Extensions", results);
        }

        public async Task<ActionResult> UpdateOpenExtensionForMe()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // For updating a single dictionary item, you would first need to retrieve & then update the extension
                await extensionsService.UpdateOpenExtensionForMe(graphClient,
                    extensionName,
                    new Dictionary<string, object>()
                        { {"theme", "light"}, {"color","yellow"}, {"lang","Swahili"}});

                results.Items = new List<ResultsItem>() { new ResultsItem() { Display = $"{extensionName} {Resource.Extensions_updated}" } };
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }

            return View("Extensions", results);
        }

        public async Task<ActionResult> DeleteOpenExtensionForMe()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                await extensionsService.DeleteOpenExtensionForMe(graphClient, extensionName);

                results.Items = new List<ResultsItem>() { new ResultsItem() { Display = $"{extensionName} {Resource.Extensions_deleted}" } };
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }

            return View("Extensions", results);
        }
    }
}
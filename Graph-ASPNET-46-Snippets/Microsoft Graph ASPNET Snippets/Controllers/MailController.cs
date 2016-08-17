/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using Resources;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{
    [Authorize]
    public class MailController : Controller
    {
        MailService mailService = new MailService();

        public ActionResult Index()
        {
            return View("Mail");
        }

        // Get messages in all the current user's mail folders.
        public async Task<ActionResult> GetMyMessages()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the messages.
                results.Items = await mailService.GetMyMessages(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Get messages in the current user's inbox.
        public async Task<ActionResult> GetMyInboxMessages()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the messages.
                results.Items = await mailService.GetMyInboxMessages(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Send an email message.
        // This snippet sends a message to the current user on behalf of the current user.
        public async Task<ActionResult> SendMessage()
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Send the message.
                results.Items = await mailService.SendMessage(graphClient);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Get a specified message.
        public async Task<ActionResult> GetMessage(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the message.
                results.Items = await mailService.GetMessage(graphClient, id);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }


        // Delete a specified message.
        public async Task<ActionResult> DeleteMessage(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            try
            {
                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Delete the message.
                results.Items = await mailService.DeleteMessage(graphClient, id);
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();

                // Personal accounts that aren't enabled for the Outlook REST API get a "MailboxNotEnabledForRESTAPI" or "MailboxNotSupportedForRESTAPI" error.
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }
    }
}
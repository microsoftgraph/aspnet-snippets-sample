/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using Resources;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{
    [Authorize]
    public class MailController : Controller
    {
        MailService mailService;
        public MailController()
        {
            GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
            mailService = new MailService(graphClient);
        }

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
                // Get the messages.
                results.Items = await mailService.GetMyMessages();
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
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
                // Get the messages.
                results.Items = await mailService.GetMyInboxMessages();
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }


        // Get messages with attachments in the current user's inbox.
        public async Task<ActionResult> GetMyInboxMessagesThatHaveAttachments()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                // Get messages in the Inbox folder that have file attachments.
                results.Items = await mailService.GetMyInboxMessagesThatHaveAttachments();
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Send an email message.
        // This snippet sends a message to the current user on behalf of the current user.
        public async Task<ActionResult> SendMessage()
        {
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {
                // Send the message.
                results.Items = await mailService.SendMessage();
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Send an email message with a file attachment.
        // This snippet sends a message to the current user on behalf of the current user.
        public async Task<ActionResult> SendMessageWithAttachment()
        {
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {
                // Send the message.
                results.Items = await mailService.SendMessageWithAttachment();
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
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
                // Get the message.
                results.Items = await mailService.GetMessage(id);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Reply to a specified message.
        public async Task<ActionResult> ReplyToMessage(string id)
        {
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {
                results.Items = await mailService.ReplyToMessage(id);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Move a specified message. This creates a new copy of the message in the destination folder.
        // This snippet moves the message to the Drafts folder.
        public async Task<ActionResult> MoveMessage(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {
                // Move the message.
                results.Items = await mailService.MoveMessage(id);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }

        // Delete a specified message.
        public async Task<ActionResult> DeleteMessage(string id)
        {
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {
                // Delete the message.
                results.Items = await mailService.DeleteMessage(id);
            }
            catch (ServiceException se)
            {
                if ((se.InnerException as AuthenticationException)?.Error.Code == Resource.Error_AuthChallengeNeeded)
                {
                    HttpContext.Request.GetOwinContext().Authentication.Challenge();
                    return new EmptyResult();
                }
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Mail", results);
        }
    }
}

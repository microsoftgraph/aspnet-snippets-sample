/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft_Graph_ASPNET_Snippets.Helpers;
using Microsoft_Graph_ASPNET_Snippets.Models;
using Resources;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers
{
    [Authorize]
    public class MailController : Controller
    {
        public ActionResult Index()
        {
            return View("Mail");
        }
        
        // Get messages in all the current user's mail folders.
        public async Task<ActionResult> GetMyMessages()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get messages from all mail folders.
                IUserMessagesCollectionPage messages = await graphClient.Me.Messages.Request().GetAsync();
                    
                if (messages?.Count > 0)
                {
                    foreach (Message message in messages)
                    {
                        items.Add(new ResultsItem
                        {
                            Display = message.Subject,
                            Id = message.Id
                        });
                    }
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get messages in the Inbox folder.
                IMailFolderMessagesCollectionPage messages = await graphClient.Me.MailFolders.Inbox.Messages.Request().GetAsync();
                
                if (messages?.Count > 0)
                {
                    foreach (Message message in messages)
                    {
                        items.Add(new ResultsItem
                        {
                            Display = message.Subject,
                            Id = message.Id
                        });
                    }
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            string guid = Guid.NewGuid().ToString();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Create the recipient list. This snippet uses the current user as the recipient.
                User me = await graphClient.Me.Request().Select("Mail, UserPrincipalName").GetAsync();
                string address = me.Mail ?? me.UserPrincipalName;

                List<Recipient> recipients = new List<Recipient>();
                recipients.Add(new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = address
                    }
                });

                // Create the message.
                Message email = new Message
                {
                    Body = new ItemBody
                    {
                        Content = Resource.Prop_Body + guid,
                        ContentType = BodyType.Text,
                    },
                    Subject = Resource.Prop_Subject + guid.Substring(0, 8),
                    ToRecipients = recipients
                };

                // Send the message.
                await graphClient.Me.SendMail(email, true).Request().PostAsync();
                
                // This operation doesn't return anything.
                item.Properties.Add(Resource.No_Return_Data, "");
                items.Add(item);
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the message.
                Message message = await graphClient.Me.Messages[id].Request().GetAsync();
                
                if (message != null)
                {

                    // Get message properties.
                    item.Display = message.Subject;
                    item.Id = message.Id;
                    item.Properties.Add(Resource.Prop_BodyPreview, message.BodyPreview);
                    item.Properties.Add(Resource.Prop_From, message.From.EmailAddress.Name);
                    item.Properties.Add(Resource.Prop_Received, message.ReceivedDateTime.Value.LocalDateTime);
                    item.Properties.Add(Resource.Prop_Id, message.Id);

                    items.Add(item);
                }
                results.Items = items;
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
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Delete the message.
                await graphClient.Me.Messages[id].Request().DeleteAsync();

                // This operation doesn't return anything.
                item.Properties.Add(Resource.No_Return_Data, "");
                items.Add(item);
                results.Items = items;
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
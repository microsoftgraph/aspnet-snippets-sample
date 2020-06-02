// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using SnippetsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SnippetsApp.Controllers
{
    [Authorize]
    public class MailController : BaseController
    {
        public MailController(
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger) : base(tokenAcquisition, logger)
        {
        }

        // GET /Mail?$folderId=""
        // folderId: ID of the selected folder
        // Displays the messages in a specific folder
        // If no folderId is given, displays messages from all folders
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite })]
        public async Task<IActionResult> Index(string folderId = null)
        {
            return await GetViewForList(folderId);
        }

        // GET /Mail/Page?pageUrl=""&folderId=""
        // pageUrl: The page URL to get the next page of results
        // folderId: ID of the currently selected folder
        // Gets the next page of results when a message list is paged
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite })]
        public async Task<IActionResult> Page(string pageUrl,
                                              string folderId = null)
        {
            return await GetViewForList(folderId, pageUrl);
        }

        // Get /Mail/Display?messageId=""
        // messageId: ID of the message to display
        // Displays the requested message allowing user
        // to respond, archive, or delete
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite })]
        public async Task<IActionResult> Display(string messageId)
        {
            var scopes = new[] { GraphConstants.MailReadWrite };

            return await DisplayMessage(messageId, scopes);
        }

        // Get /Mail/DisplayAndConsentForSend?messageId
        // messageId: ID of the message to display
        // Same as the Display method above, but it adds
        // Mail.Send to the requested scopes. This ensures that
        // the user gets prompted for Mail.Send before trying to send
        // the message if they have not yet consented
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite,
                                             GraphConstants.MailSend })]
        public async Task<IActionResult> DisplayAndConsentForSend(string messageId)
        {
            var scopes = new[] { GraphConstants.MailReadWrite,
                                 GraphConstants.MailSend };

            return await DisplayMessage(messageId, scopes);
        }

        // POST /Mail/Update?messageId=""&isRead=""
        // messageId: ID of the message to update
        // isRead: New read status
        // Updates the read status of a message
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite })]
        public async Task<IActionResult> Update(string messageId, bool isRead)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("Message ID cannot be empty.");
            }

            var scopes = new[] { GraphConstants.MailReadWrite };

            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                // Create a new message object with just the properties
                // to update, in this case, IsRead
                var updateMessage = new Message
                {
                    IsRead = isRead
                };

                // PATCH /me/messages/messageId
                //
                // {
                //   "isRead": "true"
                // }
                await graphClient.Me
                    .Messages[messageId]
                    .Request()
                    .UpdateAsync(updateMessage);

                return RedirectToAction("Display", new { messageId = messageId });
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { messageId = messageId })
                    .WithError($"Error updating message with ID: {messageId}", ex.Error.Message);
            }
        }

        // POST /Mail/Move?messageId=""&destinationFolderId=""&returnFolderId=""
        // messageId: ID of the message to move
        // destinationFolderId: ID of the folder to move to
        // returnFolderId: ID of the folder to select in the view after move
        // Moves the message to the destination folder, then
        // redirects to Index with return folder selected
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite })]
        public async Task<IActionResult> Move(string messageId,
                                              string destinationFolderId,
                                              string returnFolderId = null)
        {
            if (string.IsNullOrEmpty(messageId) ||
                string.IsNullOrEmpty(destinationFolderId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("Message ID/folder ID cannot be empty.");
            }

            var scopes = new[] { GraphConstants.MailReadWrite };

            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                // POST /me/messages/messageId/move
                //
                // {
                //   "destinationId": "..."
                // }
                await graphClient.Me
                    .Messages[messageId]
                    .Move(destinationFolderId)
                    .Request()
                    .PostAsync();

                return RedirectToAction("Index", new { folderId = returnFolderId })
                    .WithSuccess("Message moved");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { messageId = messageId })
                    .WithError($"Error moving message with ID {messageId}",
                        ex.Error.Message);
            }
        }

        // POST /Mail/Delete?messageId=""&returnFolderId=""
        // messageId: ID of the message to delete
        // returnFolderId: ID of the folder to select in the view after delete
        // Deletes the specified message and redirects to
        // the Index view with return folder selected
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite })]
        public async Task<IActionResult> Delete(string messageId,
                                                string returnFolderId = null)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("Message ID cannot be empty.");
            }

            var scopes = new[] { GraphConstants.MailReadWrite };

            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                // DELETE /me/messages/messageId
                await graphClient.Me
                    .Messages[messageId]
                    .Request()
                    .DeleteAsync();

                return RedirectToAction("Index", new { folderId = returnFolderId })
                    .WithSuccess("Message deleted");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { messageId = messageId })
                    .WithError($"Error deleting message with ID: {messageId}",
                        ex.Error.Message);
            }
        }

        // POST /Mail/Respond?messageId=""&respondAction=""&comment=""&toRecipient=""
        // messageId: ID of the message to reply to or forward
        // respondAction: Action to take: "reply", "replyAll", or "forward"
        // comment: The body of the reply/forward
        // toRecipient: The email address to forward the message to (only used by forward)
        // Replies to or forwards the message and redirects to the Display action
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.MailReadWrite,
                                             GraphConstants.MailSend})]
        public async Task<IActionResult> Respond(string messageId,
                                                 string respondAction = "reply",
                                                 string comment = null,
                                                 string toRecipient = null)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return RedirectToAction("Error", "Home")
                    .WithError("Message ID cannot be empty.");
            }

            if (respondAction.ToLower() == "forward" && string.IsNullOrEmpty(toRecipient))
            {
                return RedirectToAction("Display", new { messageId = messageId })
                    .WithError("You must supply a recipient email address to forward a message.");
            }

            var scopes = new[] { GraphConstants.MailReadWrite, GraphConstants.MailSend };

            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                switch (respondAction.ToLower())
                {
                    case "reply":
                        await graphClient.Me
                            .Messages[messageId]
                            .Reply(Comment: comment)
                            .Request()
                            .PostAsync();
                        break;
                    case "replyall":
                        await graphClient.Me
                            .Messages[messageId]
                            .ReplyAll(Comment: comment)
                            .Request()
                            .PostAsync();
                        break;
                    case "forward":
                        var recipients = new List<Recipient>
                        {
                            new Recipient
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = toRecipient
                                }
                            }
                        };

                        await graphClient.Me
                            .Messages[messageId]
                            .Forward(recipients, Comment: comment)
                            .Request()
                            .PostAsync();
                        break;
                    default:
                        return RedirectToAction("Display", new { messageId = messageId })
                            .WithError("Invalid respond action provided");
                }

                return RedirectToAction("Display", new { messageId = messageId })
                    .WithSuccess("Message sent");
            }
            catch(ServiceException ex)
            {
                if (ex.InnerException is Microsoft.Identity.Client.MsalUiRequiredException)
                {
                    return RedirectToAction("DisplayAndConsentForSend",
                        new { messageId = messageId})
                        .WithInfo("Send permissions consented. Please retry sending your message.");
                }

                return RedirectToAction("Display", new { messageId = messageId })
                    .WithError($"Error responding to message with ID {messageId}",
                        ex.Error.Message);
            }

        }

        // GET /Mail/New
        [AuthorizeForScopes(Scopes= new[] { GraphConstants.MailSend })]
        public async Task<IActionResult> New()
        {
            var scopes = new[] { GraphConstants.MailSend };
            await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            return View();
        }

        // POST /Mail/Send
        // Sends a new message and saves to
        // Sent Items
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes= new[] { GraphConstants.MailSend })]
        public async Task<IActionResult> Send(string recipient,
                                              string subject,
                                              string body,
                                              List<IFormFile> attachmentFiles)
        {
            if (string.IsNullOrEmpty(recipient))
            {
                return RedirectToAction("New")
                    .WithError("You must supply a recipient");
            }

            var scopes = new[] { GraphConstants.MailSend };

            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                bool saveToSentItems = true;
                // Create the new message
                var message  = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = body
                    },
                    ToRecipients = new List<Recipient>
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = recipient
                            }
                        }
                    }
                };

                if (attachmentFiles != null && attachmentFiles.Count > 0)
                {
                    // Copy the file Stream to a MemoryStream
                    // so we can easily convert to a byte[]
                    var file = attachmentFiles.First();
                    var memoryStream = new MemoryStream();
                    file.OpenReadStream().CopyTo(memoryStream);

                    // Initialize the collection page
                    message.Attachments = new MessageAttachmentsCollectionPage();
                    // Add an attachment
                    // Use FileAttachment object since this is a file
                    message.Attachments.Add(new FileAttachment
                    {
                        Name = file.FileName,
                        ContentType = file.ContentType,
                        ContentBytes = memoryStream.ToArray()
                    });
                }

                // POST /me/sendmail
                //
                // {
                //   "saveToSentItems": "true",
                //   "message": {
                //     "subject": "...",
                //     "body": {
                //       "contentType": "text",
                //       "content": "..."
                //     },
                //     "toRecipients": [
                //       {
                //         "emailAddress": {
                //           "address": "..."
                //         }
                //       }
                //     ],
                //     "attachments": [
                //       {
                //         "name": "...",
                //         "contentType": "...",
                //         "contentBytes": "..."
                //       }
                //     ]
                //   }
                // }
                await graphClient.Me
                    .SendMail(message, saveToSentItems)
                    .Request()
                    .PostAsync();

                return RedirectToAction("Index")
                    .WithSuccess("Message sent");
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index")
                    .WithError("Error sending message", ex.Error.Message);
            }
        }

        // Builds a MailViewDisplayModel
        // folderId: ID of the folder to get messages from. If null, gets from all folders
        // pageRequestUrl: Used for paging requests to get the next set of results
        private async Task<IActionResult> GetViewForList(string folderId = null,
                                                         string pageRequestUrl = null)
        {
            var model = new MailViewDisplayModel();

            var scopes = new[] { GraphConstants.MailReadWrite };

            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                // Get the mail folder list
                var mailFolderPage = await graphClient.Me
                    .MailFolders
                    .Request()
                    .Top(GraphConstants.PageSize)
                    .GetAsync();

                // If there are more available than can fit in
                // the page, use a PageIterator to get them all
                if (mailFolderPage.NextPageRequest == null)
                {
                    model.MailFolders = mailFolderPage.CurrentPage;
                }
                else
                {
                    model.MailFolders = await GetAllPages<MailFolder>(
                        graphClient, mailFolderPage);
                }

                if (!string.IsNullOrEmpty(folderId))
                {
                    var selectedFolder = model.MailFolders.First(f => f.Id == folderId);
                    if (selectedFolder == null)
                    {
                        return RedirectToAction("Error", "Home")
                            .WithError($"Invalid folder ID: {folderId}");
                    }

                    model.SelectedFolder = selectedFolder;
                }

                // Get messages
                // Is this a page request?
                if (!string.IsNullOrEmpty(pageRequestUrl))
                {
                    // Instead of using the request builders,
                    // initialize the request directly from the supplied
                    // URL
                    var pageRequest =
                    new MailFolderMessagesCollectionRequest(
                        pageRequestUrl, graphClient, null);

                    var messagePage = await pageRequest.GetAsync();

                    model.Messages = messagePage.CurrentPage;
                    model.NextPageUrl = messagePage.NextPageRequest?.RequestUrl;
                }
                else if (string.IsNullOrEmpty(folderId))
                {
                    // No folder ID specified, so use /me/messages
                    // to get all messages in all folders
                    // GET /me/messages?$top=25&$select=""&orderby=""
                    var messagePage = await graphClient.Me
                        .Messages
                        .Request()
                        // Get max PageSize number of results
                        .Top(GraphConstants.PageSize)
                        // Only get the fields used by the view
                        .Select(m => new
                        {
                            m.From,
                            m.HasAttachments,
                            m.IsDraft,
                            m.IsRead,
                            m.ReceivedDateTime,
                            m.Subject
                        })
                        // Sort by received time, newest first
                        .OrderBy("receivedDateTime DESC")
                        .GetAsync();

                    model.Messages = messagePage.CurrentPage;
                    model.NextPageUrl = messagePage.NextPageRequest?
                        .GetHttpRequestMessage().RequestUri.ToString();
                }
                else
                {
                    // Folder ID specified, so only get messages from
                    // that folder
                    // GET /me/mailfolders/folderId/messages?...
                    var messagePage = await graphClient.Me
                        .MailFolders[folderId]
                        .Messages
                        .Request()
                        .Top(GraphConstants.PageSize)
                        .Select(m => new
                        {
                            m.From,
                            m.HasAttachments,
                            m.IsDraft,
                            m.IsRead,
                            m.ReceivedDateTime,
                            m.Subject
                        })
                        .OrderBy("receivedDateTime DESC")
                        .GetAsync();

                    model.Messages = messagePage.CurrentPage;
                    model.NextPageUrl = messagePage.NextPageRequest?
                        .GetHttpRequestMessage().RequestUri.ToString();
                }

                return View("Index", model);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Error", "Home")
                    .WithError($"Error getting messages", ex.Error.Message);
            }
        }

        // Returns a view for the requested message
        // messageId: ID of the message to display
        // scopes: Array of permission scopes to use in the Graph client
        private async Task<IActionResult> DisplayMessage(string messageId,
                                                         string[] scopes)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return RedirectToAction("Index")
                    .WithError("Message ID cannot be empty.");
            }

            try
            {
                var graphClient = GetGraphClientForScopes(scopes);

                // GET /me/messages/messageId?$select=""&expand="attachments"
                var message = await graphClient.Me
                    .Messages[messageId]
                    .Request()
                    // Only get the fields used by the app
                    .Select(m => new {
                        m.Body,
                        m.CcRecipients,
                        m.From,
                        m.Id,
                        m.IsDraft,
                        m.IsRead,
                        m.ParentFolderId,
                        m.ReceivedDateTime,
                        m.Subject,
                        m.ToRecipients,
                    })
                    // Expand attachments so result includes attachment array
                    .Expand(m => new { m.Attachments })
                    .GetAsync();

                return View("Display", message);
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Index")
                    .WithError($"Error getting message with ID: {messageId}", ex.Error.Message);
            }
        }
    }
}

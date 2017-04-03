/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{
    public class MailService
    {

        // Get messages in all the current user's mail folders.
        public async Task<List<ResultsItem>> GetMyMessages(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

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
            return items;
        }

        // Get messages in the current user's inbox.
        // To get the messages from another mail folder, you can specify the Drafts, DeletedItems, or SentItems well-known folder,
        // or you can specify the folder ID, for example: `await graphClient.Me.MailFolders[folder-id].Messages.Request().GetAsync();`
        public async Task<List<ResultsItem>> GetMyInboxMessages(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
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
            return items;
        }

        // Get messages with attachments in the current user's inbox.
        public async Task<List<ResultsItem>> GetMyInboxMessagesThatHaveAttachments(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get messages in the Inbox folder that have attachments.
            // Note: Messages that have inline messages don't set the `hasAttachments` property to `true`. To find them, you need to parse the body content.
            IMailFolderMessagesCollectionPage messages = await graphClient.Me.MailFolders.Inbox.Messages.Request().Filter("hasAttachments eq true").Expand("attachments").GetAsync();

            if (messages?.Count > 0)
            {
                foreach (Message message in messages)
                {

                    // This snippet displays information about the first attachment and displays the content if it's an image.
                    // Reference attachments include a file attachment as a file-type icon.
                    Attachment firstAttachment = message.Attachments[0];
                    byte[] contentBytes = null;
                    if ((firstAttachment is FileAttachment) && (firstAttachment.ContentType.Contains("image")))
                    {
                        FileAttachment fileAttachment = firstAttachment as FileAttachment;
                        contentBytes = fileAttachment.ContentBytes;
                    }

                    items.Add(new ResultsItem
                    {
                        Display = message.Subject,
                        Id = message.Id,
                        Properties = new Dictionary<string, object>
                        {
                            { Resource.Prop_AttachmentsCount, message.Attachments.Count },
                            { Resource.Prop_AttachmentName, firstAttachment.Name },
                            { Resource.Prop_AttachmentType, firstAttachment.ODataType },
                            { Resource.Prop_AttachmentSize, firstAttachment.Size },
                            { "Stream", contentBytes }
                        }
                    });
                }
            }
            return items;
        }

        // Send an email message.
        // This snippet sends a message to the current user on behalf of the current user.
        public async Task<List<ResultsItem>> SendMessage(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Create the recipient list. This snippet uses the current user as the recipient.
            User me = await graphClient.Me.Request().Select("Mail, UserPrincipalName").GetAsync();
            string address = me.Mail ?? me.UserPrincipalName;
            string guid = Guid.NewGuid().ToString();

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

            items.Add(new ResultsItem
            {
                // This operation doesn't return anything.
                Properties = new Dictionary<string, object>
                {
                    {  Resource.No_Return_Data, "" }
                }
            });
            return items;
        }

        // Send an email message with a file attachment.
        // This snippet sends a message to the current user on behalf of the current user.
        public async Task<List<ResultsItem>> SendMessageWithAttachment(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Create the recipient list. This snippet uses the current user as the recipient.
            User me = await graphClient.Me.Request().Select("Mail, UserPrincipalName").GetAsync();
            string address = me.Mail ?? me.UserPrincipalName;
            string guid = Guid.NewGuid().ToString();

            List<Recipient> recipients = new List<Recipient>();
            recipients.Add(new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = address
                }
            });

            // Create an attachment and add it to the attachments collection.
            MessageAttachmentsCollectionPage attachments = new MessageAttachmentsCollectionPage();
            attachments.Add(new FileAttachment
            {
                ODataType = "#microsoft.graph.fileAttachment",
                ContentBytes = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath("/Content/test.png")),
                ContentType = "image/png",
                Name = "test.png"
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
                ToRecipients = recipients,
                Attachments = attachments
            };

            // Send the message.
            await graphClient.Me.SendMail(email, true).Request().PostAsync();

            items.Add(new ResultsItem
            {
                // This operation doesn't return anything.
                Properties = new Dictionary<string, object>
                {
                    {  Resource.No_Return_Data, "" }
                }
            });
            return items;
        }

        // Get a specified message.
        public async Task<List<ResultsItem>> GetMessage(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Get the message.
            Message message = await graphClient.Me.Messages[id].Request().GetAsync();

            if (message != null)
            {
                items.Add(new ResultsItem
                {

                    // Get message properties.
                    Display = message.Subject,
                    Id = message.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_BodyPreview, message.BodyPreview },
                        { Resource.Prop_IsDraft, message.IsDraft.ToString() },
                        { Resource.Prop_Id, message.Id }
                    }
                });
            }
            return items;
        }

        // Reply to a specified message.
        public async Task<List<ResultsItem>> ReplyToMessage(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Reply to the message.
            await graphClient.Me.Messages[id].Reply(Resource.GenericText).Request().PostAsync();

            items.Add(new ResultsItem
            {

                // This operation doesn't return anything.
                Properties = new Dictionary<string, object>
                {
                    { Resource.No_Return_Data, "" }
                }
            });
            return items;
        }

        // Move a specified message. This creates a new copy of the message in the destination folder.
        // This snippet moves the message to the Drafts folder using the well-known folder name.
        public async Task<List<ResultsItem>> MoveMessage(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Move the message.
            Message message = await graphClient.Me.Messages[id].Move("Drafts").Request().PostAsync();

            items.Add(new ResultsItem
            {

                // Get message properties.
                Display = message.Subject,
                Id = message.Id,
                Properties = new Dictionary<string, object>
                {
                    { Resource.Prop_BodyPreview, message.BodyPreview },
                    { Resource.Prop_From, message.From?.EmailAddress.Name },
                    { Resource.Prop_Received, message.ReceivedDateTime.Value.LocalDateTime },
                    { Resource.Prop_Id, message.Id }
                }
            });
            return items;
        }

        // Delete a specified message.
        public async Task<List<ResultsItem>> DeleteMessage(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Delete the message.
            await graphClient.Me.Messages[id].Request().DeleteAsync();

            items.Add(new ResultsItem
            {

                // This operation doesn't return anything.
                Properties = new Dictionary<string, object>
                {
                    { Resource.No_Return_Data, "" }
                }
            });
            return items;
        }
    }
}
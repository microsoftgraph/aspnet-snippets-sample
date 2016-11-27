/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                        { Resource.Prop_From, message.From?.EmailAddress.Name },
                        { Resource.Prop_Received, message.ReceivedDateTime.Value.LocalDateTime },
                        { Resource.Prop_Id, message.Id }
                    }
                });
            }
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
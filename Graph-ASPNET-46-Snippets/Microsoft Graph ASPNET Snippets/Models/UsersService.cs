/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Graph.Auth;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{
    public class UsersService
    {
        private GraphServiceClient graphClient;
        private IList<Option> requestOptions;
        public UsersService(GraphServiceClient graphServiceClient)
        {
            graphClient = graphServiceClient;
            requestOptions = new List<Option>
            {
                new HeaderOption("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"")
            };
        }

        // Get all users.
        public async Task<List<ResultsItem>> GetUsers()
        {
            List<ResultsItem> items = new List<ResultsItem>();
            // Get users.
            IGraphServiceUsersCollectionPage users = await graphClient.Users.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).GetAsync();

            // Populate the view model.
            if (users?.Count > 0)
            {
                foreach (User user in users)
                {
                    
                    // Filter out conference rooms.
                    string displayName = user.DisplayName ?? "";
                    if (!displayName.StartsWith("Conf Room"))
                    {

                        // Get user properties.
                        items.Add(new ResultsItem
                        {
                            Display = user.DisplayName,
                            Id = user.Id
                        });
                    }
                }
            }
            return items;
        }

        // Get the current user's profile.
        public async Task<List<ResultsItem>> GetMe()
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the current user's profile.
            var account = ClaimsPrincipal.Current.ToGraphUserAccount();
            User me = await graphClient.Me.Request().GetAsync();

            if (me != null)
            {

                // Get user properties.
                items.Add(new ResultsItem
                {
                    Display = me.DisplayName,
                    Id = me.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Upn, me.UserPrincipalName },
                        { Resource.Prop_Id, me.Id }
                    }
                });
            }
            return items;
        }

        // Get the current user's manager.
        public async Task<List<ResultsItem>> GetMyManager()
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the current user's manager.
            User manager = await graphClient.Me.Manager.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).GetAsync() as User;

            if (manager != null)
            {

                // Get user properties.
                items.Add(new ResultsItem
                {
                    Display = manager.DisplayName,
                    Id = manager.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Upn, manager.UserPrincipalName },
                        { Resource.Prop_Id, manager.Id }
                    }
                });
            }
            return items;
        }

        // Get the current user's photo. 
        public async Task<List<ResultsItem>> GetMyPhoto()
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get my photo.
            using (Stream photo = await graphClient.Me.Photo.Content.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).GetAsync())
            {
                if (photo != null)
                {

                    // Get byte[] for display.
                    using (BinaryReader reader = new BinaryReader(photo))
                    {
                        byte[] data = reader.ReadBytes((int)photo.Length);
                        items.Add(new ResultsItem
                        {
                            Properties = new Dictionary<string, object>
                            {
                                { "Stream", data }
                            }
                        });
                    }
                }
            }
            return items;
        }

        // Create a new user in the signed-in user's tenant.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> CreateUser()
        {
            List<ResultsItem> items = new List<ResultsItem>();
            string guid = Guid.NewGuid().ToString();

            // This snippet gets the tenant domain from the Organization object to construct the user's email address.
            IGraphServiceOrganizationCollectionPage organization = await graphClient.Organization.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).GetAsync();
            string alias = Resource.User.ToLower() + guid.Substring(0, 8);
            string domain = organization.CurrentPage[0].VerifiedDomains.ElementAt(0).Name;

            // Add the user.
            User user = await graphClient.Users.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).AddAsync(new User
            {
                AccountEnabled = true,
                DisplayName = Resource.User + guid.Substring(0, 8),
                MailNickname = alias,
                PasswordProfile = new PasswordProfile
                {
                    Password = Resource.Prop_Password
                },
                UserPrincipalName = alias + "@" + domain
            });

            if (user != null)
            {

                // Get user properties.
                items.Add(new ResultsItem
                {
                    Display = user.DisplayName,
                    Id = user.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Upn, user.UserPrincipalName },
                        { Resource.Prop_Id, user.Id }
                    }
                });
            }
            return items;
        }

        // Get a specified user.
        public async Task<List<ResultsItem>> GetUser(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Get the user.
            User user = await graphClient.Users[id].Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).GetAsync();

            if (user != null)
            {

                // Get user properties.
                items.Add(new ResultsItem
                {
                    Display = user.DisplayName,
                    Id = user.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Upn, user.UserPrincipalName },
                        { Resource.Prop_Id, user.Id }
                    }
                });
            }
            return items;
        }

        // Get a specified user's photo.
        public async Task<List<ResultsItem>> GetUserPhoto(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the user's photo.
            using (Stream photo = await graphClient.Users[id].Photo.Content.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).GetAsync())
            {
                if (photo != null)
                {

                    // Get byte[] for display.
                    using (BinaryReader reader = new BinaryReader(photo))
                    {
                        byte[] data = reader.ReadBytes((int)photo.Length);
                        items.Add(new ResultsItem
                        {
                            Properties = new Dictionary<string, object>
                            {
                                { "Stream", data }
                            }
                        });
                    }
                }
            }
            return items;
        }

        // Get the direct reports of a specified user.
        // This snippet requires an admin work account.
        public async Task<List<ResultsItem>> GetDirectReports(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get user's direct reports.
            IUserDirectReportsCollectionWithReferencesPage directs = await graphClient.Users[id].DirectReports.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).GetAsync();

            if (directs?.Count > 0)
            {
                foreach (User user in directs)
                {

                    // Get user properties.
                    items.Add(new ResultsItem
                    {
                        Display = user.DisplayName,
                        Id = user.Id,
                        Properties = new Dictionary<string, object>
                        {
                            { Resource.Prop_Upn, user.UserPrincipalName },
                            { Resource.Prop_Id, user.Id }
                        }
                    });
                }
            }
            return items;
        }

        // Update a user.
        // This snippet changes the user's display name. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> UpdateUser(string id, string name)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Update the user.
            await graphClient.Users[id].Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).UpdateAsync(new User
            {
                DisplayName = Resource.Updated + name
            });

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

        // Delete a user. Warning: This operation cannot be undone.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> DeleteUser(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            // Make sure that the current user is not selected.
            User me = await graphClient.Me.Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).Select("id").GetAsync();
            if (id == me.Id)
            {
                item.Properties.Add(Resource.User_ChooseAnotherUser, "");
            }
            else
            {

                // Delete the user.
                await graphClient.Users[id].Request(requestOptions).WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount()).DeleteAsync();

                // This operation doesn't return anything.
                item.Properties.Add(Resource.No_Return_Data, "");
            }
            items.Add(item);
            return items;
        }
    }
}
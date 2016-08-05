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
using System.Linq;
using System.IO;
using Resources;

namespace Microsoft_Graph_ASPNET_Snippets.Controllers.Users
{
    [Authorize]
    public class UsersController : Controller
    {

        // Load the view.
        public ActionResult Index()
        {
            return View("Users");
        }

        // Get all users.
        public async Task<ActionResult> GetUsers()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                IGraphServiceUsersCollectionPage users = await graphClient.Users.Request().GetAsync();

                // Populate the view model.
                if (users?.Count > 0)
                {
                    foreach (User user in users)
                    {

                        // Filter out conference rooms.
                        string displayName = user.DisplayName ?? "";
                        if (!displayName.StartsWith("Conf Room"))
                        {
                            items.Add(new ResultsItem
                            {
                                Display = user.DisplayName,
                                Id = user.Id
                            });
                        }
                    }
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Get the current user's profile.
        public async Task<ActionResult> GetMe()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                    
                // Get the current user's profile.
                User me = await graphClient.Me.Request().GetAsync();
                
                if (me != null)
                {

                    // Get user properties.
                    item.Display = me.DisplayName;
                    item.Id = me.Id;
                    item.Properties.Add(Resource.Prop_Upn, me.UserPrincipalName);
                    item.Properties.Add(Resource.Prop_Id, me.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Get the current user's manager.
        public async Task<ActionResult> GetMyManager()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                    
                // Get the current user's manager.
                User manager = await graphClient.Me.Manager.Request().GetAsync() as User; 
                
                if (manager != null)
                {

                    // Get user properties.
                    item.Display = manager.DisplayName;
                    item.Id = manager.Id;
                    item.Properties.Add(Resource.Prop_Upn, manager.UserPrincipalName);
                    item.Properties.Add(Resource.Prop_Id, manager.Id);

                    items.Add(item);
                }
                results.Items = items;
            }

            // Known issue: Throws exception if manager is null, with message "Resource 'manager' does not exist or one of its queried reference-property objects are not present."
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Get the current user's photo. 
        public async Task<ActionResult> GetMyPhoto()
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                using (Stream photo = await graphClient.Me.Photo.Content.Request().GetAsync())
                {
                    if (photo != null)
                    {

                        // Get byte[] for display.
                        using (BinaryReader reader = new BinaryReader(photo))
                        {
                            byte[] data = reader.ReadBytes((int)photo.Length);
                            item.Properties.Add("Stream", data);

                            items.Add(item);
                        }
                    }
                    results.Items = items;
                }
            }

            //Known issue: Throws exception if photo is null, with message "The photo wasn't found."
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Create a new user in the signed-in user's tenant.
        // This snippet requires an admin work account. 
        public async Task<ActionResult> CreateUser()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            string guid = Guid.NewGuid().ToString();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // This snippet gets the tenant domain from the Organization object to construct the user's email address.
                IGraphServiceOrganizationCollectionPage organization = await graphClient.Organization.Request().GetAsync();
                string alias = Resource.User.ToLower() + guid.Substring(0, 8);
                string domain = organization.CurrentPage[0].VerifiedDomains.ElementAt(0).Name;

                // Add the user.
                User user = await graphClient.Users.Request().AddAsync(new User
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
                    item.Display = user.DisplayName;
                    item.Id = user.Id;
                    item.Properties.Add(Resource.Prop_Upn, user.UserPrincipalName);
                    item.Properties.Add(Resource.Prop_Id, user.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Get a specified user.
        public async Task<ActionResult> GetUser(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the user.
                User user = await graphClient.Users[id].Request().GetAsync();
                
                if (user != null)
                {

                    // Get user properties.
                    item.Display = user.DisplayName;
                    item.Id = user.Id;
                    item.Properties.Add(Resource.Prop_Upn, user.UserPrincipalName);
                    item.Properties.Add(Resource.Prop_Id, user.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Get a specified user's photo.
        public async Task<ActionResult> GetUserPhoto(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the user's photo.
                using (Stream photo = await graphClient.Users[id].Photo.Content.Request().GetAsync())
                {
                    if (photo != null)
                    {

                        // Get byte[] for display.
                        using (BinaryReader reader = new BinaryReader(photo))
                        {
                            byte[] data = reader.ReadBytes((int)photo.Length);
                            item.Properties.Add("Stream", data);

                            items.Add(item);
                        }
                    }
                    results.Items = items;
                }
            }


            // Throws an exception when requesting the photo for unlicensed users (such as those created by this sample), with message "The requested user 'userf0c1fe9a@MOD182601.onmicrosoft.com' is invalid."
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Get the direct reports of a specified user.
        public async Task<ActionResult> GetDirectReports(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get user's direct reports.
                IUserDirectReportsCollectionWithReferencesPage directs = await graphClient.Users[id].DirectReports.Request().GetAsync();
                
                if (directs?.Count > 0)
                {
                    foreach (User user in directs)
                    {
                        ResultsItem item = new ResultsItem
                        {
                            Display = user.DisplayName,
                            Id = user.Id
                        };

                        // Get user properties.
                        item.Properties.Add(Resource.Prop_Name, user.DisplayName);
                        item.Properties.Add(Resource.Prop_Upn, user.UserPrincipalName);
                        item.Properties.Add(Resource.Prop_Id, user.Id);

                        items.Add(item);
                    }
                    results.Items = items;
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Update a user.
        // This snippet changes the user's display name. 
        // This snippet requires an admin work account. 
        public async Task<ActionResult> UpdateUser(string id, string name)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                
                // Change user display name.
                User userToUpdate = new User
                {
                    DisplayName = Resource.Updated + name
                };

                // Update the user.
                await graphClient.Users[id].Request().UpdateAsync(userToUpdate);
                
                // This operation doesn't return anything.
                item.Properties.Add(Resource.No_Return_Data, "");
                items.Add(item);
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }

        // Delete a user. Warning: This operation cannot be undone.
        // This snippet requires an admin work account. 
        public async Task<ActionResult> DeleteUser(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Make sure that the current user is not selected.
                User me = await graphClient.Me.Request().Select("id").GetAsync();
                if (id == me.Id)
                {

                    // Selected item is not supported.
                    results.Selectable = false;
                    item.Properties.Add(Resource.User_ChooseAnotherUser, "");
                }
                else
                {

                    // Delete the user.
                    await graphClient.Users[id].Request().DeleteAsync();
                    
                    // This operation doesn't return anything.
                    item.Properties.Add(Resource.No_Return_Data, "");
                    items.Add(item);
                    results.Items = items;
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Users", results);
        }
    }
}
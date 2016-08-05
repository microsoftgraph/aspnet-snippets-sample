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

namespace Microsoft_Graph_ASPNET_Snippets.Controllers.Groups
{
    [Authorize]
    public class GroupsController : Controller
    {
        public ActionResult Index()
        {
            return View("Groups");
        }

        // Get groups. 
        // This snippet requires an admin work account. 
        public async Task<ActionResult> GetGroups()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                
                // Get groups.
                IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().GetAsync();
                
                if (groups?.Count > 0)
                {
                    foreach (Group group in groups)
                    {
                        items.Add(new ResultsItem
                        {
                            Display = group.DisplayName,
                            Id = group.Id
                        });
                    }
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Groups", results);
        }

        // Get Office 365 (unified) groups. 
        // This snippet requires an admin work account. 
        public async Task<ActionResult> GetUnifiedGroups()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                
                // Get unified groups.
                IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().Filter("groupTypes/any(a:a%20eq%20'unified')").GetAsync();
                
                if (groups?.Count > 0)
                {
                    foreach (Group group in groups)
                    {
                        items.Add(new ResultsItem
                        {
                            Display = group.DisplayName,
                            Id = group.Id
                        });
                    }
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Groups", results);
        }

        // Get the groups that the current user is a direct member of.
        // This snippet requires an admin work account.
        public async Task<ActionResult> GetMyMemberOfGroups()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get groups the current user is a direct member of.
                IUserMemberOfCollectionWithReferencesPage memberOfGroups = await graphClient.Me.MemberOf.Request().GetAsync();
                
                if (memberOfGroups?.Count > 0)
                {
                    foreach (var directoryObject in memberOfGroups)
                    {

                        // We only want groups, so ignore DirectoryRole objects.
                        if (directoryObject is Group)
                        {
                            Group group = directoryObject as Group;
                            items.Add(new ResultsItem
                            {
                                Display = group.DisplayName,
                                Id = group.Id
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
            return View("Groups", results);
        }

        // Create a new group. It can be an Office 365 group, dynamic group, or security group.
        // This snippet creates an Office 365 (unified) group.
        // This snippet requires an admin work account. 
        public async Task<ActionResult> CreateGroup()
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();
            string guid = Guid.NewGuid().ToString();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                    
                // Add the group.
                Group group = await graphClient.Groups.Request().AddAsync(new Group
                {
                    GroupTypes = new List<string> { "Unified" },
                    DisplayName = Resource.Group + guid.Substring(0, 8),
                    Description = Resource.Group + guid,
                    MailNickname = Resource.Group.ToLower() + guid.Substring(0, 8),
                    MailEnabled = false,
                    SecurityEnabled = false
                });

                if (group != null)
                {
                    
                    // Get group properties.
                    item.Display = group.DisplayName;
                    item.Id = group.Id;
                    item.Properties.Add(Resource.Prop_Description, group.Description);
                    item.Properties.Add(Resource.Prop_Email, group.Mail);
                    item.Properties.Add(Resource.Prop_Created, group.AdditionalData["createdDateTime"]);
                    item.Properties.Add(Resource.Prop_Id, group.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Groups", results);
        }

        // Get a specified group.
        // This snippet requires an admin work account. 
        public async Task<ActionResult> GetGroup(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                    
                // Get the group.
                Group group = await graphClient.Groups[id].Request().Expand("members").GetAsync();

                if (group != null)
                {

                    // Get group properties.
                    item.Display = group.DisplayName;
                    item.Id = group.Id;
                    item.Properties.Add(Resource.Prop_Email, group.Mail);
                    item.Properties.Add(Resource.Prop_MemberCount, group.Members?.Count);
                    item.Properties.Add(Resource.Prop_Id, group.Id);

                    items.Add(item);
                }
                results.Items = items;
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Groups", results);
        }

        // Read properties and relationships of group members.
        // This snippet requires an admin work account. 
        public async Task<ActionResult> GetMembers(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                   
                // Get group members. 
                IGroupMembersCollectionWithReferencesPage members = await graphClient.Groups[id].Members.Request().GetAsync();

                if (members?.Count > 0)
                {
                    foreach (User user in members)
                    {
                        ResultsItem item = new ResultsItem();

                        // Get member properties.
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
            return View("Groups", results);
        }

        // Read properties and relationships of group members.
        // This snippet requires an admin work account. 
        public async Task<ActionResult> GetOwners(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                    
                // Get group owners.
                IGroupOwnersCollectionWithReferencesPage members = await graphClient.Groups[id].Owners.Request().GetAsync();
                
                if (members?.Count > 0)
                {
                    foreach (User user in members)
                    {
                        ResultsItem item = new ResultsItem();

                        // Get owner properties.
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
            return View("Groups", results);
        }

        // Update a group.
        // This snippet changes the group name. 
        // This snippet requires an admin work account. 
        public async Task<ActionResult> UpdateGroup(string id, string name)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Update the group.
                await graphClient.Groups[id].Request().UpdateAsync(new Group
                {
                    DisplayName = Resource.Updated + name
                });
                
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
            return View("Groups", results);
        }

        // Delete a group. Warning: This operation cannot be undone.
        // This snippet requires an admin work account. 
        public async Task<ActionResult> DeleteGroup(string id)
        {
            ResultsViewModel results = new ResultsViewModel();
            results.Selectable = false;
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Delete the group.
                await graphClient.Groups[id].Request().DeleteAsync();
                
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
            return View("Groups", results);
        }
    }
}

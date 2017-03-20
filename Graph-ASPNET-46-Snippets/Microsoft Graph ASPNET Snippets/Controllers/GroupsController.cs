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

namespace Microsoft_Graph_ASPNET_Snippets.Controllers.Groups
{
    [Authorize]
    public class GroupsController : Controller
    {
        GroupsService groupsService = new GroupsService();

        public ActionResult Index()
        {
            return View("Groups");
        }

        // Get groups. 
        // This snippet requires an admin work account. 
        public async Task<ActionResult> GetGroups()
        {
            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get groups.
                results.Items = await groupsService.GetGroups(graphClient);
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
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get unified groups.
                results.Items = await groupsService.GetUnifiedGroups(graphClient);
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
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get groups the current user is a direct member of.
                results.Items = await groupsService.GetMyMemberOfGroups(graphClient);
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
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Add the group.
                results.Items = await groupsService.CreateGroup(graphClient);
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
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get the group.
                results.Items = await groupsService.GetGroup(graphClient, id);
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
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get group members.
                results.Items = await groupsService.GetMembers(graphClient, id);
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
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get group owners.
                results.Items = await groupsService.GetOwners(graphClient, id);
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
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Update the group.
                results.Items = await groupsService.UpdateGroup(graphClient, id, name);
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
            ResultsViewModel results = new ResultsViewModel(false);
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Delete the group.
                results.Items = await groupsService.DeleteGroup(graphClient, id);
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

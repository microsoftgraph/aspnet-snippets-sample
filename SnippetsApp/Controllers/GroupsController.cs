// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using SnippetsApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SnippetsApp.Controllers
{
    [AuthorizeForScopes(Scopes = new[] { GraphConstants.GroupReadWriteAll })]
    public class GroupsController : BaseController
    {
        private readonly string[] _groupScopes =
            new [] { GraphConstants.GroupReadWriteAll };

        public GroupsController(
            GraphServiceClient graphClient,
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger) : base(graphClient, tokenAcquisition, logger)
        {
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET /Groups?groupId=""
        public async Task<IActionResult> Display(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return RedirectToAction("List")
                    .WithError("Group ID cannot be empty.");
            }

            await EnsureScopes(_groupScopes);

            // Initialize the model
            var model = new GroupWithPhoto();

            try
            {
                // GET /groups/groupId
                model.Group = await _graphClient.Groups[groupId]
                    .Request()
                    // Select just the fields used by the app
                    .Select(g => new
                    {
                        g.Description,
                        g.DisplayName,
                        g.GroupTypes,
                        g.Id,
                        g.MailEnabled,
                        g.SecurityEnabled
                    })
                    .GetAsync();
                try
                {
                    // Get the full size photo
                    // GET /groups/groupId/photo/$value
                    var photoStream = await _graphClient.Groups[groupId]
                        .Photo
                        .Content
                        .Request()
                        .GetAsync();

                    model.AddGroupPhoto(photoStream);
                }
                catch (ServiceException ex)
                {
                    // Call can return 404 for a lot of different reasons
                    // (Group has no photo, Group isn't a type that supports photo, etc.)
                    // So just check for 404, not specific error codes
                    if (!ex.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        throw;
                    }
                }


                return View(model);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error getting group with ID {groupId}", ex.Error.Message);
            }
        }

        // GET /Groups/List
        public async Task<IActionResult> List()
        {
            await EnsureScopes(_groupScopes);

            // Initialize the model
            var model = new GroupsListDisplayModel();

            try
            {
                // Get all groups
                // GET /groups
                var groupsPage = await _graphClient.Groups
                    .Request()
                    // Select just the fields used by the app
                    .Select(g => new
                    {
                        g.DisplayName,
                        g.GroupTypes,
                        g.Id
                    })
                    // Get max PageSize number of results
                    .Top(GraphConstants.PageSize)
                    // Sort by display name
                    .OrderBy("displayName")
                    .GetAsync();

                if (groupsPage.NextPageRequest == null)
                {
                    model.AllGroups = groupsPage.CurrentPage;
                }
                else
                {
                    model.AllGroups = await GetAllPages<Group>(
                        _graphClient, groupsPage);
                }

                // Get only unified groups
                // Same as previous request, except
                // filtered, and without a sort
                // (Sort not supported when filtering groups)
                var unifiedGroupsPage = await _graphClient.Groups
                    .Request()
                    .Filter("groupTypes/any(a:a%20eq%20'unified')")
                    .Select(g => new
                    {
                        g.DisplayName,
                        g.GroupTypes,
                        g.Id
                    })
                    .Top(GraphConstants.PageSize)
                    .GetAsync();

                if (unifiedGroupsPage.NextPageRequest == null)
                {
                    model.UnifiedGroups = unifiedGroupsPage.CurrentPage;
                }
                else
                {
                    model.UnifiedGroups = await GetAllPages<Group>(
                        _graphClient, unifiedGroupsPage);
                }

                // Get groups user is a member of
                // GET /me/memberOf
                var membershipPage = await _graphClient.Me.MemberOf
                    .Request()
                    .Select("displayName,groupTypes,id")
                    .Top(GraphConstants.PageSize)
                    .GetAsync();

                // This method casts the returned DirectoryObjects
                // as Group object, and filters out any non-Group objects
                model.GroupMemberships = await GetAllPagesAsType<Group>(
                    _graphClient, membershipPage);

                // Get groups user owns
                // GET /me/ownedObjects
                var ownershipPage = await _graphClient.Me.OwnedObjects
                    .Request()
                    .Select("displayName,groupTypes,id")
                    .Top(GraphConstants.PageSize)
                    .GetAsync();

                model.OwnedGroups = await GetAllPagesAsType<Group>(
                    _graphClient, ownershipPage);

                return View(model);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Error", "home")
                    .WithError($"Error getting groups", ex.Error.Message);
            }
        }

        // GET /Groups/Create
        public async Task<IActionResult> Create()
        {
            await EnsureScopes(_groupScopes);
            return View();
        }

        // POST /Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string displayName,
                                                string mailNickname,
                                                string description,
                                                bool mailEnabled,
                                                bool securityEnabled)
        {
            if (string.IsNullOrEmpty(displayName) ||
                string.IsNullOrEmpty(mailNickname))
            {
                return RedirectToAction("List")
                    .WithError("Invalid data. You must supply a value for Display name and Mail nickname");
            }

            await EnsureScopes(_groupScopes);

            try
            {
                // Create a new group with the supplied values
                var newGroup = new Group
                {
                    Description = description,
                    DisplayName = displayName,
                    MailEnabled = mailEnabled,
                    MailNickname = mailNickname,
                    SecurityEnabled = securityEnabled,
                    GroupTypes = new List<string> { "Unified" },
                    AdditionalData = new Dictionary<string, object>()
                };

                var userIds = new List<string>
                {
                    $"https://graph.microsoft.com/v1.0/users/{User.GetObjectId()}"
                };

                // Initialize the group with the logged on user as the owner
                // and only member
                newGroup.AdditionalData.Add("members@odata.bind", userIds);

                // Graph automatically adds the user who created a group
                // as the owner, leaving this to show how to add owners
                newGroup.AdditionalData.Add("owners@odata.bind", userIds);

                // POST /groups
                await _graphClient.Groups
                    .Request()
                    .AddAsync(newGroup);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error creating group", ex.Error.Message);
            }

            return RedirectToAction("List")
                .WithSuccess("Group created");
        }

        // POST /Groups/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string groupId, string displayName)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return RedirectToAction("List")
                    .WithError("Group ID cannot be empty.");
            }

            await EnsureScopes(_groupScopes);

            try
            {
                // Create a new Group object with just the
                // properties to update
                var updateGroup = new Group
                {
                    DisplayName = displayName
                };

                // PATCH /groups/groupId
                await _graphClient.Groups[groupId]
                    .Request()
                    .UpdateAsync(updateGroup);

                return RedirectToAction("List")
                    .WithSuccess("Group updated");
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error updating group with ID {groupId}", ex.Error.Message);
            }
        }

        // POST /Groups/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string groupId)
        {

            if (string.IsNullOrEmpty(groupId))
            {
                return RedirectToAction("List")
                    .WithError("Group ID cannot be empty.");
            }

            await EnsureScopes(_groupScopes);

            try
            {
                // DELETE /groups/groupId
                await _graphClient.Groups[groupId]
                    .Request()
                    .DeleteAsync();

                return RedirectToAction("List")
                    .WithSuccess("Group deleted");
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error deleteing group with ID {groupId}", ex.Error.Message);
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using SnippetsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SnippetsApp.Controllers
{
    [AuthorizeForScopes(Scopes = new[] {
        GraphConstants.GroupReadWriteAll,
        GraphConstants.UserReadWriteAll })]
    public class TeamsController : BaseController
    {
        private readonly string[] teamScopes =
            new[] {
                GraphConstants.GroupReadWriteAll,
                GraphConstants.UserReadWriteAll
            };

        public TeamsController(
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger) : base(tokenAcquisition, logger)
        {
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET /Teams/List
        // Get the list of teams, groups that teams can be added to,
        // and teams the user is a member of
        public async Task<IActionResult> List()
        {
            var model = new TeamsListDisplayModel();

            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                // Get all groups
                // Graph v1 doesn't support filtering on resourceProvisioningOptions,
                // which is the property that tells us if the group has a team or not
                // So we'll get all unified groups, then sort them into their respective
                // lists in the display model

                IList<Group> allGroups;

                // GET /groups
                var groupsPage = await graphClient.Groups
                    .Request()
                    // Only get unified groups (Teams groups must be unified)
                    .Filter("groupTypes/any(a:a%20eq%20'unified')")
                    // Only get the fields used by the view
                    .Select("displayName,id,resourceProvisioningOptions")
                    // Limit results to the default page size
                    .Top(GraphConstants.PageSize)
                    .GetAsync();

                // If there are more results available, use a
                // page iterator to get all results
                if (groupsPage.NextPageRequest == null)
                {
                    allGroups = groupsPage.CurrentPage;
                }
                else
                {
                    allGroups = await GetAllPages<Group>(
                        graphClient, groupsPage);
                }

                // Add each group to the appropriate list
                foreach (var group in allGroups)
                {
                    // Groups with Teams will have "Team" in their resourceProvisioningOptions
                    var rpOptions = group.AdditionalData["resourceProvisioningOptions"]
                        as Newtonsoft.Json.Linq.JArray;

                    var optionsArray = rpOptions.ToObject<string[]>();

                    if (optionsArray.Contains("team", StringComparer.InvariantCultureIgnoreCase))
                    {
                        model.AllTeams.Add(group);
                    }
                    else
                    {
                        model.AllNonTeamGroups.Add(group);
                    }
                }

                // GET /me/joinedTeams
                var joinedTeamsPage = await graphClient.Me
                    .JoinedTeams
                    .Request()
                    .GetAsync();

                if (joinedTeamsPage.NextPageRequest == null)
                {
                    model.JoinedTeams = joinedTeamsPage.CurrentPage;
                }
                else
                {
                    model.JoinedTeams = await GetAllPages<Team>(
                        graphClient, joinedTeamsPage);
                }

                return View(model);
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Error", "Home")
                    .WithError($"Error getting teams",
                        ex.Error.Message);
            }
        }

        // GET /Teams/Display?teamId=""
        // teamId: ID of the team to display
        // Displays the details of a team, including
        // settings, channels, and installed apps
        public async Task<IActionResult> Display(string teamId)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                return RedirectToAction("List")
                    .WithError("Team ID cannot be empty.");
            }

            var model = new TeamDisplayModel();

            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                // GET /teams/teamId
                model.Team = await graphClient.Teams[teamId]
                    .Request()
                    .GetAsync();

                // GET /teams/teamId/channels
                var channels = await graphClient.Teams[teamId]
                    .Channels
                    .Request()
                    .GetAsync();

                model.Channels = channels.CurrentPage;

                // Querying installed apps on an archived team
                // gives an error
                if (!model.Team.IsArchived.Value)
                {
                    // GET /teams/teamId/installedApps
                    var installedApps = await graphClient.Teams[teamId]
                        .InstalledApps
                        .Request()
                        // Expand the teamsAppDefinition for details
                        .Expand("teamsAppDefinition")
                        .GetAsync();

                    model.InstalledApps = installedApps.CurrentPage;
                }
                else
                {
                    model.InstalledApps = new List<TeamsAppInstallation>();
                }

                return View(model);
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error getting team with ID {teamId}",
                        ex.Error.Message);
            }
        }

        // GET /Teams/Create
        // Gets a form to get details on creating a new group
        // with a team
        public IActionResult Create()
        {
            return View();
        }

        // POST /Teams/Create
        // teamName: The display name for the new group/team
        // teamDescription: The description for the new group/team
        // teamMailNickname: The mail nickname for the new group/team
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string teamName,
                                                string teamDescription,
                                                string teamMailNickname)
        {
            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                var userUrl = $"https://graph.microsoft.com/v1.0/users/{User.GetObjectId()}";

                // Initialize the new group
                var newGroup = new Group
                {
                    DisplayName = teamName,
                    Description = teamDescription,
                    MailEnabled = true,
                    MailNickname = teamMailNickname,
                    SecurityEnabled = false,
                    GroupTypes = new string[] { "Unified" },
                    AdditionalData = new Dictionary<string, object> {
                        { "members@odata.bind", new string[] { userUrl } }
                    }
                };

                // POST /groups
                // Creates the new group
                var createdGroup = await graphClient.Groups
                    .Request()
                    .AddAsync(newGroup);

                // Initialize team settings
                var newTeam = new Team
                {
                    // Set ODataType to null - API returns an error
                    // if the odata.type field is included in the payload
                    ODataType = null,
                    // Explicitly set guest settings to now allow
                    // create/delete channels
                    GuestSettings = new TeamGuestSettings
                    {
                        ODataType = null,
                        AllowCreateUpdateChannels = false,
                        AllowDeleteChannels = false
                    }
                };

                // Add the team to the group
                // PUT /groups/groupId/team
                await graphClient.Groups[createdGroup.Id]
                    .Team
                    .Request()
                    .PutAsync(newTeam);

                return RedirectToAction("Display", new { teamId = createdGroup.Id })
                    .WithSuccess("Team and group created");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error creating group and team",
                        ex.Error.Message);
            }
        }

        // POST /Teams/ArchiveTeam?teamId=""&archiveAction=""
        // teamId: The ID of the team to archive/unarchive
        // archiveAction: Which action to take: archive or unarchive
        // Archives or unarchives the specified team
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveTeam(string teamId,
                                                     string archiveAction)
        {
            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                if (string.Compare(archiveAction, "unarchive", true) == 0)
                {
                    // POST /teams/teamId/unarchive
                    await graphClient.Teams[teamId]
                        .Unarchive()
                        .Request()
                        .PostAsync();
                }
                else
                {
                    // POST /teams/teamId/archive
                    await graphClient.Teams[teamId]
                        .Archive()
                        .Request()
                        .PostAsync();
                }

                return RedirectToAction("Display", new { teamId = teamId })
                    .WithSuccess($"Request to {archiveAction.ToLower()} team succeeded. It may take some time before the operation completes.");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { teamId = teamId })
                    .WithError($"Error changing archive state of team",
                        ex.Error.Message);
            }
        }

        // POST /Teams/AddTeamToGroup?groupId=""
        // groupId: The ID of the group to add a team to
        // Adds a team to an existing group
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeamToGroup(string groupId)
        {
            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                // Initialize the team settings
                var newTeam = new Team
                {
                    ODataType = null,
                    GuestSettings = new TeamGuestSettings
                    {
                        ODataType = null,
                        AllowCreateUpdateChannels = false,
                        AllowDeleteChannels = false
                    }
                };

                // PUT /groups/groupId/team
                await graphClient.Groups[groupId]
                    .Team
                    .Request()
                    .PutAsync(newTeam);

                return RedirectToAction("Display", new { teamId = groupId })
                    .WithSuccess("Team added to group");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error adding team to group with ID {groupId}",
                        ex.Error.Message);
            }
        }

        // POST /Teams/CreateChannel?teamId=""&channelName=""&channelDescription=""
        // teamId: The ID of the team to add a channel to
        // channelName: The name of the new channel
        // channelDescription: The description of the new channel
        // Creates a new channel in the specified team
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChannel(string teamId,
                                                       string channelName,
                                                       string channelDescription)
        {
            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                // Initialize the new channel
                var newChannel = new Channel
                {
                    DisplayName = channelName,
                    Description = channelDescription
                };

                // POST /teams/teamId/channels
                await graphClient.Teams[teamId]
                    .Channels
                    .Request()
                    .AddAsync(newChannel);

                return RedirectToAction("Display", new { teamId = teamId })
                    .WithSuccess($"Channel {channelName} created");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { teamId = teamId })
                    .WithError($"Error creating channel {channelName}",
                        ex.Error.Message);
            }
        }

        // POST /Teams/UpdateSettings
        // updatedTeam: A Team object deserialized from form values with updated settings
        // Updates the settings for a team
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(Team updatedTeam)
        {
            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                // PATCH /teams/teamId
                await graphClient.Teams[updatedTeam.Id]
                    .Request()
                    .UpdateAsync(updatedTeam);

                return RedirectToAction("Display", new { teamId = updatedTeam.Id })
                    .WithSuccess("Settings updated");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { teamId = updatedTeam.Id })
                    .WithError($"Error updating team",
                        ex.Error.Message);
            }
        }

        // GET /Teams/PostMessageToChannel?channelId=""&teamId=""
        // channelId: The ID of the channel to post to
        // teamId: The ID of the team containing the channel
        // Gets the post message form
        public IActionResult PostMessageToChannel(string channelId, string teamId)
        {
            var model = new TeamMessageDisplayModel
            {
                ChannelId = channelId,
                TeamId = teamId
            };

            return View(model);
        }

        // POST /Teams/PostMessageToChannel
        // model: Deserialized from form values
        // Posts a new message to the specified channel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostMessageToChannel(TeamMessageDisplayModel model)
        {
            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                // Initialize the chat message
                var newMessage = new ChatMessage
                {
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = model.Message
                    }
                };

                // POST /teams/teamId/channelds/channelId/messages
                await graphClient.Teams[model.TeamId]
                    .Channels[model.ChannelId]
                    .Messages
                    .Request()
                    .AddAsync(newMessage);

                return RedirectToAction("Display", new { teamId = model.TeamId })
                    .WithSuccess("Message posted");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { teamId = model.TeamId })
                    .WithError($"Error posting message",
                        ex.Error.Message);
            }
        }

        // POST /Teams/DeleteApp
        // appId: The ID of the app to delete (uninstall)
        // teamId: The ID of the team to remove the app from
        // Uninstalls an app from a team
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApp(string appId,
                                                   string teamId)
        {
            try
            {
                var graphClient = GetGraphClientForScopes(teamScopes);

                // DELETE /teams/teamId/installedApps/appId
                await graphClient.Teams[teamId]
                    .InstalledApps[appId]
                    .Request()
                    .DeleteAsync();

                return RedirectToAction("Display", new { teamId = teamId })
                    .WithSuccess("App uninstalled");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("Display", new { teamId = teamId })
                    .WithError($"Error uninstalling app with ID {appId}",
                        ex.Error.Message);
            }
        }
    }
}

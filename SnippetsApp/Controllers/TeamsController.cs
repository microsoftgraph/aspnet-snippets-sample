// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using SnippetsApp.Models;

namespace SnippetsApp.Controllers
{
    [Authorize]
    public class TeamsController : BaseController
    {
        private readonly string[] _teamsScopes =
            new [] { GraphConstants.GroupReadWriteAll,
                     GraphConstants.UserReadWriteAll };

        private readonly string[] _teamsDetailsScopes =
            new [] { GraphConstants.ChannelCreate,
                     GraphConstants.ChannelSettingsReadWriteAll,
                     GraphConstants.TeamsAppInstallationReadWriteForTeam,
                     GraphConstants.TeamSettingsReadWriteAll };

        private readonly string[] _teamsCreateScopes =
            new [] { GraphConstants.TeamCreate };

        private readonly string[] _teamsChannelMessageSendScopes =
            new [] { GraphConstants.ChannelMessageSend };

        public TeamsController(
            GraphServiceClient graphClient,
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger) : base(graphClient, tokenAcquisition, logger)
        {
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET /Teams/List
        // Get the list of teams, groups that teams can be added to,
        // and teams the user is a member of
        [AuthorizeForScopes(Scopes = new[] {
        GraphConstants.GroupReadWriteAll,
        GraphConstants.UserReadWriteAll })]
        public async Task<IActionResult> List()
        {
            var model = new TeamsListDisplayModel();

            await EnsureScopes(_teamsScopes);

            try
            {
                // Get all groups
                // Graph v1 doesn't support filtering on resourceProvisioningOptions,
                // which is the property that tells us if the group has a team or not
                // So we'll get all unified groups, then sort them into their respective
                // lists in the display model

                IList<Group> allGroups;

                // GET /groups
                var groupsPage = await _graphClient.Groups
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
                        _graphClient, groupsPage);
                }

                // Add each group to the appropriate list
                foreach (var group in allGroups)
                {
                    // Groups with Teams will have "Team" in their resourceProvisioningOptions
                    if (group.AdditionalData["resourceProvisioningOptions"] is JsonElement rpOptions
                        && rpOptions.ValueKind == JsonValueKind.Array
                        && rpOptions.EnumerateArray().Any(e => e.GetString().Equals("team", StringComparison.InvariantCultureIgnoreCase)))
                    //if (rpOptions != null && rpOptions.Contains("team", StringComparer.InvariantCultureIgnoreCase))
                    //if (optionsArray.Contains("team", StringComparer.InvariantCultureIgnoreCase))
                    {
                        model.AllTeams.Add(group);
                    }
                    else
                    {
                        model.AllNonTeamGroups.Add(group);
                    }
                }

                // GET /me/joinedTeams
                var joinedTeamsPage = await _graphClient.Me
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
                        _graphClient, joinedTeamsPage);
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
        [AuthorizeForScopes(Scopes = new[] {
            GraphConstants.ChannelCreate,
            GraphConstants.ChannelSettingsReadWriteAll,
            GraphConstants.TeamsAppInstallationReadWriteForTeam,
            GraphConstants.TeamSettingsReadWriteAll })]
        public async Task<IActionResult> Display(string teamId)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                return RedirectToAction("List")
                    .WithError("Team ID cannot be empty.");
            }

            await EnsureScopes(_teamsDetailsScopes);

            var model = new TeamDisplayModel();

            try
            {
                // GET /teams/teamId
                model.Team = await _graphClient.Teams[teamId]
                    .Request()
                    .GetAsync();

                // GET /teams/teamId/channels
                var channels = await _graphClient.Teams[teamId]
                    .Channels
                    .Request()
                    .GetAsync();

                model.Channels = channels.CurrentPage;

                // Querying installed apps on an archived team
                // gives an error
                if (!model.Team.IsArchived.Value)
                {
                    // GET /teams/teamId/installedApps
                    var installedApps = await _graphClient.Teams[teamId]
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
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.TeamCreate })]
        public async Task<IActionResult> Create()
        {
            await EnsureScopes(_teamsCreateScopes);
            return View();
        }

        // POST /Teams/Create
        // teamName: The display name for the new group/team
        // teamDescription: The description for the new group/team
        // teamMailNickname: The mail nickname for the new group/team
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.TeamCreate })]
        public async Task<IActionResult> Create(string teamName,
                                                string teamDescription)
        {
            await EnsureScopes(_teamsCreateScopes);

            try
            {
                var newTeam = new Team
                {
                    DisplayName = teamName,
                    Description = teamDescription,
                    // Explicitly set guest settings to now allow
                    // create/delete channels
                    GuestSettings = new TeamGuestSettings
                    {
                        AllowCreateUpdateChannels = false,
                        AllowDeleteChannels = false
                    },
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"template@odata.bind", "https://graph.microsoft.com/v1.0/teamsTemplates('standard')"}
                    }
                };

                // POST /teams
                var result = await _graphClient.Teams
                    .Request()
                    .AddResponseAsync(newTeam);

                if (result.HttpHeaders.TryGetValues("Location", out var locationValues))
                {
                    var newTeamId = locationValues?.First().Split('\'')[1];

                    return RedirectToAction("Display", new { teamId = newTeamId })
                        .WithSuccess("Team created");
                }

                return RedirectToAction("List")
                    .WithSuccess("Team created");
            }
            catch(ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return RedirectToAction("List")
                    .WithError($"Error creating team",
                        ex.Error.Message);
            }
        }

        // POST /Teams/ArchiveTeam?teamId=""&archiveAction=""
        // teamId: The ID of the team to archive/unarchive
        // archiveAction: Which action to take: archive or unarchive
        // Archives or unarchives the specified team
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = new[] {
            GraphConstants.ChannelCreate,
            GraphConstants.ChannelSettingsReadWriteAll,
            GraphConstants.TeamsAppInstallationReadWriteForTeam,
            GraphConstants.TeamSettingsReadWriteAll })]
        public async Task<IActionResult> ArchiveTeam(string teamId,
                                                     string archiveAction)
        {
            await EnsureScopes(_teamsDetailsScopes);

            try
            {
                if (string.Compare(archiveAction, "unarchive", true) == 0)
                {
                    // POST /teams/teamId/unarchive
                    await _graphClient.Teams[teamId]
                        .Unarchive()
                        .Request()
                        .PostAsync();
                }
                else
                {
                    // POST /teams/teamId/archive
                    await _graphClient.Teams[teamId]
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
        [AuthorizeForScopes(Scopes = new[] {
            GraphConstants.GroupReadWriteAll,
            GraphConstants.UserReadWriteAll })]
        public async Task<IActionResult> AddTeamToGroup(string groupId)
        {
            await EnsureScopes(_teamsScopes);

            try
            {
                // Initialize the team settings
                var newTeam = new Team
                {
                    ODataType = null,
                    GuestSettings = new TeamGuestSettings
                    {
                        AllowCreateUpdateChannels = false,
                        AllowDeleteChannels = false
                    }
                };

                // PUT /groups/groupId/team
                await _graphClient.Groups[groupId]
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
        [AuthorizeForScopes(Scopes = new[] {
            GraphConstants.ChannelCreate,
            GraphConstants.ChannelSettingsReadWriteAll,
            GraphConstants.TeamsAppInstallationReadWriteForTeam,
            GraphConstants.TeamSettingsReadWriteAll })]
        public async Task<IActionResult> CreateChannel(string teamId,
                                                       string channelName,
                                                       string channelDescription)
        {
            await EnsureScopes(_teamsDetailsScopes);

            try
            {
                // Initialize the new channel
                var newChannel = new Channel
                {
                    DisplayName = channelName,
                    Description = channelDescription
                };

                // POST /teams/teamId/channels
                await _graphClient.Teams[teamId]
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
        [AuthorizeForScopes(Scopes = new[] {
            GraphConstants.ChannelCreate,
            GraphConstants.ChannelSettingsReadWriteAll,
            GraphConstants.TeamsAppInstallationReadWriteForTeam,
            GraphConstants.TeamSettingsReadWriteAll })]
        public async Task<IActionResult> UpdateSettings(Team updatedTeam)
        {
            await EnsureScopes(_teamsDetailsScopes);

            try
            {
                // PATCH /teams/teamId
                await _graphClient.Teams[updatedTeam.Id]
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
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.ChannelMessageSend })]
        public async Task<IActionResult> PostMessageToChannel(string channelId, string teamId)
        {
            await EnsureScopes(_teamsChannelMessageSendScopes);

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
        [AuthorizeForScopes(Scopes = new[] { GraphConstants.ChannelMessageSend })]
        public async Task<IActionResult> PostMessageToChannel(TeamMessageDisplayModel model)
        {
            await EnsureScopes(_teamsChannelMessageSendScopes);

            try
            {
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
                await _graphClient.Teams[model.TeamId]
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
        [AuthorizeForScopes(Scopes = new[] {
            GraphConstants.ChannelCreate,
            GraphConstants.ChannelSettingsReadWriteAll,
            GraphConstants.TeamsAppInstallationReadWriteForTeam,
            GraphConstants.TeamSettingsReadWriteAll })]
        public async Task<IActionResult> DeleteApp(string appId,
                                                   string teamId)
        {
            await EnsureScopes(_teamsDetailsScopes);

            try
            {
                // DELETE /teams/teamId/installedApps/appId
                await _graphClient.Teams[teamId]
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

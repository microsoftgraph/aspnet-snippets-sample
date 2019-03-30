/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Resources;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{
    public class GroupsService
    {
        private GraphServiceClient graphClient;
        private IList<Option> requestOptions;

        public GroupsService(GraphServiceClient graphServiceClient)
        {
            graphClient = graphServiceClient;
            requestOptions = new List<Option>
            {
                new HeaderOption("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"")
            };
        }

        // Get groups. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetGroups()
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get groups.
            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .GetAsync();

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
            return items;
        }

        // Get Office 365 (unified) groups. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetUnifiedGroups()
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get unified groups.
            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .Filter("groupTypes/any(a:a%20eq%20'unified')").GetAsync();

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
            return items;
        }

        // Get the groups that the current user is a direct member of.
        // This snippet requires an admin work account.
        public async Task<List<ResultsItem>> GetMyMemberOfGroups()
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get groups the current user is a direct member of.
            IUserMemberOfCollectionWithReferencesPage memberOfGroups = await graphClient.Me.MemberOf.Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .GetAsync();

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
            return items;
        }

        // Create a new group. It can be an Office 365 group, dynamic group, or security group.
        // This snippet creates an Office 365 (unified) group.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> CreateGroup()
        {
            List<ResultsItem> items = new List<ResultsItem>();
            string guid = Guid.NewGuid().ToString();

            // Add the group.
            Group group = await graphClient.Groups.Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .AddAsync(new Group
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
                items.Add(new ResultsItem
                {
                    Display = group.DisplayName,
                    Id = group.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Description, group.Description },
                        { Resource.Prop_Email, group.Mail },
                        { Resource.Prop_Id, group.Id }
                    }
                });
            }
            return items;
        }

        // Get a specified group.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetGroup(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the group.
            Group group = await graphClient.Groups[id].Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .Expand("members").GetAsync();

            if (group != null)
            {

                // Get group properties.
                items.Add(new ResultsItem
                {
                    Display = group.DisplayName,
                    Id = group.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { Resource.Prop_Email, group.Mail },
                        { Resource.Prop_MemberCount, group.Members?.Count },
                        { Resource.Prop_Id, group.Id }
                    }
                });
            }
            return items;
        }

        // Read properties and relationships of group members.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetMembers(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get group members. 
            IGroupMembersCollectionWithReferencesPage members = await graphClient.Groups[id].Members.Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .GetAsync();

            if (members?.Count > 0)
            {
                foreach (User user in members)
                {
                    // Get member properties.
                    items.Add(new ResultsItem
                    {
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

        // Read properties and relationships of group members.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetOwners(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get group owners.
            IGroupOwnersCollectionWithReferencesPage members = await graphClient.Groups[id].Owners.Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .GetAsync();

            if (members?.Count > 0)
            {
                foreach (User user in members)
                {

                    // Get owner properties.
                    items.Add(new ResultsItem
                    {
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

        // Update a group.
        // This snippet changes the group name. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> UpdateGroup(string id, string name)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Update the group.
            await graphClient.Groups[id].Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .UpdateAsync(new Group
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

        // Delete a group. Warning: This operation cannot be undone.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> DeleteGroup(string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Delete the group.
            await graphClient.Groups[id].Request(requestOptions)
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .DeleteAsync();

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
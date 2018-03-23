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
using System.Threading.Tasks;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{
    public class EduUsersService
    {

        // Get the current user's profile.
        public async Task<List<ResultsItem>> GetMe(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Get the current user's profile.
            EducationUser me = await graphClient.Education.Me.Request().GetAsync();

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
                        { Resource.Prop_Id, me.Id },
                        { Resource.Primary_Role, me.PrimaryRole.ToString() },
                        { Resource.External_Source, me.ExternalSource.ToString() }
                    }
                });
            }
            return items;
        }

        // Get a specified user.
        public async Task<List<ResultsItem>> GetUser(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            
            // Get the user.
            EducationUser user = await graphClient.Education.Users[id].Request().GetAsync();

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
                        { Resource.Prop_Id, user.Id },
                        { Resource.Primary_Role, user.PrimaryRole.ToString() },
                        { Resource.External_Source, user.ExternalSource.ToString() }

                    }
                });
            }
            return items;
        }

    }
}
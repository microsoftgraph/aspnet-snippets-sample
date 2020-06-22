// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System.Collections.Generic;

namespace SnippetsApp.Models
{
    // The view model for the teams list pages
    public class TeamsListDisplayModel
    {
        // List of all teams
        public IList<Group> AllTeams { get; set; }

        // List of all groups that do not have teams
        public IList<Group> AllNonTeamGroups { get; set; }

        // List of teams user is a member of
        public IList<Team> JoinedTeams { get; set; }

        public TeamsListDisplayModel()
        {
            AllTeams = new List<Group>();
            AllNonTeamGroups = new List<Group>();
        }
    }
}

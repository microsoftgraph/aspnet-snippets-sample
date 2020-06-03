// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System.Collections.Generic;

namespace SnippetsApp.Models
{
    // The view model for the user list pages
    public class GroupsListDisplayModel
    {
        // List of all groups
        public IList<Group> AllGroups { get; set; }

        // List of all unified groups
        public IList<Group> UnifiedGroups { get; set; }

        // List of groups user is a member of
        public IList<Group> GroupMemberships { get; set; }

        // List of groups user owns
        public IList<Group> OwnedGroups { get; set; }
    }
}

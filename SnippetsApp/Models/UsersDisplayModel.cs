// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System.Collections.Generic;

namespace SnippetsApp.Models
{
    // The view model for the /Users page
    public class UsersDisplayModel
    {
        // For listing users, determines if
        // the admin actions should be displayed (Create, Delete)
        public bool UseAdminUI { get; set; }

        // User to display when displaying a single user
        public UserWithPhoto SelectedUser { get; set; }

        // List of users when listing users
        public List<User> UsersList { get; set; }

        // URL to next page of results when listing users
        public string NextPageUrl { get; set; }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System.Collections.Generic;

namespace SnippetsApp.Models
{
    // The view model for the user list pages
    public class UsersListDisplayModel
    {
        // List of users
        public List<User> UsersList { get; set; }

        // URL to next page of results
        public string NextPageUrl { get; set; }
    }
}

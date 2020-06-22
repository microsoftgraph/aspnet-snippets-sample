// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System.Collections.Generic;

namespace SnippetsApp.Models
{
    // The view model for the team display page
    public class TeamDisplayModel
    {
        // The team
        public Team Team { get; set; }

        // List of channels
        public IList<Channel> Channels { get; set; }

        // List of installed apps
        public IList<TeamsAppInstallation> InstalledApps { get; set; }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Graph;

namespace SnippetsApp.Models
{
    // The view model for the Files page
    public class FilesViewDisplayModel
    {
        // List of all child folders in current view
        public IList<DriveItem> Folders { get; set; }

        // List of all child files in current view
        public IList<DriveItem> Files { get; set; }

        // Currently selected folder
        public DriveItem SelectedFolder { get; set; }

        // URL to next page of results
        public string NextPageUrl { get; set; }
    }
}

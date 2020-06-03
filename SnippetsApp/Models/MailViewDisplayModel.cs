// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System.Collections.Generic;

namespace SnippetsApp.Models
{
    // The view model for the mail page
    public class MailViewDisplayModel
    {
        // List of all mail folders
        public IList<MailFolder> MailFolders { get; set; }

        // Currently selected folder
        public MailFolder SelectedFolder { get; set; }

        // Current page of messages
        public IList<Message> Messages { get; set; }

        // URL to next page of results
        public string NextPageUrl { get; set; }
    }
}

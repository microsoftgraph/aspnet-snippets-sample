// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace SnippetsApp.Models
{
    // The view model for post a message to channel page
    public class TeamMessageDisplayModel
    {
        // The channel ID for this message
        public string ChannelId { get; set; }

        // The team ID for this message
        public string TeamId { get; set; }

        // The message content
        public string Message { get; set; }
    }
}

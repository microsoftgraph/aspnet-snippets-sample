// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System;
using System.IO;

namespace SnippetsApp.Models
{
    // Graph Group object plus photo
    public class GroupWithPhoto
    {
        public Group Group { get; set; }
        public string ProfilePhotoUri { get; set; }

        public GroupWithPhoto()
        {
            Group = null;
            // Use default image if the group doesn't have a profile
            // photo
            ProfilePhotoUri = "/img/no-profile-photo-lg.png";
        }

        public void AddGroupPhoto(Stream photoStream)
        {
            if (photoStream != null)
            {
                // Copy the photo stream to a memory stream
                // to get the bytes out of it
                var memoryStream = new MemoryStream();
                photoStream.CopyTo(memoryStream);
                var photoBytes = memoryStream.ToArray();

                // Generate a date URI for the photo
                var photoUri = $"data:image/png;base64,{Convert.ToBase64String(photoBytes)}";
                ProfilePhotoUri = photoUri;
            }
        }
    }
}
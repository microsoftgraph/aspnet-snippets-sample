// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Graph;

namespace SnippetsApp.Models
{
    // Graph User object plus extra info about the user:
    // Photo, manager, direct reports
    public class UserWithPhoto
    {
        public User User { get; set; }
        public string ProfilePhotoUri { get; set; }
        public List<User> Manager { get; set; }
        public List<User> DirectReports { get; set; }

        public UserWithPhoto()
        {
            User = null;
            // Use default image if the user doesn't have a profile
            // photo
            ProfilePhotoUri = "/img/no-profile-photo-lg.png";
        }

        public void AddUserPhoto(Stream photoStream)
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
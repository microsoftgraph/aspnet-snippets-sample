// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Graph;

namespace SnippetsApp.Models
{
    public class RoamingSettings
    {
        // The extension name serves as the ID of the extension
        public static readonly string ExtensionName = "com.contoso.roamingSettings";

        // The values this app uses
        public string Theme { get; set; }
        public string Color { get; set; }
        public string Language { get; set; }

        private RoamingSettings() {}

        public static RoamingSettings Create(string theme, string color, string language)
        {
            return new RoamingSettings
            {
                Theme = theme,
                Color = color,
                Language = language

            };
        }

        public static RoamingSettings FromOpenExtension(Extension openExtension)
        {
            if (openExtension == null)
            {
                return null;
            }

            // Make sure this extension is the expected one
            // By using a fully-qualified name here using your company domain,
            // this should avoid conflicts in extensions with other apps
            if (string.Compare(openExtension.Id, RoamingSettings.ExtensionName, true) != 0)
            {
                throw new ArgumentException(
                    $"The open extension provided is not of the required type. Expected: {RoamingSettings.ExtensionName}, Actual: {openExtension.Id}.");
            }

            return Create(GetValue("theme", openExtension),
                GetValue("color", openExtension), GetValue("language", openExtension));
        }

        public OpenTypeExtension ToOpenExtension()
        {
            return new OpenTypeExtension
            {
                // Extension name is the only required property
                ExtensionName = RoamingSettings.ExtensionName,

                // Since this is an open type, developers must serialize
                // the data they use into a dictionary
                AdditionalData = new Dictionary<string, object>
                {
                    { "theme", Theme },
                    { "color", Color },
                    { "language", Language }
                }
            };
        }

        // Helper function to get values out as strings from the dictionary
        private static string GetValue(string key, Extension openExtension)
        {
            object value;
            var hasValue = openExtension
                .AdditionalData.TryGetValue(key, out value);

            if (hasValue && value != null)
            {
                return value.ToString();
            }

            return null;
        }
    }
}
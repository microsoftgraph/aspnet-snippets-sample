// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SnippetsApp.Models
{
    // Class used to simplify working with <select> inputs on the form
    // See https://docs.microsoft.com/aspnet/core/mvc/views/working-with-forms?view=aspnetcore-3.1#the-select-tag-helper
    public class RoamingSettingsDisplayModel
    {
        public IEnumerable<SelectListItem> Themes
        {
            get { return _themeList; }
        }

        public IEnumerable<SelectListItem> Colors
        {
            get { return _colorList; }
        }

        public IEnumerable<SelectListItem> Languages
        {
            get { return _languageList; }
        }

        public string SelectedTheme
        {
            get
            {
                if (Settings == null) return null;
                return Settings.Theme;
            }
        }

        public string SelectedColor
        {
            get
            {
                if (Settings == null) return null;
                return Settings.Color;
            }
        }

        public string SelectedLanguage
        {
            get
            {
                if (Settings == null) return null;
                return Settings.Language;
            }
        }

        public RoamingSettings Settings { get; }

        public RoamingSettingsDisplayModel(RoamingSettings settings)
        {
            Settings = settings;
        }

        private static IEnumerable<SelectListItem> _themeList =
            new List<SelectListItem>
            {
                new SelectListItem { Text = "Dark", Value = "dark" },
                new SelectListItem { Text = "Light", Value = "light" },
                new SelectListItem { Text = "High Contrast", Value = "highContrast" }
            };

        private static IEnumerable<SelectListItem> _colorList =
            new List<SelectListItem>
            {
                new SelectListItem { Text = "Red", Value = "red" },
                new SelectListItem { Text = "Orange", Value = "orange" },
                new SelectListItem { Text = "Yellow", Value = "yellow" },
                new SelectListItem { Text = "Green", Value = "green" },
                new SelectListItem { Text = "Blue", Value = "blue" },
                new SelectListItem { Text = "Purple", Value = "purple" }
            };

        private static IEnumerable<SelectListItem> _languageList =
            new List<SelectListItem>
            {
                new SelectListItem { Text = "Chinese", Value = "zh-cn" },
                new SelectListItem { Text = "English (US)", Value = "en-us" },
                new SelectListItem { Text = "French", Value = "fr-fr" },
                new SelectListItem { Text = "German", Value = "de-de" },
                new SelectListItem { Text = "Japanese", Value = "ja-jp" },
                new SelectListItem { Text = "Portuguese", Value = "pt-br" },
                new SelectListItem { Text = "Russian", Value = "ru-ru" },
                new SelectListItem { Text = "Spanish", Value = "es-es" }
            };
    }
}
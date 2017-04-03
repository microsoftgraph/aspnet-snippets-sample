/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System.Collections.Generic;
using System.Linq;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{

    // An entity, such as a user, group, or message.
    public class ResultsItem
    {

        // The ID and display name for the entity's radio button.
        public string Id { get; set; }
        public string Display { get; set; }

        // The properties of an entity that display in the UI.
        public Dictionary<string, object> Properties;

        public ResultsItem()
        {
            Properties = new Dictionary<string, object>();
        }
    }

    // View model to display a collection of one or more entities returned from the Microsoft Graph. 
    public class ResultsViewModel
    {

        // Set to false if you don't want to display radio buttons with the results.
        public bool Selectable { get; set; }

        // The list of entities to display.
        public IEnumerable<ResultsItem> Items { get; set; }
        public ResultsViewModel(bool selectable = true)
        {

            // Indicates whether the results should display radio buttons.
            // This is how an entity ID is passed to methods that require it.
            Selectable = selectable;

            Items = Enumerable.Empty<ResultsItem>();
        }
    }
}
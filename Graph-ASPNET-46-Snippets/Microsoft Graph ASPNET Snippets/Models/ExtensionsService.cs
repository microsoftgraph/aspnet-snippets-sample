using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Graph;
using Newtonsoft.Json;
using Resources;
using WebGrease.Css.Extensions;

namespace Microsoft_Graph_ASPNET_Snippets.Models
{
    public class ExtensionsService
    {
        public async Task<List<ResultsItem>> AddOpenExtensionToMe(GraphServiceClient graphClient, string extensionName,
            Dictionary<string, object> data)
        {
            var openExtension = new OpenTypeExtension
            {
                ExtensionName = extensionName,
                AdditionalData = data
            };

            var result = await graphClient.Me.Extensions.Request().AddAsync(openExtension);

            return new List<ResultsItem>() { new ResultsItem()
                {
                    Display = result.Id,
                    Properties = (Dictionary<string,object>)result.AdditionalData
                }
            };
        }

        public async Task<List<ResultsItem>> GetOpenExtensionsForMe(GraphServiceClient graphClient)
        {
            var result = await graphClient.Me.Extensions.Request().GetAsync();

            return result.CurrentPage.Select(r => new ResultsItem()
            {
                Display = r.Id,
                Properties = (Dictionary<string, object>)r.AdditionalData
            }).ToList();
        }

        public async Task<List<ResultsItem>> UpdateOpenExtensionForMe(GraphServiceClient graphClient, string extensionName,
            Dictionary<string, object> data)
        {
            var openExtension = new OpenTypeExtension
            {
                ExtensionName = extensionName,
                AdditionalData = data
            };

            // BUG: Returns null instead of Extension
            var result = await graphClient.Me.Extensions[extensionName].Request().UpdateAsync(openExtension);

            return new List<ResultsItem>() { new ResultsItem()
                {
                    Display = result.Id,
                    Properties = (Dictionary<string,object>)result.AdditionalData
                }
            };
        }

        public async Task DeleteOpenExtensionForMe(GraphServiceClient graphClient, string extensionName)
        {
            await graphClient.Me.Extensions[extensionName].Request().DeleteAsync();
        }
    }
}
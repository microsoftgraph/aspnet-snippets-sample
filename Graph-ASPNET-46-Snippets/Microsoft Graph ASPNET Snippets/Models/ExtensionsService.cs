using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;

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

            var result = await graphClient.Me.Extensions.Request()
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .AddAsync(openExtension);

            return new List<ResultsItem>() { new ResultsItem()
                {
                    Display = result.Id,
                    Properties = (Dictionary<string,object>)result.AdditionalData
                }
            };
        }

        public async Task<List<ResultsItem>> GetOpenExtensionsForMe(GraphServiceClient graphClient)
        {
            var result = await graphClient.Me.Extensions.Request()
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .GetAsync();

            return result.CurrentPage.Select(r => new ResultsItem()
            {
                Display = r.Id,
                Properties = (Dictionary<string, object>)r.AdditionalData
            }).ToList();
        }

        public async Task UpdateOpenExtensionForMe(GraphServiceClient graphClient, string extensionName,
            Dictionary<string, object> data)
        {
            var openExtension = new OpenTypeExtension
            {
                ExtensionName = extensionName,
                AdditionalData = data
            };

            // Note: Client SDK returns Extension, whereas REST API only return 204 with No content
            // Thus result is *always* null (Client SDK is generated, some UpdateAsync calls do return results, this doesn't)
            var result = await graphClient.Me.Extensions[extensionName].Request()
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .UpdateAsync(openExtension);
        }

        public async Task DeleteOpenExtensionForMe(GraphServiceClient graphClient, string extensionName)
        {
            await graphClient.Me.Extensions[extensionName].Request()
                .WithUserAccount(ClaimsPrincipal.Current.ToGraphUserAccount())
                .DeleteAsync();
        }
    }
}
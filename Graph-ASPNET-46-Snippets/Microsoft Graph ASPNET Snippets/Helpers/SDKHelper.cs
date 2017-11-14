/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using Microsoft.Graph;
using System.Net.Http.Headers;
using System;

namespace Microsoft_Graph_ASPNET_Snippets.Helpers
{
    public class SDKHelper
    {

        // Get an authenticated Microsoft Graph Service client.
        public static GraphServiceClient GetAuthenticatedClient()
        {
            GraphServiceClient graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        string accessToken = await SampleAuthProvider.Instance.GetUserAccessTokenAsync();

                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        // This header has been added to identify our sample in the Microsoft Graph service. If extracting this code for your project please remove.
                        requestMessage.Headers.Add("SampleID", "aspnet-snippets-sample");
                    }));
            return graphClient;
        }

    }
}
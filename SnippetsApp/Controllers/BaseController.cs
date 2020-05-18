// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnippetsApp.Controllers
{
    public class BaseController : Controller
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        protected readonly ILogger<HomeController> _logger;

        public BaseController(
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger)
        {
            _tokenAcquisition = tokenAcquisition;
            _logger = logger;
        }

        // Gets a Graph client configured with
        // the specified scopes
        protected GraphServiceClient GetGraphClientForScopes(string[] scopes)
        {
            return GraphServiceClientFactory
                .GetAuthenticatedGraphClient(async () =>
                {
                    return await _tokenAcquisition
                        .GetAccessTokenForUserAsync(scopes);
                }
            );
        }

        // If the Graph client is unable to get a token for the
        // requested scopes, it throws this type of exception.
        protected void InvokeAuthIfNeeded(ServiceException serviceException)
        {
            // Check if this failed because interactive auth is needed
            if (serviceException.InnerException is MsalUiRequiredException)
            {
                // Throwing the exception causes Microsoft.Identity.Web to
                // take over, handling auth (based on scopes defined in the
                // AuthorizeForScopes attribute)
                throw serviceException;
            }
        }

        // Uses a page iterator to get all objects in a collection
        protected async Task<List<T>> GetAllPages<T>(
            GraphServiceClient graphClient,
            ICollectionPage<T> page)
        {
            var allItems = new List<T>();

            var pageIterator = PageIterator<T>.CreatePageIterator(
                graphClient, page,
                (item) => {
                    // This code executes for each item in the
                    // collection
                    allItems.Add(item);
                    return true;
                }
            );

            await pageIterator.IterateAsync();

            return allItems;
        }

        // Uses a page iterator to get all directoryObjects
        // in a collection and cast them to a specific type
        // Will exclude any objects that cannot be case to the
        // requested type
        protected async Task<List<T>> GetAllPagesAsType<T>(
            GraphServiceClient graphClient,
            ICollectionPage<DirectoryObject> page) where T : class
        {
            var allItems = new List<T>();

            var pageIterator = PageIterator<DirectoryObject>.CreatePageIterator(
                graphClient, page,
                (item) => {
                    // This code executes for each item in the
                    // collection
                    if (item is T)
                    {
                        // Only add if the item is the requested type
                        allItems.Add(item as T);
                    }

                    return true;
                }
            );

            await pageIterator.IterateAsync();

            return allItems;
        }
    }
}

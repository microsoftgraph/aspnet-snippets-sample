/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System.Threading;
using System.Web;
using Microsoft.Identity.Client;
using Microsoft.Graph.Auth;
using System.Threading.Tasks;

namespace Microsoft_Graph_ASPNET_Snippets.TokenStorage
{

    // Store the user's token information.
    // Store the user's token information.
    public class SessionTokenCacheProvider: ITokenStorageProvider
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        HttpContextBase httpContext = null;

        public SessionTokenCacheProvider(HttpContextBase httpcontext)
        {
            httpContext = httpcontext;
        }

        public Task SetTokenCacheAsync(string cacheId, byte[] tokenCache)
        {
            SessionLock.EnterWriteLock();
            // Reflect changes in the persistent store
            httpContext.Session[cacheId] = tokenCache;
            SessionLock.ExitWriteLock();

            return Task.FromResult<object>(null);
        }

        public Task<byte[]> GetTokenCacheAsync(string cacheId)
        {
            byte[] tokenCache = null;
            SessionLock.EnterReadLock();
            tokenCache = httpContext.Session[cacheId] as byte[];
            SessionLock.ExitReadLock();

            return Task.FromResult(tokenCache);
        }
    }
}
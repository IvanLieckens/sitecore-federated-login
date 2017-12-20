using System;
using System.Web;

using Microsoft.Identity.Client;

using Sitecore.Diagnostics;

namespace Shared.SC.Feature.Login.Identity
{
    /*
     * This object is a naive representation of the token cache
     */
    [CLSCompliant(false)]
    public class SessionTokenCache : TokenCache
    {
        private static readonly object FileLock = new object();

        private readonly string _cacheId;

        private readonly HttpContextBase _httpContext;

        public SessionTokenCache(string userId, HttpContextBase httpcontext)
        {
            _cacheId = userId + "_TokenCache";
            _httpContext = httpcontext;

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            Load();
        }

        /*
         * Load the cache from the persistent store
         */
        public void Load()
        {
            lock (FileLock)
            {
                try
                {
                    Deserialize((byte[])_httpContext.Session[_cacheId]);
                }
                catch (NullReferenceException ex)
                {
                    Log.Fatal($"Problem looking up the current SessionTokenCache: {ex.Message}", ex, this);
                }
            }
        }

        /* 
         * Write changes to the persistent store
         */
        public void Persist()
        {
            lock (FileLock)
            {
                try
                {
                    // reflect changes in the persistent store
                    _httpContext.Session[_cacheId] = Serialize();

                    // once the write operation took place, restore the HasStateChanged bit to false
                    HasStateChanged = false;
                }
                catch (NullReferenceException ex)
                {
                    Log.Fatal($"Problem setting the file lock: {ex.Message}", ex, this);
                }
            }
        }

        // Clear/empty the cache
        public override void Clear(string clientId)
        {
            base.Clear(clientId);
            lock (FileLock)
            {
                try
                {
                    _httpContext?.Session.Remove(_cacheId);
                }
                catch (NullReferenceException ex)
                {
                    Log.Fatal($"Problem setting the file lock for clear: {ex.Message}", ex, this);
                }
            }
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (HasStateChanged)
            {
                Persist();
            }
        }
    }
}
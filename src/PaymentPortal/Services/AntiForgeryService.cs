using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace PaymentPortal.Services
{
    /// <summary>
    /// Anti-Forgery makes sure No more than x number of requests are processed per token
    /// </summary>
    public class AntiForgeryService
    {
        private readonly MemoryCache cache;
        private readonly int ExpiresInMinutes;
        private readonly int MaxLoginTries;

        public AntiForgeryService()
        {
            cache = MemoryCache.Default;
            ExpiresInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["RetryMinutes"]);
            MaxLoginTries = Convert.ToInt32(ConfigurationManager.AppSettings["MaxLoginTries"]);
        }

        /// <summary>
        /// Generates a Token for a link (with max) if more token are requested, failure is triggered
        /// </summary>
        /// <param name="Link">The link to generate a token for</param>
        /// <returns>The new token</returns>
        public Guid GenerateToken(Guid Link)
        {
            string cacheName = Link.ToString("N");
            List<string> tokens = cache.Get(cacheName) as List<string>;
            if (tokens == null)
            {
                tokens = new List<string>();
            }
            // Stop Login right there
            if (tokens.Count > MaxLoginTries)
            {
                return Guid.Empty;
            }

            /// Create the Token
            var token = Guid.NewGuid();
            tokens.Add(token.ToString("N"));

            // Set the Cache
            cache.Set(new CacheItem(cacheName, tokens), new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromMinutes(ExpiresInMinutes)
            });

            // Return the Token
            return token;
        }

        /// <summary>
        /// Validates a Token for a Link
        /// </summary>
        /// <param name="Link">The Link to Validate</param>
        /// <param name="token">The Token that was sent</param>
        /// <returns></returns>
        public bool ValidateToken(Guid Link, Guid token)
        {
            // Never Validate an Empty
            if (token == Guid.Empty)
                return false;

            // Check the Cache
            string cacheName = Link.ToString("N");
            List<string> tokens = cache.Get(cacheName) as List<string>;
            if (tokens == null)
            {
                return false;
            }
            else
            {
                // Check the Validity and Remove the item if it's valid
                var isValid = tokens.Contains(token.ToString("N"));
                if (isValid)
                {
                    cache.Remove(cacheName);
                    cache.Set(new CacheItem(cacheName, tokens), new CacheItemPolicy
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(ExpiresInMinutes)
                    });
                }
                return isValid;
            }
        }
    }
}
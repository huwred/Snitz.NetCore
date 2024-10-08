﻿using System;
using System.Runtime.Caching;

namespace SnitzCore.Data.Interfaces
{
    public class InMemoryCache : ICacheService
    {
        /// <summary>
        /// Number of minutes before cache expires.
        /// </summary>
        private readonly int _expireIn = 20;
        public bool DoNotExpire { get; set; }
        private readonly CacheItemPolicy? _policy;
        public InMemoryCache()
        {
            TimeSpan expireTime = new TimeSpan(0, _expireIn, 0);
            _policy = new CacheItemPolicy
            {
                SlidingExpiration = expireTime
            };
        }

        /// <summary>
        /// Create new cache object
        /// </summary>
        /// <param name="expires">number of minutes to keep cache</param>
        public InMemoryCache(int expires)
        {
            _policy = null;
            _expireIn = expires;
        }

        public T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class?
        {
            T? item = MemoryCache.Default.Get(cacheKey) as T;
            if (item == null)
            {
                item = getItemCallback();
                if (item == null)
                {
                    return item;
                }
                if (_policy != null)
                {
                    MemoryCache.Default.Add(cacheKey, item!, _policy);
                }
                else
                    MemoryCache.Default.Add(cacheKey, item!, DateTimeOffset.Now.AddMinutes(_expireIn));
            }
            return item;
        }

        public T? Get<T>(string cacheKey) where T : class
        {
            return MemoryCache.Default.Get(cacheKey) as T;

        }
        public void Remove(string cacheKey)
        {
            //var item = MemoryCache.Default.Get(cacheKey);

                MemoryCache.Default.Remove(cacheKey);
        }
        public void RemoveUserCache(string[] keys)
        {

            foreach (var key in keys)
            {
                MemoryCache.Default.Remove(key);
            }
        }
    }
}
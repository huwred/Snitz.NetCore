using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Service.Extensions
{
public static class CacheProvider
{
    private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public static T GetOrCreate<T>(string key, Func<T> createItem, TimeSpan expiration)
    {
        if (!_cache.TryGetValue(key, out T value))
        {
            value = createItem();
                var options = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = expiration
                };
            _cache.Set(key, value, options);
        }
        return value;
    }
}
}

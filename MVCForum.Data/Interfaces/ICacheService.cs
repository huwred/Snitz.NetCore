using System;

namespace SnitzCore.Data.Interfaces;

interface ICacheService
{
    T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class?;
    T? Get<T>(string cacheKey) where T : class;
    void Remove(string cacheKey);
    void RemoveUserCache(string[] keys);
}
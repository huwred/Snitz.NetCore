using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using SnitzCore.Data.Interfaces;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace SnitzCore.Service.MiddleWare
{
    public static class OnlineUsersMiddlewareExtensions
    {
        public static void UseOnlineUsers(this IApplicationBuilder app, string cookieName = "UserGuid", int lastActivityMinutes = 10)
        {
            app.UseMiddleware<OnlineUsersMiddleware>(cookieName, lastActivityMinutes);
        }
    }
    public class OnlineUsersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _cookieName;
        private readonly int _lastActivityMinutes;
        private static readonly ConcurrentDictionary<string, bool> _allKeys = new ConcurrentDictionary<string, bool>();
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public OnlineUsersMiddleware(RequestDelegate next, string cookieName = "UserGuid", int lastActivityMinutes = 10)
        {
            _next = next;
            _cookieName = cookieName;
            _lastActivityMinutes = lastActivityMinutes;
        }

        public Task InvokeAsync(HttpContext context, IMemoryCache memoryCache, ISnitzConfig snitzConfig,IConfiguration config)
        {
            var agent = context.Request.Headers.UserAgent.ToString();
            var arr = config.GetSection("SnitzForums").GetSection("excludeBots").Value?.Split(",").ToArray();
            var extras = snitzConfig.GetValue("STREXCLUDEBOTS");
            if (extras != null) {
                if(extras.Split(",").Any()) {
                    arr = arr?.Union(extras.Split(",")).ToArray();
                }            
            }

            if(arr != null && arr.Any(s => agent.Contains(s, StringComparison.OrdinalIgnoreCase))) {
                return _next(context);
            }
            _logger.Warn(agent);


            if (context.Request.Cookies.TryGetValue(_cookieName, out var userGuid) == false)
            {
                userGuid = Guid.NewGuid().ToString();
                context.Response.Cookies.Append(_cookieName, userGuid, new CookieOptions { HttpOnly = true, MaxAge = TimeSpan.FromDays(30), Secure = true, SameSite = SameSiteMode.Strict });
            }

            memoryCache.GetOrCreate(userGuid, cacheEntry =>
            {
                if (_allKeys.TryAdd(userGuid, true) == false)
                {
                    //if add key failed, setting expiration to the past cprevents caching
                    cacheEntry.AbsoluteExpiration = DateTimeOffset.MinValue;
                }
                else
                {
                    cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(_lastActivityMinutes);
                    cacheEntry.RegisterPostEvictionCallback(callback: RemoveKeyWhenExpired);
                }

                return string.Empty;
            });

            return _next(context);
        }

        protected virtual void RemoveKeyWhenExpired(object key, object value, EvictionReason reason, object state)
        {
            var strKey = (string)key;
            //try to remove key from dictionary
            if (!_allKeys.TryRemove(strKey, out _))
                //if not possible to remove key from dictionary, then try to mark key as not existing in cache
                _allKeys.TryUpdate(strKey, false, true);
        }

        public static int GetOnlineUsersCount()
        {
            return _allKeys?.Count ?? 0;
        }
    }
}

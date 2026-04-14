using KestrelWAF;
using MicroRuleEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MyCSharp.HttpUserAgentParser;
using MyCSharp.HttpUserAgentParser.AspNetCore;
using Newtonsoft.Json;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SnitzCore.Service.MiddleWare
{
    public class VisitorTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MaxMindDb geo;
        private readonly IMemoryCache cache;
        private readonly Func<WebRequest, bool> compiledRule;
        private readonly IHttpUserAgentParserAccessor _parserAccessor;

        public VisitorTrackingMiddleware(RequestDelegate next, MaxMindDb geo, IMemoryCache cache,
        IOptions<Rule> ruleset,IHttpUserAgentParserAccessor parserAccessor)
        {
            _next = next;
            this.geo = geo;
            this.cache = cache;
            _parserAccessor = parserAccessor;
            this.compiledRule = new MRE().CompileRule<WebRequest>(ruleset.Value);
        }

        public async Task InvokeAsync(HttpContext context, ILoggerService databaseLogger,ISnitzConfig snitzConfig,IConfiguration config)
        {
            //reject known BOT traffic
            var agent = context.Request.Headers.UserAgent.ToString();
            var botarr = CacheProvider.GetOrCreate("botlist",()=> botlist(snitzConfig,config),TimeSpan.FromDays(1));
            if (IsBotRequest(context, botarr)) {
                await _next(context);
                return;
            }            
            // Filter out partial view (AJAX) requests
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers["Accept"].ToString().Contains("application/json") ||
                context.Request.RouteValues.Values.Contains("OnlineUsers"))
            {
                await _next(context);
                return;
            }
            try
            {
                var wr = new WebRequest(context.Request, geo, cache);
                HttpUserAgentInformation?  info = _parserAccessor.Get(context);
                if (info != null)
                {
                    var os = info?.Platform?.Name;
                    var browser = info?.Name;
                    var version = info?.Version;
                    databaseLogger.Log(
                    $"{browser}:{version} - {os}",
                    context.Request.GetEncodedPathAndQuery(),
                    context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name.ToLowerInvariant() : $"Anonymous {wr?.IpCountry}",
                    context.Connection.RemoteIpAddress?.ToString());
                }
                else
                {
                    databaseLogger.Log(
                        context.Request.Headers["User-Agent"].ToString(),
                        context.Request.GetEncodedPathAndQuery(),
                        context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name.ToLowerInvariant() : $"Anonymous {wr?.IpCountry}",
                        context.Connection.RemoteIpAddress?.ToString());
                }
            }
            catch (Exception)
            {

            }


            await _next(context);
        }
        private string[] botlist(ISnitzConfig snitzConfig,IConfiguration config)
        {
            var arr = config.GetSection("SnitzForums").GetSection("excludeBots").Value?.Split(",").ToArray();
            var extras = snitzConfig.GetValue("STREXCLUDEBOTS");
            if (!string.IsNullOrWhiteSpace(extras)) {
                if(extras.Split(",").Any()) {
                    arr = arr?.Union(extras.Split(",")).ToArray();
                }            
            }
            return arr ?? [];
        }
        private bool IsBotRequest(HttpContext context, string[] botList)
        {
            var agent = context.Request.Headers.UserAgent.ToString().ToLowerInvariant();
            return botList.Any(bot => agent.Contains(bot.ToLowerInvariant()));
        }

        public async Task<GeoLocation> GetLocationAsync(string ipAddress)
        {
            using var httpClient = new HttpClient();
            string url = $"http://ip-api.com/json/{ipAddress}";
            var response = await httpClient.GetStringAsync(url);
            var location = JsonConvert.DeserializeObject<GeoLocation>(response);
            return location;
        }
    }
    public class GeoLocation
    {
        public string Query { get; set; }  // IP
        public string Country { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
    }
}

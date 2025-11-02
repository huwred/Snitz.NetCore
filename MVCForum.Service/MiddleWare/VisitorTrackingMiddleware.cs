using Microsoft.AspNetCore.Http;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System;
using System.Threading.Tasks;

namespace SnitzCore.Service.MiddleWare
{
    public class VisitorTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitorTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SnitzDbContext db)
        {
            // Filter out partial view (AJAX) requests
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                context.Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                await _next(context);
                return;
            }

            var log = new VisitorLog
            {
                VisitTime = DateTime.UtcNow,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                Path = context.Request.Path
            };

            db.VisitorLog.Add(log);
            await db.SaveChangesAsync();

            await _next(context);
        }
    }
}

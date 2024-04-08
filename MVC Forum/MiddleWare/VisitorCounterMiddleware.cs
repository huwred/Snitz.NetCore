using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace MVCForum.MiddleWare
{
    public class VisitorCounterMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public VisitorCounterMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            string? visitorId = context.Request.Cookies["VisitorId"];
            if (visitorId == null)
            {
                //don the necessary staffs here to save the count by one

                context.Response.Cookies.Append("VisitorId", Guid.NewGuid().ToString(), new CookieOptions()
                {
                    Path = "~/",
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddMinutes(15)
                });
            }

            await _requestDelegate(context);
        }
    }
}

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MVCForum.MiddleWare;

public class RequestSetOptionsMiddleware
{
    private readonly RequestDelegate _next;

    public RequestSetOptionsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Test with https://localhost:5001/Privacy/?option=Hello
    public async Task Invoke(HttpContext httpContext)
    {
        var option = httpContext.Request.Query["upgrade"];

        if (!string.IsNullOrWhiteSpace(option))
        {
            httpContext.Items.Add("upgrade",WebUtility.HtmlEncode(option));
            httpContext.Response.Redirect("~/Admin/Setup/",false);
        }

        await _next(httpContext);
    }
}
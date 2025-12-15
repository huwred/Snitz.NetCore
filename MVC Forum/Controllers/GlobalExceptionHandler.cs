using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(httpContext.Request.Path.Value);
        _logger.LogError(exception.Message, exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Check if the request expects HTML
        var accept = httpContext.Request.Headers["Accept"].ToString();
        if (accept.Contains("text/html"))
        {
            // Redirect to your error page for HTML requests
            //httpContext.Response.Redirect($"/Error/500");
            return false;
        }
        else
        {
            // Fallback to JSON for API or non-HTML requests
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = exception.Message,
                Instance = httpContext.Request.Path,
                Detail = exception.StackTrace
            };

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}


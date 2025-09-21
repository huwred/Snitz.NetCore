using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    ///private readonly ILogger<GlobalExceptionHandler> _logger;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    //public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    //{
    //    _logger = logger;
    //}

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.Error(httpContext.Request.Path.Value);
        _logger.Error(exception.Message, exception);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = exception.Message,
            Instance = httpContext.Request.Path,
            Detail = exception.StackTrace
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}


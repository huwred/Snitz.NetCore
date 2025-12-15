using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace MVCForum.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        
        }

        [Route("Error404")]
        public IActionResult PageNotFound(string url)
        {
            Response.Clear();
            Response.StatusCode = StatusCodes.Status404NotFound;

            return View("_PageNotFound", url ?? string.Empty);
        }

        [Route("Error/{code}")]
        public IActionResult Index(int code)
        {
            IExceptionHandlerPathFeature? feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            Response.Clear();
            Response.StatusCode = code;
            return View("_GenericError");    
        }

        [Route("Error/handle-exception")]
        public IActionResult HandleException()
        {
            // log error
            IExceptionHandlerPathFeature? feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (feature != null)
            {
                _logger.LogError($"Error {feature.Error.Message} occurred at path: {feature.Path}",feature.Error);
                //return StatusCode(StatusCodes.Status500InternalServerError,feature?.Error);
            }
            return View("_GenericError");
        }

    }
}

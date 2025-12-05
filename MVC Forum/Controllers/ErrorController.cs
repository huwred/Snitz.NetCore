using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace MVCForum.Controllers
{
    public class ErrorController : Controller
    {
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

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
                _logger.Error($"Error {feature.Error.Message} occurred at path: {feature.Path}",feature.Error);
                return StatusCode(StatusCodes.Status500InternalServerError,feature?.Error);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }
}

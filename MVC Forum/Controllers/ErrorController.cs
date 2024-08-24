using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MVCForum.Controllers
{
    public class ErrorController : Controller
    {
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        [Route("error/404")]
        public IActionResult PageNotFound()
        {
            Response.Clear();
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("_PageNotFound");
        }

        [Route("error/{code}")]
        public IActionResult Index(int code)
        {
            Response.Clear();
            Response.StatusCode = code;
            return View("_GenericError");    
        }

        [Route("error/handle-exception")]
        public IActionResult HandleException()
        {
            // log error
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (feature != null)
            {
                _logger.Error(feature.Error.Message,feature.Error.InnerException);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using MVCForum.Extensions;
using SnitzCore.Data.Extensions;
using System;

namespace MVCForum.Controllers
{
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/restart")]
    public class RestartController : ControllerBase
    {
        private readonly IShutdownService _shutdownService;

        public RestartController(IShutdownService shutdownService) 
        {
            _shutdownService = shutdownService;
        }


        [HttpPost("restart")]
        public IActionResult Restart()
        {
            SetupCacheProvider.Remove("AdminUser");
            //Console.WriteLine("Restarting application...");
            

        // Fire-and-forget shutdown
        _ = _shutdownService.TriggerShutdownAsync();

            //Thread.Sleep(3000);

            //Response.Redirect("/");
            //Console.WriteLine("Server is doing a triple somersault and will land back on its feet in a moment...");
            return new EmptyResult(); // Response already redirected
        }
    }
}

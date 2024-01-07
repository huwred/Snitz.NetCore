using Microsoft.AspNetCore.Mvc;

namespace SnitzCore.BackOffice
{
    [Route("[controller]")]
    public class Admin : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}
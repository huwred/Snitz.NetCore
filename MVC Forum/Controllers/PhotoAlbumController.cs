using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace MVCForum.Controllers
{
    public class PhotoAlbumController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PhotoAlbumController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetPhoto(int? id)
        {
            return Content("/images/notfound.jpg");
        }

        public IActionResult Thumbnail(int? id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;

            // Load the image file from disk or database
            byte[] imageData = System.IO.File.ReadAllBytes(Path.Combine(webRootPath,"images","notfound.jpg"));

            // Set the content type and return the image data as an ActionResult
            return File(imageData, "image/jpeg");
        }
    }
}

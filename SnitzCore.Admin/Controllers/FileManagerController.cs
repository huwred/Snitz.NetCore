
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System.Net.Mime;
using YG.ASPNetCore.FileManager.CommandsProcessor;
using YG.ASPNetCore.FileManager.Enums;

namespace SnitzCore.BackOffice.Controllers
{
    public class FileManagerController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly SnitzDbContext _dbcontext;
        private readonly IFileManagerCommandsProcessor _processor;

        public FileManagerController(IWebHostEnvironment env, SnitzDbContext dbcontext,IFileManagerCommandsProcessor processor)
        {
            _env = env;
            _dbcontext = dbcontext;
            _processor = processor;
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> SnitzFileApi(string id, Command command, string parameters, IFormFile file)
        {
            return await _processor.ProcessCommandAsync(id, command, parameters, file);
        }

        public IActionResult Index(string path = "")
        {
            return View();
        }

        [Route("Releases/")]
        public IActionResult Releases()
        {
            var rootpath = Path.Combine(_env.WebRootPath, "releases");
            if (!Directory.Exists(rootpath))
            {
                ViewBag.Error = "Uploads directory does not exist";
                return View("Error");
            }
            var dir = new DirectoryInfo(rootpath);
            var vm = new List<FileRelease>();

            foreach (var file in dir.GetFiles())
            {

                var filerec = _dbcontext.Database.SqlQuery<FileCount>($"SELECT [FC_ID],[Title],[FileName],[LinkHits],[LinkOrder],[Posted], [Archived],[Version] FROM [FORUM_FILECOUNT] WHERE [FileName] = {file.Name}").ToList();
                if (filerec != null && filerec.Count == 1)
                {
                    vm.Add(new FileRelease
                    {
                        Id = filerec.First().Id,
                        Title = filerec.First().Title,
                        Filename = filerec.First().FileName,
                        LinkHits = filerec.First().LinkHits,
                        LinkOrder = filerec.First().LinkOrder,
                        Archived = filerec.First().Archived,
                        Posted = filerec.First().Posted,
                        Version = filerec.First().Version,
                        File = file
                    });

                }
            }

            return View(vm);
        }

        public FileResult Download(int id)
        {
            var rootpath = Path.Combine(_env.WebRootPath, "releases");

            var file = _dbcontext.Database.SqlQuery<FileCount>($"SELECT * FROM FORUM_FILECOUNT WHERE FC_ID = {id}").Single();
            //update the counter
            string sql = $"UPDATE FORUM_FILECOUNT SET LinkHits = LinkHits + 1 WHERE FC_ID = {id}";
            _dbcontext.Database.ExecuteSqlRaw(sql);
            string virtualPath = "~/releases/" + file.FileName;

            return File(virtualPath, "application/zip", file.FileName);
        }
        [HttpPost]
        [RequestSizeLimit(52428800*4)]
        public IActionResult UploadRelease(IFormCollection form) 
        {
            if (StringValues.IsNullOrEmpty(form["Title"]) || form.Files.Count == 0)
            {
                return View("Error");
            }
            var rootpath = Path.Combine(_env.WebRootPath, "releases");
            for (int i = 0; i < form.Files.Count; i++)
            {
                IFormFile file = form.Files[i]; //Uploaded file 
                if (file != null)
                {
                    ContentDisposition contentDisposition = new ContentDisposition(file.ContentDisposition);
                    string? filename = contentDisposition.FileName;
                    var filepath = Path.Combine(rootpath,filename);

                    using (FileStream output = System.IO.File.Create(filepath))
                    { file.CopyTo(output); }

                    string title = form["Title"];
                    string version = form["Version"];

                    // Insert data using raw SQL
                    string sql = "INSERT INTO FORUM_FILECOUNT (FileName, Title,Version, Posted, LinkHits, LinkOrder,Archived) VALUES (@p0, @p1,@p2, @p3,@p4, @p5,@p6)";
                    _dbcontext.Database.ExecuteSqlRaw(sql, filename, title,version,DateTime.UtcNow.ToForumDateStr(),0,0,0);

                }
            }

            return RedirectToAction(nameof(Releases));
        }

        public IActionResult EditRelease(int id)
        {
            var filerec = _dbcontext.Database.SqlQuery<FileCount>($"SELECT [FC_ID],[Title],[FileName],[LinkHits],[LinkOrder],[Posted], [Archived],[Version] FROM [FORUM_FILECOUNT] WHERE [FC_ID] = {id}").ToList();

            return PartialView("_EditForm",filerec.FirstOrDefault());
        }
        [HttpPost]
        public IActionResult EditRelease(FileCount model)
        {
            if (ModelState.IsValid)
            {
                string sql = "UPDATE FORUM_FILECOUNT SET Title = @p0, Version = @p1, LinkOrder = @p2, Archived = @p3 WHERE FC_ID = @p4";
                _dbcontext.Database.ExecuteSqlRaw(sql, model.Title, model.Version, model.LinkOrder, model.Archived, model.Id);
                return RedirectToAction(nameof(Releases));
            }
            return View("Error");
        }
    }

    public class FileViewModel
    {
        public FileInfo[] File {get;set; }
        public string Title {get;set;}

    }
    public class FileRelease
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public int LinkHits { get; set; } = 0;
        public int LinkOrder { get; set; } = 0;
        public int Archived { get; set; } = 0;
        public string Posted { get; set; }
        public string Version {get;set;}
        public FileInfo File { get; set; }

    }
}

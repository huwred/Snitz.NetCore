using HGO.ASPNetCore.FileManager.CommandsProcessor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System.IO;
using System.Net.Mime;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

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
public async Task<IActionResult> HgoApi(string id, string command, string parameters, IFormFile file)
{
    return await _processor.ProcessCommandAsync(id, command, parameters, file);
}

        [Breadcrumb("uploads", FromAction = "Home.Index")]
        public IActionResult Index(string path = "")
        {
            var rootpath = Path.Combine(_env.WebRootPath, "uploads");
            if(!Directory.Exists(rootpath))
            {
                ViewBag.Error = "Uploads directory does not exist";
                return View("Error");
            }

            ViewBag.RootPath = rootpath;
            var dir = new DirectoryInfo(Path.Combine(rootpath,path));
            var parentdir = dir.Parent;
            if(parentdir != null && parentdir.FullName.StartsWith(rootpath))
            {
                ViewBag.ParentPath = parentdir.FullName.Substring(rootpath.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            else
            {
                ViewBag.ParentPath = "";
            }
            var childNode1 = new MvcBreadcrumbNode("Index", "FileManager", "uploads");
            var childNode2 = new MvcBreadcrumbNode("Index", "FileManager",  ViewBag.ParentPath)
            {
                RouteValues = new { path = ViewBag.ParentPath},
	            OverwriteTitleOnExactMatch = true,
	            Parent = childNode1
            };
            if(ViewBag.ParentPath == "")
            {
                childNode2 = null;
            }
            var pathtitle = path;
            if(ViewBag.ParentPath != "")
            {
                pathtitle = pathtitle.Replace(ViewBag.ParentPath + "\\","");
            }

            var childNode3 = new RazorPageBreadcrumbNode("/" + path, pathtitle)
            {
    
	            OverwriteTitleOnExactMatch = true,
	            Parent = childNode2 == null ? childNode1 : childNode2
            };
            ViewData["BreadcrumbNode"] = childNode3;

            var vm = new FileViewModel
            {
                RootPath = "uploads",
                CurrentPath = path,
                Files = dir.GetFiles(),
                Folders = dir.GetDirectories()
            };
            return View(vm);
        }

        public IActionResult Upload(IFormCollection form)
        {
            var rootpath = Path.Combine(_env.WebRootPath, "uploads");
            var currPath = form["CurrentPath"];
            
            for (int i = 0; i < form.Files.Count; i++)
            {
                IFormFile file = Request.Form.Files[i]; //Uploaded file 
                if (file != null)
                {
                    ContentDisposition contentDisposition = new ContentDisposition(file.ContentDisposition);
                    string? filename = contentDisposition.FileName;
                    var filepath = Path.Combine(rootpath,currPath,filename);

                    using (FileStream output = System.IO.File.Create(filepath))
                        file.CopyTo(output);
                    if(currPath == "releases")
                    {
                        string title = form["Title"];

                        // Insert data using raw SQL
                        string sql = "INSERT INTO FORUM_FILECOUNT (FileName, Title, Posted, LinkHits, LinkOrder,Archived) VALUES (@p0, @p1,@p2, @p3,@p4, @p5)";
                        _dbcontext.Database.ExecuteSqlRaw(sql, filename, title,DateTime.UtcNow.ToForumDateStr(),0,0,0);

                    } 

                }
            }
            return RedirectToAction("Index",new {path = currPath});
        }

        public IActionResult Download(string file)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = file,
                    Inline = false,
                };
                Response.Headers.Append("Content-Disposition", cd.ToString());
                return File(fileStream, "txt/plain");
            }

        }
    }

    public class FileViewModel
    {
        public string RootPath {get;set; }
        public string CurrentPath {get;set; }
        public FileInfo[] Files {get;set; }
        public DirectoryInfo[] Folders {get;set;}

    }
}

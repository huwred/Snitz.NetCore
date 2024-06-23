using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using SnitzCore.Service.Extensions;
using System.Net.Mime;
using System.Text;

namespace SnitzCore.BackOffice.Controllers;

[Authorize(Roles="Administrator,LanguageEditor")]
public class LanguageManagerController : Controller
{
    private readonly SnitzDbContext _dbcontext;
    private readonly ISnitzConfig _snitzConfig;
    private readonly IWebHostEnvironment _env;
    private readonly LanguageService  _languageResource;
    private readonly ILogger<LanguageManagerController> _logger;
    public LanguageManagerController(SnitzDbContext dbcontext,ISnitzConfig snitzConfig,IWebHostEnvironment hostEnvironment,
        IHtmlLocalizerFactory localizerFactory,ILogger<LanguageManagerController> logger)
    {
        _dbcontext = dbcontext;
        _snitzConfig = snitzConfig;
        _env = hostEnvironment;
        _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
        _logger = logger;
    }

    // GET
    public IActionResult Index()
    {
        TranslationViewModel vm = new TranslationViewModel
        {
            Resources = GetStrings().ToList(),
            ResourceSets = _dbcontext.LanguageResources.Select(l=>l.ResourceSet).Distinct().ToList()
        };
        return View(vm);
    }

    public IActionResult Search(string filter = "", string filterby = "value", string Culture = "en")
    {
        TranslationViewModel vm = new TranslationViewModel
        {
            filter = filter,
            filterby = filterby,
            Resources = new List<LanguageResource>()
        };
        if (filter != "")
        {
            vm.Resources = filterby == "id" ? GetStrings(Culture).Where(s => s.Name.ToLower().Contains(filter.ToLower())).ToList() : GetStrings(Culture).Where(s => s.Value.ToLower().Contains(filter.ToLower())).ToList();
        }
        return View("Search",vm);
    }    
    [HttpPost]
    public IActionResult SearchUpdate(TranslationViewModel vm)
    {
        if (vm.Resources.Any())
        {
            var itemToUpdate = _dbcontext.LanguageResources.Find(vm.Resources[0].Id);
            if (itemToUpdate != null)
            {
                itemToUpdate.Value = vm.Resources[0].Value;
                _dbcontext.LanguageResources.Update(itemToUpdate);
                _dbcontext.SaveChanges();
            }
        }

        if (vm.filter != "")
        {
            vm.Resources = vm.filterby == "id" ? GetStrings().Where(s => s.Name.ToLower().Contains(vm.filter!.ToLower())).ToList() : GetStrings().Where(s => s.Value.ToLower().Contains(vm.filter!.ToLower())).ToList();
        }
        return View("Search",vm);
    }  
    [HttpPost]
    public IActionResult SearchDelete(TranslationViewModel vm)
    {
        if (vm.Resources.Any())
        {
            var itemToUpdate = _dbcontext.LanguageResources.AsNoTracking().FirstOrDefault(l=> l.Id == vm.Resources[0].Id);
            if (itemToUpdate != null)
            {
                _dbcontext.LanguageResources.Where(l=>l.Name == vm.Resources[0].Name).ExecuteDelete();

            }
        }

        if (vm.filter != "")
        {
            vm.Resources = vm.filterby == "id" ? GetStrings().Where(s => s.Name.ToLower().Contains(vm.filter!.ToLower())).ToList() : GetStrings().Where(s => s.Value.ToLower().Contains(vm.filter!.ToLower())).ToList();
        }
        return View("Search",vm);
    } 
    public IActionResult ResourceSet(string id,string culture = "en",string filter = "")
    {
        var model = new LanguageViewModel
        {
            Languages = _dbcontext.LanguageResources.Select(l => l.Culture).Distinct().OrderBy(o=>o).ToList(),
            LanguageStrings = new List<KeyValuePair<string, List<LanguageResource>>>(),
            ResourceSet = id
        };
        _logger.LogWarning($"id:{id} culture:{culture} filter:{filter}");
        if (filter != "")
        {
            model.DefaultStrings =
                model.DefaultStrings!.Where(s => s.Value.ToLower().Contains(filter.ToLower())).ToList();
            foreach (var language in model.Languages)
            {
                model.LanguageStrings.Add(new KeyValuePair<string, List<LanguageResource>>(language,GetStrings(language).Where(l=>l.ResourceSet == id && l.Value.ToLower().Contains(filter.ToLower())).ToList()));
            }
        }
        else
        {
            _logger.LogWarning($"DefaultStrings");
            model.DefaultStrings = GetStrings().Where(l => l.ResourceSet == id).ToList();
            foreach (var language in model.Languages)
            {
                try
                {
                    model.LanguageStrings.Add(new KeyValuePair<string, List<LanguageResource>>(language,GetStrings(language).Where(l=>l.ResourceSet == id).ToList()));

                }
                catch (Exception e)
                {
                    _logger.LogError($"language:{language} {e.Message}");
                    //throw;
                }
            }
        }

        ViewBag.Current = id;
        return PartialView(model);
    }
    private IQueryable<LanguageResource> GetStrings(string culture = "en")
    {
        if (culture.StartsWith("en-"))
        {
            culture = "en";
        }

        var results = _dbcontext.LanguageResources
            .Where(r => r.Culture == culture);
        return results;
    }


    [HttpPost]
    public IActionResult UpdateResource(LanguageResource langres,string? subBut)
    {
        var itemToUpdate = _dbcontext.LanguageResources.SingleOrDefault(l =>
            l.Culture == langres.Culture && l.Name == langres.Name && l.ResourceSet == langres.ResourceSet);
        if (subBut == "update")
        {
            if (itemToUpdate != null)
            {
                itemToUpdate.Value = langres.Value;
                _dbcontext.LanguageResources.Update(itemToUpdate);
            }
            else
            {
                itemToUpdate = new LanguageResource()
                {
                    Culture = langres.Culture,
                    Name = langres.Name,
                    ResourceSet = langres.ResourceSet,
                    Value = langres.Value
                };
                _dbcontext.LanguageResources.Add(itemToUpdate);
            }
            _dbcontext.SaveChanges();
            return Content("<i class='fa fa-check'></i>");
        }
        if (subBut == "delete")
        {
            _dbcontext.LanguageResources.Where(l => l.ResourceSet == langres.ResourceSet && l.Name == langres.Name).ExecuteDelete();

            return Content("<script>location.reload();</script>");
        }
        return Content("<i class='fa fa-times'></i>");
    }

    public IActionResult DeleteResource(string id)
    {
        _dbcontext.LanguageResources.Where(l => l.Name == id).ExecuteDelete();
        return Content("");
    }
    [HttpGet]
    public IActionResult DeleteResourceSet(string id)
    {
        _dbcontext.LanguageResources.Where(l => l.ResourceSet == id).ExecuteDeleteAsync();
        return Content("");
    }
    public IActionResult AddResource(LanguageResource res)
    {
        
        if (ModelState.IsValid)
        {
            _dbcontext.LanguageResources.Add(res);
            _dbcontext.SaveChanges();
            return Content("Resource Saved");
        }

        return Content("Error saving resource");
    }

    [HttpPost]
    public FileResult Export(IFormCollection form)
    {
        string culture = form["culture"];
        string resourceset = form["resource-set"];
        List<LanguageResource> res;

        if (!String.IsNullOrWhiteSpace(resourceset))
        {
            res = _dbcontext.LanguageResources.Where(l=>l.ResourceSet == resourceset).ToList();
            resourceset = "_" + resourceset;
        }
        else
        {
            res = _dbcontext.LanguageResources.Where(l=>l.Culture == culture).ToList();
            resourceset = "";
        }


        var byteArray = Encoding.UTF8.GetBytes(res.ToCSV("path", "", "Id"));
        var stream = new MemoryStream(byteArray);


        var cd = new System.Net.Mime.ContentDisposition
        {
            FileName = "export_" + culture + resourceset + ".csv",
            Inline = false,
        };
        Response.Headers.Add("Content-Disposition", cd.ToString());
        return File(stream, "txt/plain");

    }

    public PartialViewResult Export()
    {
        var resorcesets = _dbcontext.LanguageResources.Select(l=>l.ResourceSet).Distinct();
        return PartialView("popExportCsv", resorcesets.ToList());
    }

    [Authorize(Roles = "Administrator")]
    public PartialViewResult Import()
    {
        ViewBag.Title = _languageResource.GetString("resImport", "ResEditor");
        ViewBag.Hint = "";
        return PartialView("popImportCsv");
    }
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<JsonResult> UploadCSV()
    {
        if (!Directory.Exists(_env.ContentRootPath + "\\App_Data\\"))
        {
            return Json("error|App_Data folder is missing");
        }

        for (int i = 0; i < Request.Form.Files.Count; i++)
        {
            IFormFile file = Request.Form.Files[i]; //Uploaded file                                       
            if (file != null &&
                file.Length > Convert.ToInt32(_snitzConfig.GetIntValue("INTMAXFILESIZE",5))*1024*1024)
            {
                return Json("error|File too large");
            }
            if (file != null)
            {
                ContentDisposition contentDisposition = new ContentDisposition(file.ContentDisposition);
                string filename = contentDisposition.FileName;

                filename = this.EnsureCorrectFilename(filename);

                var extension = Path.GetExtension(filename);
                if (extension != null)
                {
                    string fileExt = extension.ToLower();
                    if (!fileExt.Contains("csv"))
                    {
                        return Json("error|Invalid File type");
                    }
                }
                using (FileStream output = System.IO.File.Create(this.GetPathAndFilename(filename)))
                    await file.CopyToAsync(output);

                _dbcontext.ImportLangResCSV(this.GetPathAndFilename(filename), Convert.ToBoolean(Request.Form["UpdateExisting"]));
                }

        }
        return new JsonResult("error|Problem uploading data");
    }

    private string GetPathAndFilename(string filename)
    {
        return _env.ContentRootPath + "\\App_Data\\" + filename;
    }

    private string EnsureCorrectFilename(string filename)
    {
        if (filename.Contains("\\"))
        filename = filename.Substring(filename.LastIndexOf("\\") + 1);

        return filename;
    }
}
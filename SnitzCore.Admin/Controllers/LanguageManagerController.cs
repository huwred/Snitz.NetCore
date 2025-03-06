using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MVCForum.ViewModels;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using SnitzCore.Service.Extensions;
using System.Net;
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
            Resources = GetStrings("en").ToList(),
            ResourceSets = _dbcontext.LanguageResources.Select(l=>l.ResourceSet).Distinct().OrderBy(o=>o).ToList()
        };
        return View(vm);
    }
    public IActionResult Grid()
    {
        IQueryable<IGrouping<string, LanguageResource>> vm =  _dbcontext.LanguageResources.GroupBy(c => c.Name);
        return View(vm);
    }
    public IActionResult Search(string filter = "", string filterby = "value", string Culture = "")
    {
        TranslationViewModel vm = new TranslationViewModel
        {
            filter = filter,
            filterby = filterby,
            Resources = new List<LanguageResource>(),
            Translations = new List<KeyValuePair<string, List<LanguageResource>>>()
        };
        if (filter != "")
        {
            vm.Resources = filterby == "id" ? GetStrings(Culture).Where(s => s.Name.ToLower().Contains(filter.ToLower())).ToList() : GetStrings(Culture).Where(s => s.Value!.ToLower().Contains(filter.ToLower())).ToList();
            vm.ResourceSets = [.. vm.Resources.Select(l=>l.Name).Distinct().OrderBy(o=>o)];

            foreach (var item in vm.ResourceSets)
            {
                vm.Translations.Add(new KeyValuePair<string, List<LanguageResource>>(item!,_dbcontext.LanguageResources.AsNoTracking().Where(l=>l.Name == item).ToList()));
            }
        }
        return View("Search",vm);
    }    

    public IActionResult Templates(string id)
    {
        DirectoryInfo d = new DirectoryInfo(Path.Combine(_env.WebRootPath, "Templates\\en-GB\\"));
        FileInfo[] Files = d.GetFiles("*.html"); // Get html files


        var templates = Files.Select(f=>f.Name.Replace(".html",""));
        var model = new TemplateViewModel(){TemplateLang = "en-GB",TemplateFile = "approvePost",TemplateHtml = ""};
        model.Templates = templates.ToList();
        return View(model);
    }
    [HttpPost]
    public IActionResult SaveTemplate(TemplateViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (!Directory.Exists(Path.Combine(_env.WebRootPath, $"Templates\\{model.TemplateLang}\\")))
            {
                Directory.CreateDirectory(Path.Combine(_env.WebRootPath, $"Templates\\{model.TemplateLang}\\"));
            }
            var filename = Path.Combine(_env.WebRootPath, $"Templates\\{model.TemplateLang}\\{model.TemplateFile}.html");
            System.IO.File.WriteAllText(filename, model.TemplateHtml);
        }
        DirectoryInfo d = new DirectoryInfo(Path.Combine(_env.WebRootPath, "Templates\\en-GB\\"));
        FileInfo[] Files = d.GetFiles("*.html"); // Get html files

        model.Templates = Files.Select(f=>f.Name.Replace(".html","")).ToList();
        return View("Templates",model);
    }

    public IActionResult ResourceSet(string id,string culture = "en")
    {
        var model = new LanguageViewModel
        {
            Languages = _dbcontext.LanguageResources.Select(l => l.Culture).Distinct().OrderBy(o=>o).ToList(),
            LanguageStrings = new List<KeyValuePair<string, List<LanguageResource>>>(),
            ResourceSet = id
        };

        model.DefaultStrings = GetStrings("en").Where(l => l.ResourceSet == id).ToList();
        foreach (var language in model.Languages)
        {
            try
            {
                model.LanguageStrings.Add(new KeyValuePair<string, List<LanguageResource>>(language,GetStrings(language).Where(l=>l.ResourceSet == id).ToList()));

            }
            catch (Exception e)
            {
                _logger.LogError($"language:{language} {e.Message}");
            }
        }

        ViewBag.Current = id;
        return PartialView(model);
    }
    public IActionResult Resource(string id)
    {
        var model = new LanguageViewModel
        {
            Languages = _dbcontext.LanguageResources.Select(l => l.Culture).Distinct().OrderBy(o=>o).ToList(),
            LanguageStrings = new List<KeyValuePair<string, List<LanguageResource>>>(),
            ResourceSet = id
        };

            model.DefaultStrings = GetStrings("en").Where(l => l.Name == id).ToList();
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

        ViewBag.Current = id;
        return PartialView("Resource",model);
    }
    private IQueryable<LanguageResource> GetStrings(string culture = "")
    {
        if (culture.StartsWith("en-"))
        {
            culture = "en";
        }

        var results = culture == "" ? _dbcontext.LanguageResources
            : _dbcontext.LanguageResources.Where(r => r.Culture == culture);
        return results;
    }
    [HttpPost]
    public IActionResult EditRow(RowDataClass data)
    {

        if (data != null)
        {
            var cultures = data.rowData![1].Split(',');
            LangUpdateViewModel vm = new LangUpdateViewModel
            {
                ResourceId = data.rowData[0],
                ResourceSet = data.rowData[2],
                ResourceTranslations = new Dictionary<string, string>()
            };
            for (int i = 0; i < cultures.Length; i++)
            {
                vm.ResourceTranslations.Add(cultures[i], WebUtility.HtmlDecode(data.rowData[3 + i]));
            }
            vm.rownum = data.id.ToString();
            return PartialView("_EditGrid", vm);
        }
        else
        {
            return Json("An Error Has occoured");
        }
    }
        [HttpPost]
    public IActionResult Update(LangUpdateViewModel data)
    {
        try
        {
            foreach (var item in data.ResourceTranslations!)
            {
                var itemToUpdate = _dbcontext.LanguageResources.SingleOrDefault(l =>
                l.Culture == item.Key && l.Name == data.ResourceSet && l.ResourceSet == data.ResourceId);
                if (itemToUpdate != null)
                {
                    if(itemToUpdate.Value != item.Value)
                    {
                        itemToUpdate.Value = item.Value;
                        _dbcontext.LanguageResources.Update(itemToUpdate);
                    }

                }
                else
                {
                    itemToUpdate = new LanguageResource
                    {
                        Value = item.Value,
                        ResourceSet = data.ResourceId,
                        Name = data.ResourceSet!,
                        Culture = item.Key
                    };
                    _dbcontext.LanguageResources.Add(itemToUpdate);
                }
            }
            _dbcontext.SaveChanges();
            return Json("Changes Saved");
        }
        catch (Exception)
        {
            return Json("An Error Has occoured");
        }
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
        try
        {
            _dbcontext.LanguageResources.Where(l => l.ResourceSet == id).ExecuteDeleteAsync();
        }
        catch (Exception e)
        {
            return Content(e.Message);
        }
        
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
        string culture = form["culture"]!;
        string resourceset = form["resource-set"]!;
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


        var byteArray = Encoding.UTF8.GetBytes(res.ToCSV("path", "", "Id")!);
        var stream = new MemoryStream(byteArray);


        var cd = new System.Net.Mime.ContentDisposition
        {
            FileName = "export_" + culture + resourceset + ".csv",
            Inline = false,
        };
        Response.Headers.Append("Content-Disposition", cd.ToString());
        return File(stream, "txt/plain");

    }

    public PartialViewResult Export()
    {
        var resorcesets = _dbcontext.LanguageResources.Select(l=>l.ResourceSet).Distinct().OrderBy(o=>o);
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
                string? filename = contentDisposition.FileName;

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

    private string EnsureCorrectFilename(string? filename)
    {
        if (filename != null &&  filename.Contains("\\"))
        filename = filename.Substring(filename.LastIndexOf("\\") + 1);

        return filename ?? String.Empty;
    }
}
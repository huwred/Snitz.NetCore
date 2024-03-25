using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.Controllers;

[Authorize(Roles="Administrator,LanguageEditor")]
public class LanguageManagerController : Controller
{
    private readonly SnitzDbContext _dbcontext;

    public LanguageManagerController(SnitzDbContext dbcontext)
    {
        _dbcontext = dbcontext;
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
            model.DefaultStrings = GetStrings().Where(l => l.ResourceSet == id).ToList();
            foreach (var language in model.Languages)
            {
                model.LanguageStrings.Add(new KeyValuePair<string, List<LanguageResource>>(language,GetStrings(language).Where(l=>l.ResourceSet == id).ToList()));
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
}
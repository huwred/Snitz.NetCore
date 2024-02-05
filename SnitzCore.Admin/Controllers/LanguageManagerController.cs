using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.Controllers;

[Authorize(Roles="Admin,LanguageEditor")]
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
            Resources = GetStrings().ToList()
        };
        return View(vm);
    }

    public IActionResult Search(string filter = "", string filterby = "value")
    {
        TranslationViewModel vm = new TranslationViewModel
        {
            filter = filter,
            filterby = filterby,
            Resources = new List<LanguageResource>()
        };
        if (filter != "")
        {
            vm.Resources = filterby == "id" ? GetStrings().Where(s => s.Name.ToLower().Contains(filter.ToLower())).ToList() : GetStrings().Where(s => s.Value.ToLower().Contains(filter.ToLower())).ToList();
        }
        return View("Search",vm);
    }    
    public IActionResult SearchUpdate(TranslationViewModel vm)
    {

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
            LanguageStrings = new List<KeyValuePair<string, List<LanguageResource>>>()
            
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

        var vm = GetStrings(culture).Where(l=>l.ResourceSet == id);
        return PartialView(model);
    }
    private IQueryable<LanguageResource> GetStrings(string culture = "en")
    {
        if (culture.StartsWith("en-"))
        {
            culture = "en";
        }

        var results = _dbcontext!.LanguageResources
            .Where(r => r.Culture == culture);
        return results;
    }


}
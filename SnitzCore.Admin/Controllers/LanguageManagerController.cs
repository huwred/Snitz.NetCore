using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using SnitzCore.Service;

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
        var vm = GetStrings();
        return View(vm);
    }
    public IActionResult ResourceSet(string id,string culture = "en")
    {
        var model = new LanguageViewModel
        {
            Languages = _dbcontext.LanguageResources.Select(l => l.Culture).Distinct().OrderBy(o=>o).ToList(),
            LanguageStrings = new List<KeyValuePair<string, List<LanguageResource>>>(),
            DefaultStrings = GetStrings().Where(l=>l.ResourceSet == id).ToList()
        };
        foreach (var language in model.Languages)
        {
            model.LanguageStrings.Add(new KeyValuePair<string, List<LanguageResource>>(language,GetStrings(language).Where(l=>l.ResourceSet == id).ToList()));
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
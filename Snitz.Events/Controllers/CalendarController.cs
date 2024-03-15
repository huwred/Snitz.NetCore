using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace Snitz.Events.Controllers;

public class CalendarController : Controller
{
    private readonly ISnitzConfig _config;
    private readonly SnitzDbContext _context;
    public CalendarController(ISnitzConfig config,SnitzDbContext dbContext)
    {
        _config = config;
        _context = dbContext;
    }
    // GET
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public IActionResult SaveFeatures(IFormCollection form)
    {
        return PartialView("SaveResult",SaveForm(form));
    }

    private string SaveForm(IFormCollection form)
    {
        try
        {
            _context.Database.BeginTransaction();

            foreach (var formKey in form.Keys.Where(k => !k.StartsWith("_")))
            {
                var val = form[formKey][0];
                var conf = _context.SnitzConfig.FirstOrDefault(f => f.Key == formKey);
                if (conf != null)
                {
                    if (conf.Value != val)
                    {
                        conf.Value = val;
                        _context.SnitzConfig.Update(conf);
                    }
                    _config.RemoveFromCache(formKey);
                }
                else
                {
                    if (val != "0" && val != "")
                    {
                        _context.SnitzConfig.Add(new SnitzConfig() { Id = 0, Key = formKey, Value = val });
                    }
                }
            }
            _context.SaveChanges(true);
        }
        catch (Exception e)
        {
            _context.Database.RollbackTransaction();
            return e.Message;
        }
        finally
        {
            _context.Database.CommitTransaction();
        }

        return "Settings saved successfully";
    }
}
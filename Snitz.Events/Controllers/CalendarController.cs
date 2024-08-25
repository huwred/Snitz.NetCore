using System.Net;
using BbCodeFormatter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Snitz.Events.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;


namespace Snitz.Events.Controllers;

public class CalendarController : Controller
{
    private readonly ISnitzConfig _config;
    private readonly EventContext _context;
    private readonly ICodeProcessor _bbCodeProcessor;
    private readonly SnitzDbContext _snitzContext;
    protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    public CalendarController(ISnitzConfig config,EventContext dbContext,ICodeProcessor BbCodeProcessor,SnitzDbContext snitzContext)
    {
        _config = config;
        _context = dbContext;
        _bbCodeProcessor = BbCodeProcessor;
        _snitzContext = snitzContext;
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
    public JsonResult GetCalendarEvents()
    {
        
        try
        {
            return new EventsRepository(_context,_config,_snitzContext,_bbCodeProcessor).GetCalendarEvents();

        }catch(Exception ex)
        {
            _logger.Error("GetCalendarEvents",ex);
            return Json("");
        }

    }
    public JsonResult GetBirthDays()
    {
        var start = Request.Query["start"];
        var end = Request.Query["end"];
        
        return new EventsRepository(_context,_config,_snitzContext,_bbCodeProcessor).GetBirthdays(User,start, end);
    }



    public JsonResult GetHolidays(string country = "")
    {
        try
        {
            var start = Request.Query["start"];
            var end = Request.Query["end"];
            return new EventsRepository(_context,_config,_snitzContext,_bbCodeProcessor).GetHolidays(start, end,country);

        }
        catch (Exception)
        {
            Response.StatusCode = (int) HttpStatusCode.BadRequest;
            return Json("Problem getting public holidays");
        }

    }

    public JsonResult GetRegions(string id)
    {
        var ctry = id.Split('|')[0];
        try
        {
            var country = new EventsRepository(_context,_config,_snitzContext, _bbCodeProcessor).GetCountries().SingleOrDefault(c => c.countryCode == ctry);
            if (country == null)
            {
                throw new Exception("Invalid country code");
            }
            return Json(country.regions);
        }
        catch (Exception ex)
        {
            Response.StatusCode = (int) HttpStatusCode.BadRequest;
            List<string> errors = new List<string>
            {
                //..some processing
                ex.Message,
                //..some processing
                ex.InnerException.Message
            };
            return Json(errors);
        }
    }


    private string SaveForm(IFormCollection form)
    {
        try
        {
            _snitzContext.Database.BeginTransaction();

            foreach (var formKey in form.Keys.Where(k => !k.StartsWith("_")))
            {
                var val = form[formKey][0];
                var conf = _snitzContext.SnitzConfig.OrderBy(m=>m.Key).FirstOrDefault(f => f.Key == formKey);
                if (conf != null)
                {
                    if (conf.Value != val)
                    {
                        conf.Value = val;
                        _snitzContext.SnitzConfig.Update(conf);
                    }
                    _config.RemoveFromCache(formKey);
                }
                else
                {
                    if (val != "0" && val != "")
                    {
                        _snitzContext.SnitzConfig.Add(new SnitzConfig() { Id = 0, Key = formKey, Value = val });
                    }
                }
            }
            _snitzContext.SaveChanges(true);
        }
        catch (Exception e)
        {
            _snitzContext.Database.RollbackTransaction();
            return e.Message;
        }
        finally
        {
            _snitzContext.Database.CommitTransaction();
        }

        return "Settings saved successfully";
    }
}
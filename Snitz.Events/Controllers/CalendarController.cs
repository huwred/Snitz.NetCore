using System.Net;
using BbCodeFormatter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Snitz.Events.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;


namespace Snitz.Events.Controllers;

public class CalendarController : Controller
{
    private readonly ISnitzConfig _config;
    private readonly EventContext _context;
    private readonly ICodeProcessor _bbCodeProcessor;
    private readonly SnitzDbContext _snitzContext;
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
        IEnumerable<CalendarEventItem> eventDetails = 
            _context.EventItems
                .Include(e=> e.Topic)
                .Include(e => e.Author)
                .Where(e=>e.TopicId != null);

        var urlPath = Url.Action("Index", "Topic");

        var eventList = from item in eventDetails
            let itemStartDate = item.StartDate
            where itemStartDate != null
            select new
            {
                id = item.TopicId,
                title = WebUtility.HtmlDecode(_bbCodeProcessor.Format(item.Title)),
                author = item.Author,
                description = _bbCodeProcessor.Format(item.Description),
                start = itemStartDate.Value.ToString("s"),
                end = item.EndDate?.ToString("s") ?? "",
                allDay = item.IsAllDayEvent,
                editable = false,
                url = urlPath + "/" + item.TopicId
            };
        try
        {
            return Json(eventList.ToArray());

        }catch(Exception ex)
        {
            return Json("");
        }

    }
    public JsonResult GetBirthDays()
    {
        var start = Request.Query["start"];
        var end = Request.Query["end"];
        var today = DateTime.UtcNow;
        List<BirthdayEventItem> eventDetails = new List<BirthdayEventItem>();
        if (_config.GetIntValue("INTCALSHOWBDAYS") == 1 && User.Identity.IsAuthenticated)
        {
            try
            {
                eventDetails = MemberBirthdays(start, end).ToList();
            }
            catch (Exception ex)
            {
                eventDetails = new List<BirthdayEventItem>();
            }
        }

        try
        {
            var eventList = from item in eventDetails
                let itemStartDate = item.StartDate
                where itemStartDate != null
                select new
                {
                    id = item.MemberId,
                    title = " " + item.Title,
                    start = itemStartDate.Value.ToString("s"), //item.StartDate.Value.WithYear(today).ToString("s"),
                    allDay = true,
                    editable = false,
                    url = "/Account/Detail/" + item.Title,
                    className = "event-birthday"
                };
            var returnArray = eventList.ToArray();
            return Json(returnArray);
        }
        catch (Exception ex)
        {
            
            throw new Exception(ex.Message,ex.InnerException);
        }
    }

    public JsonResult GetHolidays(string country = "")
    {
        try
        {
            var start = Request.Query["start"];
            var end = Request.Query["end"];
            List<PublicHoliday> holidays = new List<PublicHoliday>();
            if (_config.GetIntValue("INTCALPUBLICHOLIDAYS") == 1)
            {
                if (country == "")
                    country = _config.GetValue("STRCALCOUNTRY");
                holidays = FetchJsonHolidays(country.Split('|')[0], start, end);
            }

            var eventList = from item in holidays
                select new
                {
                    title = " " + item.Localname,
                    start = new DateTime(item.Date.Year, item.Date.Month, item.Date.Day, 12, 0, 0).ToString("s"),
                    allDay = true,
                    editable = false,
                    url = "",
                    className = "public-holiday"
                };

            return Json(eventList.ToArray());
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
            var country = GetCountries().SingleOrDefault(c => c.CountryCode == ctry);
            if (country == null)
            {
                throw new Exception("Invalid country code");
            }
            return Json(country.Regions);
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


    private List<EnricoCountry> GetCountries()
    {
        var service = new InMemoryCache(600);

        return service.GetOrSet("cal.countries", FetchJsonCountries);
    }

    private IEnumerable<BirthdayEventItem> MemberBirthdays(string start, string end)
    {
        try
        {
            var sdate = DateTime.Parse(start.ToEnglishNumber());
            var edate = DateTime.Parse(end.ToEnglishNumber());

            var s = start.ToEnglishNumber().Replace("-", "").Replace("/", "");
            var e = end.ToEnglishNumber().Replace("-", "").Replace("/", "");

            var thisyear = DateTime.UtcNow.Year;

            if (edate - sdate > new TimeSpan(366, 0, 0, 0)) //don't show on year view
            {
                return new List<BirthdayEventItem>();
            }
            return _snitzContext.Members.Where(m=>m.Status == 1 
                        && !string.IsNullOrWhiteSpace(m.Dob) 
                        && string.Compare(m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()),s) >= 0
                        && string.Compare(m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()),e) <= 0)
                .Select(m => new BirthdayEventItem()
                {
                    MemberId = m.Id,
                    Dob = m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()) + "120000",
                    Title = m.Name
                });

        }
        catch (Exception ex)
        {
            return new List<BirthdayEventItem>();
        }
    }

    private List<PublicHoliday> FetchJsonHolidays(string country, string start, string end)
    {
        string[] cReg = country.Split('|');

        try
        {
            var s = DateTime.Parse(start).ToString("dd-MM-yyyy");
            var e = DateTime.Parse(end).ToString("dd-MM-yyyy");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var json =
                new WebClient().DownloadString(
                    String.Format(
                        "https://kayaposoft.com/enrico/json/v1.0/index.php?action=getPublicHolidaysForDateRange&fromDate={0}&toDate={1}&country={2}&region={3}",
                        s, e, cReg[0], (cReg.Length > 1 ? cReg[1] : "")));
            return JsonConvert.DeserializeObject<List<PublicHoliday>>(json);
        }
        catch (Exception)
        {
            return new List<PublicHoliday>();
        }

    }

    private List<EnricoCountry> FetchJsonCountries()
    {
        try
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Uri myUri = new Uri("https://kayaposoft.com/enrico/json/v1.0?action=getSupportedCountries", UriKind.Absolute);
            WebClient client = new WebClient();

            var json = client.DownloadString(myUri); // new WebClient().DownloadString("https://kayaposoft.com/enrico/json/v1.0?action=getSupportedCountries");
            var countries = JsonConvert.DeserializeObject<List<EnricoCountry>>(json);
            return countries;
        }
        catch (Exception ex)
        {
            EnricoCountry ec = new EnricoCountry();
            ec.CountryCode = "eng";
            ec.Name = "Error: " + ex.InnerException.Message;
            return new List<EnricoCountry>() { ec };

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
                var conf = _snitzContext.SnitzConfig.FirstOrDefault(f => f.Key == formKey);
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
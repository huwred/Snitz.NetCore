using BbCodeFormatter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Nodes;
using Snitz.Events.Models;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Net;


namespace Snitz.Events.Controllers;

public class CalendarController : Controller
{
    private readonly ISnitzConfig _config;
    private readonly EventContext _context;
    private readonly ICodeProcessor _bbCodeProcessor;
    private readonly SnitzDbContext _snitzContext;
    private readonly IMember _memberService;
    private readonly IHttpClientFactory _httpClientFactory;
    protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    public CalendarController(ISnitzConfig config,EventContext dbContext,ICodeProcessor BbCodeProcessor,SnitzDbContext snitzContext,IMember memberservice,IHttpClientFactory factory)
    {
        _config = config;
        _context = dbContext;
        _bbCodeProcessor = BbCodeProcessor;
        _snitzContext = snitzContext;
        _memberService = memberservice;
        _httpClientFactory = factory;
    }
    /// <summary>
    /// Displays the main calendar view with a list of countries and breadcrumb navigation.
    /// </summary>
    /// <remarks>This action retrieves a list of countries from the data repository and sets up the 
    /// breadcrumb navigation for the calendar page. The retrieved countries are passed to  the view via the <see
    /// cref="ViewData"/> dictionary.</remarks>
    /// <returns>An <see cref="IActionResult"/> that renders the calendar view.</returns>
    public IActionResult Index()
    {
        var countries = new EventsRepository(_context,_config,_snitzContext, _bbCodeProcessor, _httpClientFactory).GetCountries().Result;

        ViewData["BreadcrumbNode"] = new MvcBreadcrumbNode("Index", "Calendar", "calTitle");
        ViewData["Countries"] = countries;
        return View(); //"IndexNew"
    }
    /// <summary>
    /// Saves the submitted config data and returns a partial view with the result.
    /// </summary>
    /// <remarks>This method processes the submitted form data and renders the "SaveResult" partial view with
    /// the outcome. Ensure that the form data contains all required fields for successful processing.</remarks>
    /// <param name="form">The form data submitted by the client, represented as an <see cref="IFormCollection"/>.</param>
    /// <returns>A <see cref="PartialViewResult"/> containing the "SaveResult" view and the result of processing the form data.</returns>
    [HttpPost]
    public IActionResult SaveFeatures(IFormCollection form)
    {
        return PartialView("SaveResult",SaveForm(form));
    }
    /// <summary>
    /// Retrieves a collection of calendar events.
    /// </summary>
    /// <remarks>This method returns calendar events as a JSON result. The response is cached for 300 seconds
    /// to improve performance. If an error occurs during the retrieval process, an empty JSON result is
    /// returned.</remarks>
    /// <returns>A <see cref="JsonResult"/> containing the calendar events. If an error occurs, an empty JSON result is returned.</returns>
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public JsonResult GetCalendarEvents()
    {
        
        try
        {
            return new EventsRepository(_context,_config,_snitzContext,_bbCodeProcessor,_httpClientFactory).GetCalendarEvents();

        }catch(Exception ex)
        {
            _logger.Error("GetCalendarEvents",ex);
            return Json("");
        }

    }
    /// <summary>
    /// Retrieves a list of user birthdays within the specified date range.
    /// </summary>
    /// <remarks>The date range is determined by the "start" and "end" query parameters in the HTTP request.
    /// The result is cached for 300 seconds to improve performance.</remarks>
    /// <returns>A <see cref="JsonResult"/> containing the birthdays of users within the specified date range.</returns>
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [Authorize]
    public JsonResult GetBirthDays()
    {
        var start = Request.Query["start"];
        var end = Request.Query["end"];
        
        return new EventsRepository(_context,_config,_snitzContext,_bbCodeProcessor,_httpClientFactory).GetBirthdays(User,start, end);
    }
    /// <summary>
    /// Retrieves a list of upcoming calendar events, limited to the specified count.
    /// </summary>
    /// <remarks>The events are ordered by their start time and filtered to include only those occurring after
    /// the current UTC time. The response is cached for 300 seconds to improve performance.</remarks>
    /// <param name="count">The maximum number of upcoming events to retrieve. Must be a positive integer.</param>
    /// <returns>A <see cref="JsonResult"/> containing an array of event details. Each event includes its ID, title, start and
    /// end times, all-day status, editability, and URL.</returns>
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public JsonResult GetUpcomingEvents(int count)
    {
        string datetime = DateTime.UtcNow.ToForumDateStr(true);

        IEnumerable<CalendarEventItem> eventDetails = _context.EventItems.OrderBy(e=>e.Start).Where(e=> string.Compare(e.Start,datetime) > 0).Take(count);

            var eventList = from item in eventDetails
                select new
                {
                    id = item.TopicId,
                    title = _bbCodeProcessor.Format(item.Title),
                    start = item.StartDate.Value.ToString("s"),
                    end = item.EndDate.HasValue ? item.EndDate.Value.ToString("s") : "",
                    allDay = item.IsAllDayEvent,
                    editable = false,
                    url = "/Topic/" + item.TopicId
                };

            return Json(eventList.ToArray());
    }
    /// <summary>
    /// Retrieves a list of public holidays within the specified date range and country.
    /// </summary>
    /// <remarks>The method uses caching to improve performance, with a cache duration of 300 seconds. The
    /// date range for the holidays is determined by the "start" and "end" query parameters in the HTTP
    /// request.</remarks>
    /// <param name="country">The country for which to retrieve public holidays. If not specified, holidays for all countries are returned.</param>
    /// <returns>A <see cref="JsonResult"/> containing the list of public holidays. If an error occurs, the response status code
    /// is set to 400, and an error message is returned.</returns>
    //[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public JsonResult GetHolidays(string country = "")
    {
        try
        {
            var start = Request.Query["start"];
            var end = Request.Query["end"];
            return new EventsRepository(_context,_config,_snitzContext,_bbCodeProcessor,_httpClientFactory).GetHolidaysAsync(start, end,country).Result;

        }
        catch (Exception)
        {
            Response.StatusCode = (int) HttpStatusCode.BadRequest;
            return Json("Problem getting public holidays");
        }

    }
    /// <summary>
    /// Retrieves the list of regions for a specified country.
    /// </summary>
    /// <remarks>The method expects the <paramref name="id"/> parameter to include a valid country code as the
    /// first segment. If the country code is invalid or an error occurs, the HTTP status code is set to 400 (Bad
    /// Request), and the response includes error details.</remarks>
    /// <param name="id">A string containing the country code, followed by additional data separated by a pipe ('|'). The country code
    /// must be the first segment of the string.</param>
    /// <returns>A <see cref="JsonResult"/> containing the list of regions for the specified country. If the country code is
    /// invalid, the result contains a list of error messages.</returns>
    public JsonResult GetRegions(string id)
    {
        var ctry = id.Split('|')[0];
        try
        {
            var country = new EventsRepository(_context,_config,_snitzContext, _bbCodeProcessor, _httpClientFactory).GetCountries().Result.SingleOrDefault(c => c.countryCode == ctry);
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

    /// <summary>
    /// Saves the configuration settings provided in the form collection to the database.
    /// </summary>
    /// <remarks>This method updates existing configuration settings in the database if the provided values
    /// differ from the current ones. If a key does not exist in the database and its value is not "0" or empty, a new
    /// configuration entry is added. The method uses a database transaction to ensure atomicity, rolling back changes
    /// if an exception occurs.</remarks>
    /// <param name="form">The form collection containing key-value pairs of configuration settings to be saved. Keys starting with an
    /// underscore (_) are ignored.</param>
    /// <returns>A string indicating the result of the operation. Returns "Settings saved successfully" if the operation
    /// completes successfully, or the error message if an exception occurs.</returns>
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
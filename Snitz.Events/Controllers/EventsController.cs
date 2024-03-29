using BbCodeFormatter;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data;
using System.Net;
using Microsoft.AspNetCore.Http;
using Snitz.Events.Models;
using SnitzCore.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Snitz.Events.Controllers;

public class EventsController : Controller
{
    private readonly ISnitzConfig _config;
    private readonly EventContext _context;
    private readonly ICodeProcessor _bbCodeProcessor;
    private readonly IMember _memberService;

    public EventsController(ISnitzConfig config,EventContext dbContext,ICodeProcessor BbCodeProcessor,
        IMember memberService)
    {
        _config = config;
        _context = dbContext;
        _bbCodeProcessor = BbCodeProcessor;
        _memberService = memberService;
    }

    [HttpPost]
    public IActionResult AddEvent(CalendarEventItem model)
    {
        var existingevent = _context.EventItems.Find(model.Id);
        if (existingevent != null)
        {
            existingevent.Title = model.Title;
            existingevent.Description = model.Description;
            existingevent.StartDate = model.StartDate;
            existingevent.EndDate = model.EndDate;
            existingevent.LocId = model.LocId;
            existingevent.CatId = model.CatId;
            existingevent.ClubId = model.ClubId;
            _context.EventItems.Update(existingevent);
        }
        else
        {
            model.AuthorId = _memberService.Current()?.Id;
            _context.EventItems.Add(model);
        }
        _context.SaveChanges();
        return Json(new{url=Url.Action("Index", "Topic", new { id = model.TopicId }),id = model.TopicId});
    }

    [HttpPost]
    public IActionResult SaveForum(IFormCollection form)
    {
        try
        {
            _context.Database.ExecuteSql($"UPDATE FORUM_FORUM SET F_ALLOWEVENTS = {Convert.ToInt32(form["Allowed"])} WHERE FORUM_ID={Convert.ToInt32(form["ForumId"])}");
            return Content("Config updated.");
        }
        catch (Exception e)
        {
            return Content(e.Message);
        }

    }
    [HttpGet]
    public JsonResult GetClubCalendarEvents(string id,string old, int calendar = 0, string start = "", string end = "")
    {
        //2018-09-23
        var eventDetails = new List<CalendarEventItem>();
            if (calendar == 0 || (calendar == 1 && _config.GetIntValue("INTCALSHOWEVENTS") == 1))
            {
                eventDetails = new EventsRepository(_context).GetAllClubEvents(id,old,start.Replace("-", ""),end.Replace("-", "")).ToList();
            }
            if (calendar == 1)
            {
                var eventList = from item in eventDetails
                    select new
                    {
                        id = "_ce" + item.Id,
                        title = WebUtility.HtmlDecode(_bbCodeProcessor.Format(item.Title)),
                        author = item.Author,
                        start = item.StartDate.Value.ToString("s"),
                        end = item.EndDate.HasValue ? item.EndDate.Value.ToString("s") : "",
                        allDay = item.IsAllDayEvent,
                        editable = false,
                        className = "event-club",
                        url = Url.Action("Event",new { id = item.Id})
                    };
                return Json(eventList.ToArray());
            }
            else
            {
                var eventList = from item in eventDetails
                    select new
                    {
                        id = item.Id,
                        title = WebUtility.HtmlDecode(_bbCodeProcessor.Format(item.Title)),
                        author = item.Author,
                        currentuser = _memberService.Current(),
                        description = _bbCodeProcessor.Format(item.Description),
                        start = item.StartDate.Value.ToString("s"),
                        enddate = item.EndDate.HasValue ? item.EndDate.Value.ToString("s") : "",
                        allDay = item.IsAllDayEvent,
                        editable = (item.AuthorId == _memberService.Current().Id) || User.IsInRole("Administrator"),
                        location = item.Loc,
                        club = item.Club.Abbreviation,
                        clublong = item.Club.ShortName,
                        category = item.Cat.Name,
                        posted = item.Posted.FromForumDateStr().ToString("s"),
                        url = ""
                    };
                return Json(eventList.ToArray());                    
            }
    }
}
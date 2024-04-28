using BbCodeFormatter;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data;
using Microsoft.AspNetCore.Http;
using Snitz.Events.Models;
using Microsoft.Extensions.Options;
using SnitzCore.Data.Models;

namespace Snitz.Events.Controllers;

public class EventsController : Controller
{
    private readonly ISnitzConfig _config;
    private readonly EventContext _context;
    private readonly ICodeProcessor _bbCodeProcessor;
    private readonly IMember _memberService;
    private readonly string? _tableprefix;
    private readonly SnitzDbContext _snitzContext;

    public EventsController(ISnitzConfig config,EventContext dbContext,ICodeProcessor BbCodeProcessor,
        IMember memberService,IOptions<SnitzForums> options,SnitzDbContext snitzContext)
    {
        _config = config;
        _context = dbContext;
        _bbCodeProcessor = BbCodeProcessor;
        _memberService = memberService;
        _tableprefix = options.Value.forumTablePrefix;
        _snitzContext = snitzContext;

    }

    [HttpPost]
    public IActionResult AddEvent(CalendarEventItem model)
    {
        new EventsRepository(_context,_config,_snitzContext, _bbCodeProcessor).AddEvent(model,_memberService);

        return Json(new{url=Url.Action("Index", "Topic", new { id = model.TopicId }),id = model.TopicId});
    }

    [HttpPost]
    public IActionResult SaveForum(IFormCollection form)
    {
        try
        {
            new EventsRepository(_context,_config,_snitzContext, _bbCodeProcessor).AllowEvents(Convert.ToInt32(form["ForumId"]),Convert.ToInt32(form["Allowed"]),_tableprefix);
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
        return new EventsRepository(_context,_config,_snitzContext, _bbCodeProcessor).GetClubCalendarEvents(id, old, calendar, start, end);

    }
}
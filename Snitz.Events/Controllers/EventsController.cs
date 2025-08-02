using BbCodeFormatter;
using BbCodeFormatter.Processors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SkiaSharp;
using SmartBreadcrumbs.Nodes;
using Snitz.Events.Models;
using Snitz.Events.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzEvents.ViewModels;
using System.Net;

namespace Snitz.Events.Controllers;

public class EventsController : Controller
{
    private readonly ISnitzConfig _config;
    private readonly EventContext _context;
    private readonly ICodeProcessor _bbCodeProcessor;
    private readonly IMember _memberService;
    private readonly string? _tableprefix;
    private readonly SnitzDbContext _snitzContext;
    private readonly EventsRepository _eventsRepository;
    private readonly Dictionary<int, string> _locations = new();
    private readonly Dictionary<int, string> _clubs = new();
    private readonly Dictionary<int, string> _categories = new();

    public EventsController(ISnitzConfig config,EventContext dbContext,ICodeProcessor BbCodeProcessor,
        IMember memberService,IOptions<SnitzForums> options,SnitzDbContext snitzContext)
    {
        _config = config;
        _context = dbContext;
        _bbCodeProcessor = BbCodeProcessor;
        _memberService = memberService;
        _tableprefix = options.Value.forumTablePrefix;
        _snitzContext = snitzContext;
        _eventsRepository = new EventsRepository(_context, _config, _snitzContext, _bbCodeProcessor);

        _locations = dbContext.Set<ClubCalendarLocation>().ToDictionary(t => t.Id, t => t.Name);
        _clubs = dbContext.Set<ClubCalendarClub>().ToDictionary(t => t.Id, t => t.ShortName);
        _categories = dbContext.Set<ClubCalendarCategory>().ToDictionary(t => t.Id, t => t.Name);

    }
        [HttpGet]
        public ActionResult Index(int id=0, int old=0)
        {
            var BookmarkPage = new MvcBreadcrumbNode("Index", "Events", "mnuEvents");
            ViewData["BreadcrumbNode"] = BookmarkPage;

            var vm = new AgendaViewModel
            {
                Categories = _categories,
                Locations = _locations,
                Clubs = _clubs,
                AllowedRoles = new List<string>()
            };

                if(!String.IsNullOrWhiteSpace(_config.GetValue("STRRESTRICTROLES")))
                    vm.AllowedRoles = _config.GetValue("STRRESTRICTROLES").Split(',').ToList();

                if (id == -1 )
                {
                    ViewBag.StartDate = _eventsRepository.OldestEvent();
                    ViewBag.OldNew = -1;
                }else if (old < 0)
                {
                    ViewBag.OldNew = -1;
                    ViewBag.StartDate = _eventsRepository.OldestEvent();
                }
                else
                {
                    ViewBag.StartDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                    ViewBag.OldNew = 0;
                }

                ViewBag.CatSummary = _eventsRepository.GetCategorySummaryList(ViewBag.StartDate);
            vm.CatSummary = ViewBag.CatSummary;
            ViewBag.CatFilter = id;
            ViewBag.TotalEvents = 0;
            foreach (CatSummary cat in ViewBag.CatSummary)
            {
                ViewBag.TotalEvents += cat.EventCount;
            }
            return View(vm);
        }
        [Authorize]
        public IActionResult AddEditEvent(int id)
        {
                var BookmarkPage = new MvcBreadcrumbNode("Index", "Events", "mnuEvents");
                var addPage = new MvcBreadcrumbNode("AddEditEvent", "Events", "New Event"){ Parent = BookmarkPage };
                ViewData["BreadcrumbNode"] = addPage;

                var vm = new ClubEventViewModel();
                vm.CatSummary = _eventsRepository.GetCategorySummaryList(_eventsRepository.OldestEvent());
                var evnt = _eventsRepository.GetClubEventById(id) ?? new ClubCalendarEventItem();
                vm.Id = evnt.Id;
                vm.Title = evnt.Title;
                vm.Description = evnt.Description;
                vm.CatId = evnt.CatId;
                vm.ClubId = evnt.ClubId;
                vm.LocId = evnt.LocId;
                vm.StartDate = evnt.StartDate;
                vm.EndDate = evnt.EndDate;

                vm.Categories = new Dictionary<int, string>();
                vm.Locations = new Dictionary<int, string>();
                vm.Clubs = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> forum in _context.Set<ClubCalendarCategory>().ToDictionary(t => t.Id, t => t.Name))
                {
                    vm.Categories.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
                }
                foreach (KeyValuePair<int, string> forum in _context.Set<ClubCalendarLocation>().ToDictionary(t => t.Id, t => t.Name))
                {
                    vm.Locations.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
                }
                foreach (KeyValuePair<int, string> forum in _context.Set<ClubCalendarClub>().ToDictionary(t => t.Id, t => t.ShortName))
                {
                    vm.Clubs.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
                }
                
                return View(vm);
        }
        [HttpGet]
        [Authorize]
        public ActionResult EventSubs()
        {
                var BookmarkPage = new MvcBreadcrumbNode("Index", "Events", "mnuEvents");
                var addPage = new MvcBreadcrumbNode("EventSubs", "Events", "Subscriptions"){ Parent = BookmarkPage };
                ViewData["BreadcrumbNode"] = addPage;

            var currentMember = _memberService.Current();
            var vm = new SubscribeModel();
            vm.SubscriptionSources = _eventsRepository.GetClubsList().ToDictionary(t => t.Key, t => t.Value);
            vm.SelectedSources = _eventsRepository.GetSubsList(currentMember.Id);

            return View(vm);
        }
        [HttpPost]
        [Authorize]
        public ActionResult EventSubs(SubscribeModel vm)
        {
            if (ModelState.IsValid)
            {
                var currentMember = _memberService.Current();
                if(currentMember == null)
                {
                    return View(vm);
                }
                var currentsubs = _eventsRepository.GetSubsList(currentMember.Id).ToList();

                var removesubs = from item in currentsubs
                            where !vm.SelectedSources.Contains(item)
                            select item;

                foreach (var ii in removesubs)
                {
                    _eventsRepository.SubDelete(ii, currentMember.Id);
                }

                var addsubs = from item in vm.SelectedSources
                                where !currentsubs.Contains(item)
                            select item;

                foreach (var source in addsubs)
                {
                    ClubCalendarSubscriptions sub = new ClubCalendarSubscriptions();
                    sub.ClubId = source;
                    sub.MemberId = currentMember.Id;
                    _context.Set<ClubCalendarSubscriptions>().Add(sub);

                }
                _context.SaveChanges();
                vm.SelectedSources = _eventsRepository.GetSubsList(currentMember.Id);
            }
            return View(vm);

        }
    [HttpPost]
    public IActionResult AddEvent(CalendarEventItem model)
    {
        _eventsRepository.AddEvent(model,_memberService);

        return Json(new{url=Url.Action("Index", "Topic", new { id = model.TopicId }),id = model.TopicId});
    }

    [HttpPost]
    public IActionResult SaveForum(IFormCollection form)
    {
        try
        {
            _eventsRepository.AllowEvents(Convert.ToInt32(form["ForumId"]),Convert.ToInt32(form["Allowed"]),_tableprefix);
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
        var currmembername = _memberService.Current()?.Name;
        return _eventsRepository.GetClubCalendarEvents(id, old, calendar, start, end,currmembername??"");

    }

    #region Admin Actions

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public PartialViewResult Admin()
        {
            var vm = new EventsAdminViewModel
            {
                
                Categories = new Dictionary<int, string>(),
                Locations = new Dictionary<int, string>(),
                Clubs = new Dictionary<int, string>()
            };

            foreach (KeyValuePair<int, string> forum in _context.Set<ClubCalendarCategory>().ToDictionary(t => t.Id, t => t.Name))
            {
                vm.Categories.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
            }
            foreach (KeyValuePair<int, string> forum in _locations)
            {
                vm.Locations.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
            }
            foreach (KeyValuePair<int, string> forum in _context.Set<ClubCalendarClub>().ToDictionary(t => t.Id, t => t.ShortName))
            {
                vm.Clubs.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
            }
            return PartialView("_Admin",vm);
        }

        [HttpGet]
        [Authorize(Roles ="Administrator")]
        public PartialViewResult AddEditCategory(int id)
        {
            EditListViewModel vm;
            if (id == 0)
            {
                vm = new EditListViewModel() {ListType = "cat", Order = 99 };
                return PartialView("_AddEditListItem", vm);
            }
            var cat = _context.Set<ClubCalendarCategory>().FirstOrDefault(c => c.Id == id);
            vm = new EditListViewModel()
            {
                Id = cat.Id,
                Name = cat.Name,
                Order = cat.Order,
                ListType = "cat"
            };
            return PartialView("_AddEditListItem",vm);            
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public PartialViewResult AddEditLocation(int id)
        {
            EditListViewModel vm;
            if (id == 0)
            {
                vm = new EditListViewModel() { ListType = "loc" , Order=99};
                return PartialView("_AddEditListItem", vm);
            }
            var loc = _context.Set<ClubCalendarLocation>().FirstOrDefault(c => c.Id == id);
            vm = new EditListViewModel(){
                Id = loc.Id,
                Name = loc.Name,
                Order = loc.Order,
                ListType = "loc"
            };
            return PartialView("_AddEditListItem",vm);
                
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public PartialViewResult AddEditClub(int id)
        {
            var Locations = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> forum in _locations)
                {
                    Locations.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
                }
            ViewBag.Locations = Locations;
            if (id == 0)
            {
                ClubCalendarClub vm = new ClubCalendarClub() { Order = 99 };
                return PartialView("_AddEditClubItem", vm);
            }else {
                var vm = _context.Set<ClubCalendarClub>().FirstOrDefault(c => c.Id == id);

                return PartialView("_AddEditClubItem", vm);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult AddEditClub(ClubCalendarClub vm)
        {
            if (ModelState.IsValid)
            {
                if (vm.Id == 0)
                {
                    _context.Add(vm);
                }
                else
                {
                    var existingClub = _context.Set<ClubCalendarClub>().FirstOrDefault(c => c.Id == vm.Id);
                    if (existingClub != null)
                    {
                        existingClub.LongName = vm.LongName;
                        existingClub.ShortName = vm.ShortName;
                        existingClub.Abbreviation = vm.Abbreviation;
                        existingClub.Order = vm.Order;
                        existingClub.DefLocId = vm.DefLocId;
                    }
                    _context.Update(existingClub);
                }
                
                _context.SaveChanges();
                return Json(new { success = true, responseText = "Club Added!" });
            }
            var locations = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> forum in _locations)
                {
                    locations.Add(forum.Key, _bbCodeProcessor.Format(forum.Value));
                }            
            ViewBag.Locations = locations;
            return PartialView("_AddEditClubItem", vm);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult AddEditItem(EditListViewModel vm)
        {
            if (ModelState.IsValid)
            {
                    switch (vm.ListType)
                    {
                        case "cat":
                            _eventsRepository.SaveEventCategory(vm);

                            break;
                        case "loc":
                            _eventsRepository.SaveEventLocation(vm);
                            break;

                    }
                return Json(new { success = true, responseText = "Item Added!" });;
            }
            return PartialView("_AddEditListItem", vm);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonResult Delete(int id, string t)
        {
            Response.StatusCode = (int)HttpStatusCode.OK;
            switch (t)
            {
                case "cat":
                    try
                    {
                        _eventsRepository.DeleteCategory(id);
                        return Json(new { success = true, responseText = "Delete success!" });
                    }
                    catch (Exception e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(e.Message);
                    }
                    break;
                case "club":
                    try
                    {
                        _eventsRepository.DeleteClub(id);
                        return Json(new { success = true, responseText = "Delete success!" });
                    }
                    catch (Exception e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(e.Message);
                    }
                    break;
                case "loc":
                    try
                    {
                        _eventsRepository.DeleteLocation(id);
                        return Json(new { success = true, responseText = "Delete success!" });
                    }
                    catch (Exception e)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(e.Message);
                    }
                    break;
            }
            
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Problem removing list item");
        }

    #endregion
}
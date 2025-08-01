﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data.Extensions;
using SnitzCore.Service.Extensions;
using System.Security.Claims;
using SnitzCore.Data.Interfaces;
using System.Text.Json;
using SnitzCore.Data;
using System.Net;
using BbCodeFormatter;
using SkiaSharp;
using SnitzCore.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Snitz.Events.ViewModels;

namespace Snitz.Events.Models
{
    public class EventsRepository : IDisposable
    {
        private readonly EventContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly SnitzDbContext _snitzContext;
        private readonly ICodeProcessor _bbCodeProcessor;
        //private readonly IMember _memberService;

        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public EventsRepository(EventContext dbContext,ISnitzConfig config,SnitzDbContext snitzContext,ICodeProcessor BbCodeProcessor)
        {
            _dbContext = dbContext;
            _config = config;
            _snitzContext = snitzContext;
            _bbCodeProcessor = BbCodeProcessor;
            //_memberService = memberservice;
        }

        public void Dispose()
        {

        }
        public CalendarEventItem? GetById(int id)
        {
            return _dbContext.EventItems.Include(e=>e.Topic).FirstOrDefault(e=>e.Id == id);
        }

        public CalendarEventItem? GetClubEventById(int id)
        {
            return _dbContext.EventItems.Include(e=>e.Author)
                .Include(ce => ce.Cat)
                .Include(ce => ce.Club)
                .Include(ce => ce.Loc)                
                .FirstOrDefault(e=>e.Id == id && e.ClubId != null);
        }
        public IEnumerable<CalendarEventItem> GetAllClubEvents(string catid, string old, string? start, string? end)
        {
            var clubevents = _dbContext.EventItems
                .Include(ce => ce.Author)
                .Include(ce => ce.Cat)
                .Include(ce => ce.Club)
                .Include(ce => ce.Loc)
                .Where(ce => ce.ClubId != null && ce.TopicId < 0)
                .ToList();

            if (start != null)
                start = start.ToEnglishNumber().Replace("T", "").Replace(":", "");
            if (end != null)
                end = end.ToEnglishNumber().Replace("T", "").Replace(":", "");

            if (Convert.ToInt32(catid) > 0)
            {
                clubevents = clubevents.Where(ce => ce.CatId == Convert.ToInt32(catid)).ToList();
            }
            else if (Convert.ToInt32(old) < 0)
            {
                start = null;
                end = DateTime.UtcNow.ToForumDateStr();
            }
            if (start != null && end != null)
            {
                clubevents = clubevents.Where(ce => string.Compare(ce.Start, start) >= 0
                                        && string.Compare(ce.Start, end) <= 0).ToList();
            }
            else if (start != null)
            {
                clubevents = clubevents.Where(ce => string.Compare(ce.Start, start) >= 0).ToList();
            }
            else if (end != null)
            {
                clubevents = clubevents.Where(ce=>string.Compare(ce.Start, end) <= 0).ToList();
            }

            return clubevents.OrderBy(ce => ce.Start).ToList();
        }
        public JsonResult GetBirthdays(ClaimsPrincipal? user, string start, string end)
        {
            List<BirthdayEventItem> eventDetails = new List<BirthdayEventItem>();
            if (_config.GetIntValue("INTCALSHOWBDAYS") == 1 && user.Identity.IsAuthenticated)
            {
                _logger.Info($"GetBirthDays {start}:{end}");
                try
                {
                    eventDetails = MemberBirthdays(start, end).ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error("GetBirthDays", ex);
                    eventDetails = new List<BirthdayEventItem>();
                }
                _logger.Info($"eventDetails {eventDetails.Count}");
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
                _logger.Info($"returnArray {eventList.Count()}");
                var returnArray = eventList.ToArray();

                return new JsonResult(returnArray);
            }
            catch (Exception ex)
            {
                _logger.Error("GetBirthDays", ex);
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
        public JsonResult GetHolidays(string start, string end,string country = "")
        {

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
                    title = " " + item.localName,
                    start = new DateTime(item.date.year, item.date.month, item.date.day, 12, 0, 0).ToString("s"),
                    allDay = true,
                    editable = false,
                    url = "",
                    className = "public-holiday"
                };

            return new JsonResult(eventList.ToArray());
        }
        public JsonResult GetCalendarEvents()
        {
            IEnumerable<CalendarEventItem> eventDetails = 
                _dbContext.EventItems
                    .Include(e=> e.Topic)
                    .Include(e => e.Author)
                    .Where(e=>e.TopicId != null);

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
                    url = "/Topic/Index/" + item.TopicId
                };
            try
            {
                return new JsonResult(eventList.ToArray());

            }catch(Exception ex)
            {
                return new JsonResult("");
            }

        }
        public List<EnricoCountry> GetCountries()
        {
            return CacheProvider.GetOrCreate("cal.countries", FetchJsonCountries,TimeSpan.FromMinutes(600));
        }
    public JsonResult GetClubCalendarEvents(string id,string old, int calendar = 0, string start = "", string end = "", string currmember = "")
    {
        //2018-09-23
        var eventDetails = new List<CalendarEventItem>();
            if (calendar == 0 || (calendar == 1 && _config.GetIntValue("INTCALSHOWEVENTS") == 1))
            {
                eventDetails = GetAllClubEvents(id,old,start.Replace("-", ""),end.Replace("-", "")).ToList();
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
                        url = $"/Event/{item.Id}"
                    };
                return new JsonResult(eventList.ToArray());
            }
            else
            {
                var eventList = from item in eventDetails
                    select new
                    {
                        id = item.Id,
                        title = WebUtility.HtmlDecode(_bbCodeProcessor.Format(item.Title)),
                        author = item.Author,
                        currentuser = currmember,
                        description = _bbCodeProcessor.Format(item.Description),
                        start = item.StartDate.Value.ToString("s"),
                        enddate = item.EndDate.HasValue ? item.EndDate.Value.ToString("s") : "",
                        allDay = item.IsAllDayEvent,
                        //editable = (item.AuthorId == _memberService.Current().Id) || User.IsInRole("Administrator"),
                        location = item.Loc,
                        club = item.Club.Abbreviation,
                        clublong = item.Club.ShortName,
                        category = item.Cat.Name,
                        posted = item.Posted.FromForumDateStr().ToString("s"),
                        url = ""
                    };
                return new JsonResult(eventList.ToArray());                    
            }
    }

    public void AllowEvents(int forumid, int allow,string tableprefix)
    {
        _dbContext.Database.ExecuteSql($"UPDATE {tableprefix}FORUM SET F_ALLOWEVENTS = {allow} WHERE FORUM_ID={forumid}");

    }
    public void AddEvent(CalendarEventItem model, IMember memberService)
    {
        var existingevent = _dbContext.EventItems.Find(model.Id);
        if (existingevent != null)
        {
            existingevent.Title = model.Title;
            existingevent.Description = model.Description;
            existingevent.StartDate = model.StartDate;
            existingevent.EndDate = model.EndDate;
            existingevent.LocId = model.LocId;
            existingevent.CatId = model.CatId;
            existingevent.ClubId = model.ClubId;
            _dbContext.EventItems.Update(existingevent);
        }
        else
        {
            model.AuthorId = memberService.Current()?.Id;
            _dbContext.EventItems.Add(model);
        }
        _dbContext.SaveChanges();
    }
    private List<EnricoCountry> FetchJsonCountries()
    {
        try
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://kayaposoft.com/enrico/json/v1.0?action=getSupportedCountries");
            var response = httpClient.Send(request);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            var responseBody = reader.ReadToEnd();
            var countries = JsonSerializer.Deserialize<List<EnricoCountry>>(responseBody);
            return countries;
        }
        catch (Exception ex)
        {
            EnricoCountry ec = new EnricoCountry();
            ec.countryCode = "eng";
            ec.fullName = "Error: " + ex.InnerException.Message;
            return new List<EnricoCountry>() { ec };

        }

    }
    private IEnumerable<BirthdayEventItem> MemberBirthdays(string start, string end)
    {
        _logger.Info($"MemberBirthdays {start}:{end}");
        try
        {
            var sdate = DateTime.Parse(start.ToEnglishNumber());
            var edate = DateTime.Parse(end.ToEnglishNumber());
            if (edate - sdate > new TimeSpan(365, 0, 0, 0)) //don't show on year view
            {
                _logger.Info($"return yearview");
                return new List<BirthdayEventItem>();
            }

            var s = start.ToEnglishNumber().Replace("-", "").Replace("/", "");
            var e = end.ToEnglishNumber().Replace("-", "").Replace("/", "");

            var thisyear = DateTime.UtcNow.Year;

            _logger.Info($"MemberBirthdays {s}:{e} - {thisyear}");
            var results = _snitzContext.Members.Where(m=>m.Status == 1 
                        && !string.IsNullOrWhiteSpace(m.Dob) 
                        && string.Compare(m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()),s) >= 0
                        && string.Compare(m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()),e) <= 0);
            _logger.Info("query: ");
            _logger.Info(results.ToQueryString());
            return results.Select(m => new BirthdayEventItem()
                {
                    MemberId = m.Id,
                    Dob = m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()) + "120000",
                    Title = m.Name
                });

        }
        catch (Exception ex)
        {
            _logger.Error("MemberBirthdays",ex);
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

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,string.Format(
                "https://kayaposoft.com/enrico/json/v1.0/index.php?action=getPublicHolidaysForDateRange&fromDate={0}&toDate={1}&country={2}&region={3}",
                s, e, cReg[0], (cReg.Length > 1 ? cReg[1] : "")));
            var response = httpClient.Send(request);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            var responseBody = reader.ReadToEnd();

            var result =  JsonSerializer.Deserialize<PublicHoliday[]>(responseBody);
            return result.ToList();

        }
        catch (Exception)
        {
            return new List<PublicHoliday>();
        }

    }

        internal void SaveEventCategory(EditListViewModel vm)
        {
            if(vm.Id == 0)
            {
                ClubCalendarCategory cat = new ClubCalendarCategory()
                {
                    Name = vm.Name,
                    Order = vm.Order
                };
                _dbContext.Set<ClubCalendarCategory>().Add(cat);
            }
            else
            {
                var cat = _dbContext.Set<ClubCalendarCategory>().FirstOrDefault(c => c.Id == vm.Id);
                if (cat != null)
                {
                    cat.Name = vm.Name;
                    cat.Order = vm.Order;
                    _dbContext.Set<ClubCalendarCategory>().Update(cat);
                }
            }
            _dbContext.SaveChanges();
        }

        internal void SaveEventLocation(EditListViewModel vm)
        {
            if(vm.Id == 0)
            {
                ClubCalendarLocation loc = new ClubCalendarLocation()
                {
                    Name = vm.Name,
                    Order = vm.Order
                };
                _dbContext.Set<ClubCalendarLocation>().Add(loc);
            }
            else
            {
                var loc = _dbContext.Set<ClubCalendarLocation>().FirstOrDefault(c => c.Id == vm.Id);
                if (loc != null)
                {
                    loc.Name = vm.Name;
                    loc.Order = vm.Order;
                    _dbContext.Set<ClubCalendarLocation>().Update(loc);
                }
            }
            _dbContext.SaveChanges();
        }

        internal void DeleteCategory(int id)
        {
            _dbContext.Set<ClubCalendarCategory>().Where(c => c.Id == id).ExecuteDelete();
        }

        internal void DeleteClub(int id)
        {
            _dbContext.Set<ClubCalendarClub>().Where(c => c.Id == id).ExecuteDelete();
        }

        internal void DeleteLocation(int id)
        {
            _dbContext.Set<ClubCalendarLocation>().Where(c => c.Id == id).ExecuteDelete();
        }

        public List<CatSummary> GetCategorySummaryList(string startDate)
        {
            var test = from c in _dbContext.Set<CalendarEventItem>()
                        join ce in _dbContext.Set<ClubCalendarCategory>() on c.CatId equals ce.Id into ceGroup
                        where c.CatId != null
                       //where string.Compare(c.Start , startDate.ToEnglishNumber().Replace("-", "")) > 0
                       group ceGroup by new{c.Cat.Name,c.CatId } into g
                       select new CatSummary
                        {
                            CatId = g.Key.CatId.Value,
                            Name = g.Key.Name,
                            EventCount = g.Count()
                        };
            return test.ToList();

            //using (var context = new SnitzDataContext())
            //{
            //    var sql = new Sql();
            //    sql.Select("c.CAT_ID AS CatId ,c.CAT_NAME As Name, COUNT(ce.C_ID) as EventCount");
            //    sql.From("CAL_EVENTS ce");
            //    sql.LeftJoin("EVENT_CAT c").On("c.CAT_ID=ce.CAT_ID");
            //    sql.Where("ce.CAT_ID IS NOT NULL ");
            //    if (startDate != null)
            //    {
            //        sql.Where(" ce.EVENT_DATE >= '" + startDate.Replace("-", "") + "'");
            //    }
            //    sql.GroupBy("c.CAT_NAME,c.CAT_ID");
            //    return _dbContext.Fetch<CatSummary>(sql);
            //}
        }

        public string OldestEvent()
        {
                string top = "TOP 1";
                string limit = "";

                var evnt = _dbContext.EventItems.OrderBy(e=>e.Start).First();
                if (evnt != null)
                {
                    if (evnt.StartDate != null) return evnt.StartDate.Value.AddDays(-1).ToString("yyyy-MM-dd");
                }
            return null;
        }

        internal IEnumerable<int> GetSubsList(int memberid)
        {
            return _dbContext.Set<ClubCalendarSubscriptions>().Where(s=>s.MemberId == memberid).Select(s=>s.Id);

        }

        internal void SubDelete(int subid, int memberid)
        {
            _dbContext.Set<ClubCalendarSubscriptions>().Where(s => s.Id == subid && s.MemberId == memberid).ExecuteDelete();
        }
        public IEnumerable<Pair<int, string>> GetClubsList()
        {
            return _dbContext.Set<ClubCalendarClub>()
                .OrderBy(c => c.Order)
                .Select(c => new Pair<int, string> { Key = c.Id, Value = c.ShortName })
                .ToList();

        }
    }
}

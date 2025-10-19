using BbCodeFormatter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snitz.Events.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service.Extensions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;

namespace Snitz.Events.Models
{
    public class EventsRepository : IDisposable
    {
        private readonly EventContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly SnitzDbContext _snitzContext;
        private readonly ICodeProcessor _bbCodeProcessor;
        private readonly IHttpClientFactory _factory;

        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public EventsRepository(EventContext dbContext,ISnitzConfig config,SnitzDbContext snitzContext,ICodeProcessor BbCodeProcessor,IHttpClientFactory factory)
        {
            _dbContext = dbContext;
            _config = config;
            _snitzContext = snitzContext;
            _bbCodeProcessor = BbCodeProcessor;
            _factory = factory;
            //_memberService = memberservice;
        }

        public void Dispose()
        {

        }
        public bool EnabledForTopic(int topicid)
        {
            try
            {
                return _dbContext.EventItems.SingleOrDefault(e => e.TopicId == topicid) != null;

            }
            catch (Exception)
            {
                return false;
            }

        }

        public int AuthLevel(int forumid)
        {
            try
            {
                var auth = _snitzContext.Database.SqlQuery<int>(
                    $"SELECT F_ALLOWEVENTS AS Value FROM FORUM_FORUM WHERE FORUM_ID={forumid}").Single();
                return auth;
            }
            catch (Exception e)
            {
                return 0;
            }

        }

        public IEnumerable<object> Get(int id)
        {
            try
            {
                return _dbContext.EventItems.AsNoTracking().Where(e => e.TopicId == id).AsEnumerable();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddItemAsync(object item)
        {
            try
            {
                _dbContext.EventItems.Add((CalendarEventItem)item);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public CalendarEventItem? GetById(int id)
        {
            return _dbContext.EventItems.Include(e=>e.Topic).FirstOrDefault(e=>e.Id == id);
        }

        /// <summary>
        /// Retrieves a club-specific calendar event by its unique identifier.
        /// </summary>
        /// <remarks>This method queries the database for a calendar event with the specified identifier
        /// that is associated with a club. The result includes related data such as the event's author, category, club,
        /// and location.</remarks>
        /// <param name="id">The unique identifier of the calendar event to retrieve.</param>
        /// <returns>The <see cref="CalendarEventItem"/> representing the calendar event with the specified identifier, or <see
        /// langword="null"/> if no matching event is found or the event is not associated with a club.</returns>
        public CalendarEventItem? GetClubEventById(int id)
        {
            return _dbContext.EventItems.Include(e=>e.Author)
                .Include(ce => ce.Cat)
                .Include(ce => ce.Club)
                .Include(ce => ce.Loc)                
                .FirstOrDefault(e=>e.Id == id && e.ClubId != null);
        }
        /// <summary>
        /// Retrieves a collection of club events based on the specified category, date range, and other filters.
        /// </summary>
        /// <remarks>This method filters events based on the provided parameters. If both <paramref
        /// name="start"/> and <paramref name="end"/> are specified, only events within the specified range are
        /// included. If only one of these parameters is provided, the filter is applied accordingly.</remarks>
        /// <param name="catid">The category ID to filter events. Must be a valid integer. If greater than 0, only events matching this
        /// category are included.</param>
        /// <param name="old">A flag indicating whether to include old events. If less than 0, the date range is adjusted to include only
        /// past events.</param>
        /// <param name="start">The start date of the range, in a specific string format. Events starting on or after this date are
        /// included. Can be <see langword="null"/> to ignore the start date filter.</param>
        /// <param name="end">The end date of the range, in a specific string format. Events starting on or before this date are included.
        /// Can be <see langword="null"/> to ignore the end date filter.</param>
        /// <returns>An ordered collection of <see cref="CalendarEventItem"/> objects representing the filtered club events. The
        /// collection is sorted by the event start date in ascending order.</returns>
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
        /// <summary>
        /// Retrieves a list of birthday events within the specified date range for the authenticated user.
        /// </summary>
        /// <remarks>This method retrieves birthday events for members within the specified date range if
        /// the feature is enabled in the configuration and the user is authenticated. If an error occurs during the
        /// retrieval process, an empty list of events is returned in the <see cref="JsonResult"/>.</remarks>
        /// <param name="user">The authenticated user whose birthday events are to be retrieved. Must not be null and must have an
        /// authenticated identity.</param>
        /// <param name="start">The start date of the range, in ISO 8601 format (e.g., "yyyy-MM-dd").</param>
        /// <param name="end">The end date of the range, in ISO 8601 format (e.g., "yyyy-MM-dd").</param>
        /// <returns>A <see cref="JsonResult"/> containing an array of birthday event details. Each event includes the member ID,
        /// title, start date, and additional metadata.</returns>
        /// <exception cref="Exception">Thrown if an unexpected error occurs while processing the birthday events.</exception>
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
        /// <summary>
        /// Retrieves a list of public holidays within the specified date range and formats them as JSON.
        /// </summary>
        /// <remarks>This method fetches public holidays based on the application's configuration
        /// settings. If the feature is disabled via configuration, an empty result is returned. The returned JSON is
        /// suitable for use in calendar applications.</remarks>
        /// <param name="start">The start date of the range, in ISO 8601 format (e.g., "yyyy-MM-dd").</param>
        /// <param name="end">The end date of the range, in ISO 8601 format (e.g., "yyyy-MM-dd").</param>
        /// <param name="country">An optional parameter specifying the country for which to retrieve public holidays. If not provided, the
        /// default country configured in the application settings is used.</param>
        /// <returns>A <see cref="JsonResult"/> containing an array of public holidays formatted as event objects. Each event
        /// includes details such as the holiday name, date, and additional metadata.</returns>
        public async Task<JsonResult> GetHolidaysAsync(string start, string end,string country = "")
        {

            List<PublicHoliday> holidays = new List<PublicHoliday>();
            if (_config.GetIntValue("INTCALPUBLICHOLIDAYS") == 1)
            {
                if (country == "")
                    country = _config.GetValue("STRCALCOUNTRY");
                holidays = await FetchJsonHolidays(country.Split('|')[0], start, end);
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
        /// <summary>
        /// Retrieves a list of calendar events from the database and returns them as a JSON result.
        /// </summary>
        /// <remarks>The method queries the database for calendar events that are associated with a topic.
        /// Each event is formatted with its details, including title, author, description, start and end times, and
        /// other metadata. The event data is returned in a JSON-compatible format suitable for use in client-side
        /// applications.</remarks>
        /// <returns>A <see cref="JsonResult"/> containing an array of calendar events. Each event includes properties such as
        /// ID, title, author, description, start and end times, and a URL for accessing the event's topic.</returns>
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
        /// <summary>
        /// Retrieves a list of countries supported by the service.
        /// </summary>
        /// <remarks>The list of countries is cached for 10 minutes to improve performance.  Subsequent
        /// calls within this period will return the cached data.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of  <see
        /// cref="EnricoCountry"/> objects representing the supported countries.</returns>
        public Task<List<EnricoCountry>> GetCountries()
        {
            return CacheProvider.GetOrCreate("cal.countries", FetchJsonCountries,TimeSpan.FromMinutes(10));
        }
        /// <summary>
        /// Retrieves a list of calendar events for a specified club and returns them in JSON format.
        /// </summary>
        /// <remarks>This method formats event data based on the specified parameters and configuration
        /// settings. The returned JSON is suitable for use in client-side applications that display calendar
        /// events.</remarks>
        /// <param name="id">The unique identifier of the club for which events are being retrieved.</param>
        /// <param name="old">A string parameter used to filter events based on specific criteria.</param>
        /// <param name="calendar">An integer indicating the calendar type. A value of 0 retrieves all events, while a value of 1 retrieves
        /// events based on additional configuration settings.</param>
        /// <param name="start">The start date of the event range, in ISO 8601 format (e.g., "yyyy-MM-dd").</param>
        /// <param name="end">The end date of the event range, in ISO 8601 format (e.g., "yyyy-MM-dd").</param>
        /// <param name="currmember">The identifier of the current member, used to include member-specific details in the response.</param>
        /// <returns>A <see cref="JsonResult"/> containing a collection of calendar events. The structure of the returned JSON
        /// depends on the value of the <paramref name="calendar"/> parameter: <list type="bullet"> <item> <description>
        /// If <paramref name="calendar"/> is 1, the JSON includes event details such as ID, title, author, start and
        /// end dates, and a URL for each event. </description> </item> <item> <description> If <paramref
        /// name="calendar"/> is 0, the JSON includes additional details such as the event description, location, club
        /// information, category, and posting date. </description> </item> </list></returns>
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
        /// <summary>
        /// Updates the specified forum to allow or disallow events.
        /// </summary>
        /// <remarks>This method directly executes an SQL update statement to modify the event allowance
        /// for the specified forum. Ensure that the <paramref name="tableprefix"/> parameter is properly sanitized to
        /// prevent SQL injection.</remarks>
        /// <param name="forumid">The unique identifier of the forum to update.</param>
        /// <param name="allow">A value indicating whether events are allowed. Use <c>1</c> to allow events or <c>0</c> to disallow them.</param>
        /// <param name="tableprefix">The prefix of the database table to update. This is used to construct the table name dynamically.</param>
        public void AllowEvents(int forumid, int allow, string tableprefix)
        {
            var sql = $"UPDATE {tableprefix}FORUM SET F_ALLOWEVENTS = {allow} WHERE FORUM_ID={forumid}";
            _dbContext.Database.ExecuteSqlInterpolated(FormattableStringFactory.Create(sql));
        }
        /// <summary>
        /// Adds a new event or updates an existing event in the database.
        /// </summary>
        /// <remarks>If an event with the same <see cref="CalendarEventItem.Id"/> already exists in the
        /// database, the existing event is updated with the details from the provided <paramref name="model"/>.
        /// Otherwise, a new event is added, and the author is set to the current member retrieved from <paramref
        /// name="memberService"/>.</remarks>
        /// <param name="model">The event model containing the details of the event to add or update.</param>
        /// <param name="memberService">The service used to retrieve the current member. This is used to set the author of the event if a new event
        /// is being added.</param>
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
        /// <summary>
        /// Retrieves a list of category summaries, including the category ID, name, and the count of events associated
        /// with each category, filtered by the specified start date.
        /// </summary>
        /// <remarks>This method groups events by their associated category and calculates the total
        /// number of events for each category. Categories without any associated events are excluded from the
        /// result.</remarks>
        /// <param name="startDate">The start date used to filter events. Only events occurring on or after this date are included. The date
        /// should be provided as a string in the format "yyyy-MM-dd".</param>
        /// <returns>A list of <see cref="CatSummary"/> objects, where each object contains the category ID, name, and the count
        /// of events for that category. Returns an empty list if no matching categories are found.</returns>
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

        }
        /// <summary>
        /// Retrieves the date of the oldest event in the database, adjusted to one day prior.
        /// </summary>
        /// <remarks>This method queries the database for the earliest event based on its start date. If
        /// the event's start date is available, the method returns the date as a string in "yyyy-MM-dd" format,
        /// adjusted to one day before the event's start date. If no events are found or the start date is null, an
        /// empty string is returned.</remarks>
        /// <returns>A string representing the date of the oldest event, adjusted to one day prior, in "yyyy-MM-dd" format.
        /// Returns an empty string if no events are found or if the start date is null.</returns>
        public string OldestEvent()
        {
            var evnt = _dbContext.EventItems.OrderBy(e=>e.Start).First();
            if (evnt != null)
            {
                if (evnt.StartDate != null) return evnt.StartDate.Value.AddDays(-1).ToString("yyyy-MM-dd");
            }
            return string.Empty;
        }
        /// <summary>
        /// Retrieves a list of clubs, ordered by their predefined sequence, with each club represented by its ID and
        /// short name.
        /// </summary>
        /// <remarks>The clubs are retrieved from the database and ordered by the "Order" field. The
        /// resulting list is suitable for scenarios such as populating dropdowns or displaying club information in a
        /// user interface.</remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Pair{TKey, TValue}"/> objects, where each pair contains the
        /// club's ID as the key and its short name as the value.</returns>
        public IEnumerable<Pair<int, string>> GetClubsList()
        {
            return _dbContext.Set<ClubCalendarClub>()
                .OrderBy(c => c.Order)
                .Select(c => new Pair<int, string> { Key = c.Id, Value = c.ShortName })
                .ToList();

        }
        /// <summary>
        /// Deletes an event with the specified identifier from the database.
        /// </summary>
        /// <remarks>If the event is associated with a club, all related club calendar subscriptions are
        /// also deleted. No action is taken if the event with the specified identifier does not exist.</remarks>
        /// <param name="id">The unique identifier of the event to delete.</param>
        public void DeleteEvent(int id)
        {
            var evt = _dbContext.EventItems.Find(id);
            if(evt != null)
            {
                if(evt.ClubId != null && evt.ClubId > 0)
                {
                    _dbContext.Set<ClubCalendarSubscriptions>().Where(e=>e.ClubId == evt.ClubId).ExecuteDelete();
                }
                _dbContext.Remove(evt);
                _dbContext.SaveChanges();
            }

        }

        /// <summary>
        /// Retrieves a list of supported countries from the Enrico API in JSON format.
        /// </summary>
        /// <remarks>This method sends an HTTP GET request to the Enrico API to fetch the list of
        /// supported countries. If the request fails or an error occurs during processing, a default list containing a
        /// single country ("England") is returned. The method uses asynchronous operations to perform the HTTP request
        /// and JSON deserialization.</remarks>
        /// <returns>A <see cref="List{T}"/> of <see cref="EnricoCountry"/> objects representing the supported countries. If the
        /// API request fails, the list will contain a single default country with the code "eng" and the name
        /// "England".</returns>
    private async Task<List<EnricoCountry>> FetchJsonCountries()
    {
        EnricoCountry ec = new EnricoCountry
        {
            countryCode = "eng",
            fullName = "England"
        };
        try
        {
            using var httpClient = _factory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://kayaposoft.com/enrico/json/v1.0?action=getSupportedCountries");
            var response = await httpClient.SendAsync(request);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var responseBody = reader.ReadToEnd();
            var countries = JsonSerializer.Deserialize<List<EnricoCountry>>(responseBody);
            return countries ?? [ec];
        }
        catch (Exception ex)
        {
                _logger.Error("FetchJsonCountries", ex);
                return [ec];
        }

    }
        /// <summary>
        /// Retrieves a collection of members whose birthdays fall within the specified date range.
        /// </summary>
        /// <remarks>The method filters members based on their status and ensures that only those with
        /// valid date-of-birth values are included. The year in the date-of-birth is replaced with the current year to
        /// match the specified range.</remarks>
        /// <param name="start">The start date of the range, in a string format that can be parsed into a valid date.</param>
        /// <param name="end">The end date of the range, in a string format that can be parsed into a valid date.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BirthdayEventItem"/> representing the members with birthdays
        /// within the specified date range. Returns an empty collection if the date range exceeds one year or if an
        /// error occurs.</returns>
    private IEnumerable<BirthdayEventItem> MemberBirthdays(string start, string end)
    {
        try
        {
            var sdate = DateTime.Parse(start.ToEnglishNumber());
            var edate = DateTime.Parse(end.ToEnglishNumber());
            if (edate - sdate > new TimeSpan(365, 0, 0, 0)) //don't show on year view
            {
                return [];
            }

            var s = start.ToEnglishNumber().Replace("-", "").Replace("/", "");
            var e = end.ToEnglishNumber().Replace("-", "").Replace("/", "");
            var thisyear = DateTime.UtcNow.Year;

            var results = _snitzContext.Members.Where(m=>m.Status == 1 
                        && !string.IsNullOrWhiteSpace(m.Dob) 
                        && string.Compare(m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()),s) >= 0
                        && string.Compare(m.Dob.Replace(m.Dob.Substring(0,4),thisyear.ToString()),e) <= 0).ToList();

            return results.Select(m => new BirthdayEventItem()
                {
                    MemberId = m.Id,
                    Dob = m.Dob!.Replace(m.Dob.Substring(0,4),thisyear.ToString()) + "120000",
                    Title = m.Name
                });

        }
        catch (Exception ex)
        {
            _logger.Error("MemberBirthdays",ex);
            return [];
        }
    }
        /// <summary>
        /// Retrieves a list of public holidays for a specified country and date range.
        /// </summary>
        /// <remarks>This method sends an HTTP GET request to an external API to fetch public holiday
        /// data. The API requires the country code, optional region code, and a date range as parameters. The method
        /// ensures that the HTTP response is successful and deserializes the JSON response into a list of <see
        /// cref="PublicHoliday"/> objects.</remarks>
        /// <param name="country">The country code and optional region code, separated by a pipe character ('|') if a region is specified. For
        /// example, "US" for the United States or "US|CA" for California.</param>
        /// <param name="start">The start date of the range, in a format that can be parsed into a <see cref="DateTime"/>.</param>
        /// <param name="end">The end date of the range, in a format that can be parsed into a <see cref="DateTime"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="PublicHoliday"/> objects representing the public holidays within the specified date range. If no
        /// holidays are found or an error occurs, an empty list is returned.</returns>
    private async Task<List<PublicHoliday>> FetchJsonHolidays(string country, string start, string end)
    {
        string[] cReg = country.Split('|');

        try
        {
            var s = DateTime.Parse(start).ToString("dd-MM-yyyy");
            var e = DateTime.Parse(end).ToString("dd-MM-yyyy");

            using var httpClient = _factory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get,string.Format(
                "https://kayaposoft.com/enrico/json/v1.0/index.php?action=getPublicHolidaysForDateRange&fromDate={0}&toDate={1}&country={2}&region={3}",
                s, e, cReg[0], (cReg.Length > 1 ? cReg[1] : "")));

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var responseBody = reader.ReadToEnd();

            var result =  JsonSerializer.Deserialize<PublicHoliday[]>(responseBody);
            return result != null ? [.. result] : [];

        }
        catch (Exception)
        {
            return [];
        }

    }
        /// <summary>
        /// Saves the event category based on the provided view model. Adds a new category if the ID is 0, or updates an
        /// existing category if the ID matches an existing record.
        /// </summary>
        /// <remarks>This method interacts with the database context to persist changes. Ensure that the
        /// provided view model contains valid data.</remarks>
        /// <param name="vm">The view model containing the event category details, including the ID, name, and order. The <paramref
        /// name="vm.Id"/> must be 0 to add a new category, or a valid existing ID to update an existing category.</param>
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
        /// <summary>
        /// Saves the event location details to the database.
        /// </summary>
        /// <remarks>If the <paramref name="vm"/> parameter has an <see cref="EditListViewModel.Id"/>
        /// value of 0, a new event location is created and added to the database. If the <paramref name="vm"/>
        /// parameter specifies an existing location ID, the corresponding location is updated with the provided
        /// details. Changes are persisted to the database.</remarks>
        /// <param name="vm">The view model containing the event location details. The <paramref name="vm"/> parameter must include the
        /// location's name and order, and optionally an identifier to update an existing location.</param>
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
        /// <summary>
        /// Deletes the category with the specified identifier from the database.
        /// </summary>
        /// <remarks>This method removes the category with the given <paramref name="id"/> from the
        /// database. If no category with the specified identifier exists, no action is performed.</remarks>
        /// <param name="id">The unique identifier of the category to delete.</param>
        internal void DeleteCategory(int id)
        {
            _dbContext.Set<ClubCalendarCategory>().Where(c => c.Id == id).ExecuteDelete();
        }
        /// <summary>
        /// Deletes the club with the specified identifier from the database.
        /// </summary>
        /// <remarks>This method removes the club with the given <paramref name="id"/> from the database. 
        /// Ensure that the identifier corresponds to an existing club; otherwise, no action will be
        /// performed.</remarks>
        /// <param name="id">The unique identifier of the club to delete.</param>
        internal void DeleteClub(int id)
        {
            _dbContext.Set<ClubCalendarClub>().Where(c => c.Id == id).ExecuteDelete();
        }
        /// <summary>
        /// Deletes the location with the specified identifier from the database.
        /// </summary>
        /// <remarks>This method removes the location with the given <paramref name="id"/> from the
        /// database. If no location with the specified identifier exists, no action is performed.</remarks>
        /// <param name="id">The unique identifier of the location to delete.</param>
        internal void DeleteLocation(int id)
        {
            _dbContext.Set<ClubCalendarLocation>().Where(c => c.Id == id).ExecuteDelete();
        }

        /// <summary>
        /// Retrieves a list of subscription IDs associated with the specified member.
        /// </summary>
        /// <param name="memberid">The unique identifier of the member whose subscriptions are to be retrieved.</param>
        /// <returns>An enumerable collection of subscription IDs for the specified member. The collection will be empty if the
        /// member has no subscriptions.</returns>
        internal IEnumerable<int> GetSubsList(int memberid)
        {
            return _dbContext.Set<ClubCalendarSubscriptions>().Where(s=>s.MemberId == memberid).Select(s=>s.Id);

        }
        /// <summary>
        /// Deletes a subscription record for the specified subscription ID and member ID.
        /// </summary>
        /// <remarks>This method removes the subscription record from the database where the specified
        /// subscription ID  and member ID match. Ensure that the provided IDs correspond to an existing
        /// record.</remarks>
        /// <param name="subid">The unique identifier of the subscription to delete.</param>
        /// <param name="memberid">The unique identifier of the member associated with the subscription.</param>
        internal void SubDelete(int subid, int memberid)
        {
            _dbContext.Set<ClubCalendarSubscriptions>().Where(s => s.Id == subid && s.MemberId == memberid).ExecuteDelete();
        }
    }
}

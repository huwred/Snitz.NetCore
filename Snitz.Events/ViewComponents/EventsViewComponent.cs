using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snitz.Events.Models;
using Snitz.Events.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Snitz.Events.ViewComponents
{
    public class EventsViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly EventContext _eventContext;

        public EventsViewComponent(SnitzDbContext dbContext, ISnitzConfig config,EventContext eventContext)
        {
            _dbContext = dbContext;
            _config = config;
            _eventContext = eventContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string template,int id = 0,int? topicid = 0)
        {
            if(template == "UpcomingEvents")
            {
                return await Task.FromResult((IViewComponentResult)View(template));
            }
            if (template == "ForumConfig")
            {
                var forum = _dbContext.Forums.Find(id);
                return await Task.FromResult((IViewComponentResult)View(template,forum));
            }
            if (template == "TopicSummary")
            {
                try
                {
                    var eventitem = _eventContext.EventItems.Include(e=>e.Loc).Include(e=>e.Club).Include(e=>e.Cat).FirstOrDefault(e=>e.TopicId == id);
                    return await Task.FromResult((IViewComponentResult)View(template,eventitem));

                }
                catch (Exception)
                {
                    return await Task.FromResult((IViewComponentResult)View());
                }
            }
            if (template == "AddEvent")
            {
                var calendaritem = new CalendarEventItem()
                {
                    AuthorId = id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMinutes(30)
                };                
                if (topicid != null && topicid > 0)
                {
                    calendaritem = _eventContext.EventItems.FirstOrDefault(e=>e.TopicId == topicid);
                }

                return await Task.FromResult((IViewComponentResult)View(template,calendaritem));
            }
            if (template == "EnableButton")
            {
                var installed = _config.TableExists("CAL_EVENTS");
                return await Task.FromResult((IViewComponentResult)View(template,installed));
            }
            if (template == "MenuItem")
            {
                var showcalendar = _config.TableExists("CAL_EVENTS") && _config.GetIntValue("INTCALPUBLIC") == 1 && !(_config.GetIntValue("INTCALPUBLICHOLIDAYS", -1) == -1);

                return await Task.FromResult((IViewComponentResult)View(template,showcalendar));
            }
            if (template == "Config")
            {
                var vm = new EventsAdminViewModel
                {
                    Categories = _dbContext.Set<ClubCalendarCategory>().ToDictionary(t => t.Id, t => t.Name),
                    Locations = _dbContext.Set<ClubCalendarLocation>().ToDictionary(t => t.Id, t => t.Name),
                    Clubs = _dbContext.Set<ClubCalendarClub>().ToDictionary(t => t.Id, t => t.ShortName),
                    Subs = _config.TableExists("EVENT_SUBSCRIPTIONS"),
                    EnableEvents = _config.GetIntValue("INTCALEVENTS") == 1
                };

                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }

            if (template == "Admin")
            {
                int dow = _config.GetIntValue("INTFIRSTDAYOFWEEK");
                var vm = new CalAdminViewModel
                {
                    Installed = _config.TableExists("CAL_EVENTS"),
                    Enabled = _config.GetIntValue("INTCALEVENTS") == 1,
                    EnableEvents = _config.GetIntValue("INTCLUBEVENTS") == 1,
                    ShowInCalendar = _config.GetIntValue("INTCALSHOWEVENTS") == 1,
                    MaxRecords = _config.GetIntValue("INTCALMAXRECORDS") == 0
                        ? 20
                        : _config.GetIntValue("INTCALMAXRECORDS"),
                    FirstDayofWeek = (CalEnums.CalDays) dow,
                    UpcomingEvents = _config.GetIntValue("INTCALUPCOMINGEVENTS") == 1,
                    ShowBirthdays = _config.GetIntValue("INTCALSHOWBDAYS") == 1,
                    PublicHolidays = _config.GetIntValue("INTCALPUBLICHOLIDAYS") == 1,
                    CountryCode = _config.GetValue("STRCALCOUNTRY"),
                    Region = _config.GetValue("STRCALREGION"),
                    IsPublic = _config.GetIntValue("INTCALPUBLIC") == 1,
                    Roles = _config.GetValue("STRRESTRICTROLES"),
                    Countries = GetCountries()
                };
                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }
            return await Task.FromResult((IViewComponentResult)View());
        }
        

        private List<EnricoCountry> GetCountries()
        {
            return CacheProvider.GetOrCreate("cal.countries", FetchJsonCountries, TimeSpan.FromMinutes(10));
        }
        private List<EnricoCountry> FetchJsonCountries()
        {

            try
            {
                using var client = new HttpClient();
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get,"https://kayaposoft.com/enrico/json/v1.0?action=getSupportedCountries");
                var response = httpClient.Send(request);
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var responseBody = reader.ReadToEnd();

                var result = JsonSerializer.Deserialize<EnricoCountry[]>(responseBody);
                return result.ToList();

            }
            catch (Exception ex)
            {
                EnricoCountry ec = new EnricoCountry();
                ec.countryCode = "eng";
                ec.fullName = "Error: " + ex.InnerException.Message;
                return new List<EnricoCountry>() { ec };

            }

        }
    }
}

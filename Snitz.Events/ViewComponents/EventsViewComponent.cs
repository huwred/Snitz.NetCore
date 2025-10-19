using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snitz.Events.Models;
using Snitz.Events.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service.Extensions;
using System.Text.Json;

namespace Snitz.Events.ViewComponents
{
    /// <summary>
    /// Represents a view component for rendering various event-related views in the application.
    /// </summary>
    /// <remarks>This view component dynamically renders different views based on the provided template name. 
    /// It supports templates for displaying upcoming events, forum configurations, topic summaries,  event creation
    /// forms, and administrative settings, among others. The data for these views is  retrieved from the database or
    /// configuration settings as appropriate.</remarks>
    public class EventsViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly EventContext _eventContext;
        private readonly IHttpClientFactory _factory;

        public EventsViewComponent(SnitzDbContext dbContext, ISnitzConfig config,EventContext eventContext, IHttpClientFactory factory)
        {
            _dbContext = dbContext;
            _config = config;
            _eventContext = eventContext;
            _factory = factory;
        }

        /// <summary>
        /// Asynchronously invokes a view component with the specified template and optional parameters.
        /// </summary>
        /// <remarks>The behavior of this method depends on the value of the <paramref name="template"/>
        /// parameter: <list type="bullet"> <item> <description> <c>"UpcomingEvents"</c>: Renders the view with no
        /// additional data. </description> </item> <item> <description> <c>"ForumConfig"</c>: Retrieves a forum entity
        /// using the <paramref name="id"/> parameter and renders the view with the forum data. </description> </item>
        /// <item> <description> <c>"TopicSummary"</c>: Retrieves an event item associated with the <paramref
        /// name="id"/> parameter and renders the view with the event data. </description> </item> <item> <description>
        /// <c>"AddEvent"</c>: Creates a new calendar event item or retrieves an existing one based on the <paramref
        /// name="topicid"/> parameter and renders the view with the event data. </description> </item> <item>
        /// <description> <c>"EnableButton"</c>: Checks if the "CAL_EVENTS" table exists and renders the view with a
        /// boolean indicating the result. </description> </item> <item> <description> <c>"MenuItem"</c>: Checks if the
        /// "CAL_EVENTS" table exists and whether calendar events are enabled, then renders the view with a boolean
        /// indicating the result. </description> </item> <item> <description> <c>"Config"</c>: Retrieves administrative
        /// configuration data and renders the view with an <see cref="EventsAdminViewModel"/>. </description> </item>
        /// <item> <description> <c>"Admin"</c>: Retrieves administrative settings and renders the view with a <see
        /// cref="CalAdminViewModel"/>. </description> </item> </list> If the <paramref name="template"/> does not match
        /// any of the predefined cases, an empty view is rendered.</remarks>
        /// <param name="template">The name of the template to render. This determines the view and data model used.</param>
        /// <param name="id">An optional identifier used to retrieve specific data for certain templates. Defaults to 0.</param>
        /// <param name="topicid">An optional topic identifier used to retrieve specific data for certain templates. Defaults to 0.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="IViewComponentResult"/> that renders the specified template with the appropriate data model.</returns>
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
                var showcalendar = _config.TableExists("CAL_EVENTS") && _config.GetIntValue("INTCALEVENTS") == 1;

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
                    Countries = await GetCountries()
                };
                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }
            return await Task.FromResult((IViewComponentResult)View());
        }
        

        private Task<List<EnricoCountry>> GetCountries()
        {
            return CacheProvider.GetOrCreate("cal.countries", FetchJsonCountries, TimeSpan.FromMinutes(10));
        }
        private async Task<List<EnricoCountry>> FetchJsonCountries()
        {

            try
            {

                using var httpClient = _factory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get,"https://kayaposoft.com/enrico/json/v1.0?action=getSupportedCountries");
                var response = await httpClient.SendAsync(request);
                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                var responseBody = reader.ReadToEnd();

                var result = JsonSerializer.Deserialize<EnricoCountry[]>(responseBody);
                return result != null ? [.. result] : [];

            }
            catch (Exception ex)
            {
                EnricoCountry ec = new EnricoCountry
                {
                    countryCode = "eng",
                    fullName = "Error: " + ex.InnerException?.Message
                };
                return [ec];

            }

        }
    }
}

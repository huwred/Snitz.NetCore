using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Snitz.Events.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzEvents.Helpers;
using SnitzEvents.Models;

namespace Snitz.Events.ViewComponents
{
    public class EventsViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;

        public EventsViewComponent(SnitzDbContext dbContext, ISnitzConfig config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        public async Task<IViewComponentResult> InvokeAsync(string template)
        {
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
            var service = new InMemoryCache(600);

            return service.GetOrSet("cal.countries", FetchJsonCountries);
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
    }
}

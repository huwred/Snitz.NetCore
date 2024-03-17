using SnitzEvents.Helpers;
using SnitzEvents.Models;

namespace Snitz.Events.ViewModels
{
    public class CalAdminViewModel
    {
        public bool Installed { get; set; }
        public bool Enabled { get; set; }
        public bool EnableEvents { get; set; }
        public CalEnums.CalAllowed IntCalMlev { get; set; }
        public int MaxRecords { get; set; }
        public bool ShowBirthdays { get; set; }
        public CalEnums.CalDays FirstDayofWeek { get; set; }
        public bool UpcomingEvents { get; set; }
        public bool PublicHolidays { get; set; }
        public string CountryCode { get; set; }
        public string Region { get; set; }
        public string Roles { get; set; }
        public List<EnricoCountry> Countries { get; set; }
        public bool IsPublic { get; set; }
        public bool ShowInCalendar { get; set; }
    }
}

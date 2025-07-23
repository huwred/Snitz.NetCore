using Snitz.Events.Models;

namespace Snitz.Events.ViewModels
{
    public class EventsAdminViewModel
    {
        
        public Dictionary<int, string> Categories { get; set; }
        public Dictionary<int, string> Locations { get; set; }
        public Dictionary<int, string> Clubs { get; set; }

        public bool EnableEvents { get; set; }

        public bool ShowInCalendar { get; set; }

        public string? Roles { get; set; }
        public bool Subs { get; set; }
    }

    public class EditListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }

        public string ListType { get; set; }
    }
    public class EditClubViewModel
    {
        public int Id { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Abbr { get; set; }
        public int Order { get; set; }
        public ClubCalendarLocation DefaultLocation { get; set; }

    }
}

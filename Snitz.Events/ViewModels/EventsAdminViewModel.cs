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
}

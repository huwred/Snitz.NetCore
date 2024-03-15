using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

using Snitz.Events.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SnitzEvents.ViewModels
{
    public class ClubEventViewModel : AgendaViewModel
    {
        public int Id { get; set; }
        [Required]
        public new string Title { get; set; }
        [Required]
        public new string Description { get; set; }

        //public int? ClubId { get; set; }
        //[Range(1, int.MaxValue, ErrorMessage = "Select a Category")]
        //public int? CatId { get; set; }
        //public int? LocId { get; set; }
        [Required]
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        //public Dictionary<int, string> Categories { get; set; }
        //public Dictionary<int, string> Locations { get; set; }
        //public Dictionary<int, string> Clubs { get; set; }

        //public List<string> AllowedRoles { get; set; }
    }

    public class AgendaViewModel
    {
        public int? ClubId { get; set; }
        public int? CatId { get; set; }
        public int? LocId { get; set; }
        public Dictionary<int, string> Categories { get; set; }
        public Dictionary<int, string> Locations { get; set; }
        public Dictionary<int, string> Clubs { get; set; }

        public List<string> AllowedRoles { get; set; }
        public List<CatSummary> CatSummary { get; internal set; }
    }

    public class SubscribeModel
    {

        // This property contains the available options
        public Dictionary<int, string> SubscriptionSources { get; set; }

        // This property contains the selected options
        public IEnumerable<int> SelectedSources { get; set; }

        public SubscribeModel()
        {
            Dictionary<int, string> clubs = new Dictionary<int, string>();
            //using (EventsRepository db = new EventsRepository())
            //{
            //    foreach (KeyValuePair<int, string> forum in db.GetClubsList().ToDictionary(t => t.Key, t => t.Value))
            //    {
            //        clubs.Add(forum.Key, BbCodeProcessor.Format(forum.Value));
            //    }
            //    SubscriptionSources =  clubs;
            //}

        }
    }

}
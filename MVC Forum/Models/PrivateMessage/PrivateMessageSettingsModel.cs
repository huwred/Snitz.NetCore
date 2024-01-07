using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVC_Forum.Models.Forum
{
    public class PrivateMessageSettingsModel
    {
        [Display(Name = "Receive Messages")]
        public bool RecievePM { get; set; }
        public bool EmailNotification { get; set; }
        [Display(Name = "Save Sent Messages")]
        public bool SaveSentMessages { get; set;}

        public IEnumerable<PrivateMessageBlocklist> BlockedList { get; set;}
    }
}
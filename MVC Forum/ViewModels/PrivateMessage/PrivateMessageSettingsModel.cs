using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SnitzCore.Data.Models;

namespace MVCForum.ViewModels.PrivateMessage
{
    public class PrivateMessageSettingsModel
    {
        [Display(Name = "Receive Messages")]
        public bool RecievePM { get; set; }
        public bool EmailNotification { get; set; }
        [Display(Name = "Save Sent Messages")]
        public bool SaveSentMessages { get; set; }

        public IEnumerable<PrivateMessageBlocklist>? BlockedList { get; set; }
    }
}
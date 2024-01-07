using System.Collections.Generic;

namespace MVC_Forum.Models.Forum
{
    public class PrivateMessageIndexModel
    {
        public int MemberId { get; set; }
        public  IEnumerable<PrivateMessageListingModel>? Inbox { get; set; }
        public  IEnumerable<PrivateMessageListingModel>? Outbox { get; set; }
        public PrivateMessageSettingsModel? Settings { get; set; }
    }
}

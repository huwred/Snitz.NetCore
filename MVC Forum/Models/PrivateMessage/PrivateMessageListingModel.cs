using System;

namespace MVC_Forum.Models.Forum
{
    public class PrivateMessageListingModel
    {
        public int Id { get; set; }

        public bool Read { get; set; }
        public DateTime? Sent { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string ToMemberName { get; set; }
        public string FromMemberName { get; set; }
        public int ToMemberId { get; set; }
        public int FromMemberId { get; set; }
    }
}

using System;

namespace MVCForum.ViewModels.User
{
    public class MemberListingModel
    {
        public int Id { get; set; }

        public DateTime? LastHereDate { get; set; }

        public string? Title { get; set; }
        public DateTime? LastPost { get; set; }
        public DateTime? MemberSince { get; set; }

        public SnitzCore.Data.Models.Member Member { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
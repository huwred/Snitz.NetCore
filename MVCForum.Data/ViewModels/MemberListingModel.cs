using SnitzCore.Data.Models;
using System;

namespace SnitzCore.Data.ViewModels
{
    public class MemberListingModel
    {
        public int Id { get; set; }

        public DateTime? LastLogin { get; set; }

        public string? Title { get; set; }
        public DateTime? LastPost { get; set; }
        public DateTime? MemberSince { get; set; }

        public Models.Member Member { get; set; } = null!;
        public bool IsActive { get; set; }
        //public ForumUser? ForumUser { get; set; }

        public bool Migrated { get; set; }
    }
}
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels.Forum
{
    public class NewForumModel
    {
        public int Id { get; set; }
        [Required]
        public int Category { get; set; }
        [Required]
        public string Subject { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public ForumType Type { get; set; }

        public bool Status { get; set; }
        public int Order { get; set; } = 99;
        [Display(Name = "Default view")]
        public DefaultDays DefaultView { get; set; } = DefaultDays.Last30Days;

        [Display(Name = "Allowed Acccess")]
        public ForumAuthType AuthType { get; set; }
        public ForumSubscription Subscription { get; set; }

        public Moderation Moderation { get; set; }

        [Display(Name = "Increment post count")]
        public bool IncrementMemberPosts { get; set; } = true;
        public Dictionary<int, string>? CategoryList { get; set; }
        public int ForumId { get; set; }
    }
}

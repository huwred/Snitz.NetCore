using MVCForum.ViewModels.Forum;
using System;

namespace MVCForum.ViewModels.Post
{
    public class PostListingModel : PostBase
    {
        public string? Title { get; set; }
        public string Message { get; set; } = null!;
        public ForumListingModel? Forum { get; set; }

        public int RepliesCount { get; set; }
        public int ViewCount { get; set; }
        public int UnmoderatedReplies { get; set; }

        //public DateTime Edited { get; set; }
        public DateTime? LastPostDate { get; set; }
        public string? LastPostAuthorName { get; set; }

        public bool IsSticky { get; set; }
        public int Status { get; set; }
        public int? LatestReply { get; set; }

        public bool Answered { get; set; }
        public bool HasPoll { get; set; }

        public int ForumAllowRating {get;set;}
        public int AllowRating {get;set;}

        public decimal Rating {get;set;}
    }
}

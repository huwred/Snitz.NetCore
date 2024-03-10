using MVCForum.ViewModels.Forum;
using System;

namespace MVCForum.ViewModels.Post
{
    public class PostListingModel : PostBase
    {
        public string? Title { get; set; }
        public string Message { get; set; } = null!;
        public ForumListingModel? Forum { get; set; }

        public int RepliesCount;
        public int ViewCount;

        //public DateTime Edited { get; set; }
        public DateTime? LastPostDate { get; set; }
        public string? LastPostAuthorName { get; set; }

        public bool IsSticky { get; set; }
        public int Status { get; set; }
        public int? LatestReply { get; set; }

        public bool Answered { get; set; }
    }
}

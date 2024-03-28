using System.Collections.Generic;

namespace MVCForum.ViewModels.Post
{
    public class PostIndexModel : PostBase
    {
        public string Title { get; set; } = null!;

        public int ForumId { get; set; }
        public string ForumName { get; set; } = null!;

        public IEnumerable<PostReplyModel>? Replies { get; set; }
        public int PageCount { get; set; }

        public string? SortDir { get; set; }
        public int Views { get; set; }
        public SnitzCore.Data.Models.Member Author { get; set; } = null!;
        public bool IsLocked { get; set; }
        public bool IsSticky { get; set; }
        public int PageNum { get; set; }
        public int PageSize { get; set; }

        public bool Answered { get; set; }

        public bool ShowSig { get; set; }

        public int Status { get; set; }

        public bool HasPoll { get; set; }
    }
}

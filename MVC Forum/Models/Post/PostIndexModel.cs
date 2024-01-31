using System.Collections.Generic;

namespace MVCForum.Models.Post
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
        public int PageNum { get; set; }
    }
}

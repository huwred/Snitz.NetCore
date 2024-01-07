using System.Collections.Generic;

namespace MVCForum.Models.Post
{
    public class PostIndexModel : PostBase
    {
        public string Title { get; set; }

        public int ForumId { get; set; }
        public string ForumName { get; set; }
           
        public IEnumerable<PostReplyModel> Replies { get; set; }
        public int PageCount { get; set; }

        public string SortDir { get; set; }
        public int Views { get; set; }
        public SnitzCore.Data.Models.Member Author { get; set; }
        public bool IsLocked { get; set; }
        public int PageNum { get; set; }
    }
}

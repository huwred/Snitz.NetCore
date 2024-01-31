using MVCForum.Models.Post;
using System.Collections.Generic;

namespace MVCForum.Models.Forum
{
    public class ForumIndexModel
    {
        public  IEnumerable<ForumListingModel>? ForumList { get; set; }
        public IEnumerable<PostListingModel>? LatestPosts { get; set; }
    }
}

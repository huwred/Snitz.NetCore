using MVCForum.ViewModels.Post;
using System.Collections.Generic;

namespace MVCForum.ViewModels.Forum
{
    public class ForumIndexModel
    {
        public IEnumerable<ForumListingModel>? ForumList { get; set; }
        public IEnumerable<PostListingModel>? LatestPosts { get; set; }
    }
}

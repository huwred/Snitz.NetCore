using MVCForum.ViewModels.Post;
using System.Collections.Generic;
using System.Linq;

namespace MVCForum.ViewModels.Forum
{
    public class ForumIndexModel
    {
        public IEnumerable<ForumListingModel>? ForumList { get; set; }
        public IEnumerable<PostListingModel>? LatestPosts { get; set; }
        public IEnumerable<SnitzCore.Data.Models.Category>? Categories { get; internal set; }

    }
}

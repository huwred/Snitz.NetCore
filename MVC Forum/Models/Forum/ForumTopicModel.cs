using MVCForum.Models.Post;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using MVCForum.Extensions;

namespace MVCForum.Models.Forum
{
    public class ForumTopicModel
    {
        public ForumListingModel? Forum { get; set; }
        public IEnumerable<PostListingModel>? Posts { get; set; }
        public int PageCount { get; set; }
        public IEnumerable<PostListingModel>? StickyPosts { get; set; }
        public int? PageNum { get; set; }
    }

    public class ActiveTopicModel
    {
        //public ForumListingModel? Forum { get; set; }
        public IEnumerable<PostListingModel>? Posts { get; set; }
        public int PageCount { get; set; }
        public ActiveSince Since { get; set; }
        public ActiveRefresh Refresh { get; set; }
        public int? PageNum { get; set; }
    }
}

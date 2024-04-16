using SnitzCore.Data.Models;
using System.Collections.Generic;
using SnitzCore.Data.Extensions;
using MVCForum.ViewModels.Post;

namespace MVCForum.ViewModels.Forum
{
    public class ForumTopicModel
    {
        public ForumListingModel? Forum { get; set; }
        public IEnumerable<PostListingModel>? Posts { get; set; }
        public int PageCount { get; set; }
        public IEnumerable<PostListingModel>? StickyPosts { get; set; }
        public int? PageNum { get; set; }
        public int PageSize { get; set; } = 10;

        public bool PasswordRequired { get; set; }
        public bool AccessDenied { get; set; }

        public bool Archives {get; set; }
    }

    public class ActiveTopicModel
    {
        //public ForumListingModel? Forum { get; set; }
        public IEnumerable<PostListingModel>? Posts { get; set; }
        public int PageCount { get; set; }
        public ActiveSince Since { get; set; }
        public ActiveRefresh Refresh { get; set; }
        public int? PageNum { get; set; }
        public int PageSize { get; set; } = 20;
    }
}

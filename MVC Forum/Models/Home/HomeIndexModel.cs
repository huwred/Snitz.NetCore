using MVCForum.Models.Post;
using System.Collections.Generic;

namespace MVCForum.Models.Home
{
    public class HomeIndexModel
    {
        public string? SearchQuery { get; set; }
        public IEnumerable<PostListingModel>? LatestPosts { get; set; }
    }
}

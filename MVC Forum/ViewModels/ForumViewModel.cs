using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System.Collections.Generic;

namespace MVCForum.ViewModels
{
    public class ForumViewModel
    {
        public int Id { get; set; }
        public required SnitzCore.Data.Models.Forum Forum { get; set; }
        public required List<SnitzCore.Data.Models.Post> Topics { get; set; }
        public List<SnitzCore.Data.Models.Post>? StickyTopics { get; set; } 

        //Paging params
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public long TotalRecords { get; set; }
        public int Page { get; set; }

        //right column collections
        public List<SnitzCore.Data.Models.Post>? RecentTopics { get; set; }
        public DefaultDays DefaultDays { get; set; }
        public ActiveSince ActiveSince { get; set; }
        public string? OrderBy { get; set; }
        public string? SortDir { get; set; }
    }
}

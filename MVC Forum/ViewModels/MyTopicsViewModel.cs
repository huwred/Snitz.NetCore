using System.Collections.Generic;
using SnitzCore.Data.Models;
using X.PagedList;

namespace MVCForum.ViewModels
{
    public class MyTopicsViewModel
    {
        public MyTopicsSince ActiveSince { get; set; }
        public ActiveRefresh Refresh { get; set; }
        public PagedList<SnitzCore.Data.Models.Post> Topics { get; set; }
        public int PageCount { get; internal set; }
        public int Page { get; internal set; }
        public long TotalRecords { get; internal set; }
        public IEnumerable<MyViewTopic> AllTopics { get; set; }
        public DefaultDays DefaultDays { get; set; }
    }


}

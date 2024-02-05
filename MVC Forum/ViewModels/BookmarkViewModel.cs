using SnitzCore.Data.Models;
using System.Collections.Generic;
using SnitzCore.Data.Extensions;

namespace MVCForum.ViewModels
{
    public class BookmarkViewModel
    {
        public int MemberId { get; set; }
        public List<BookmarkEntry>? Bookmarks { get; set; }
        public ActiveRefresh Refresh { get; set; } = ActiveRefresh.None;
        public ActiveSince ActiveSince { get; set; } = ActiveSince.LastDay;

    }
}

using SnitzCore.Data.ViewModels;
using System.Collections.Generic;

namespace MVCForum.ViewModels.User
{
    public class MemberIndexModel
    {
        public IEnumerable<MemberListingModel>? MemberList { get; set; }
        public int PageCount { get; set; }
        public int PageNum { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
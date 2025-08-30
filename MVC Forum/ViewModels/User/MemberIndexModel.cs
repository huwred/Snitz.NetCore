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
        public string? SortCol {get;set;}
        public string? SortDir {get;set;}
        public string? Initial {get;set;}
    }
}
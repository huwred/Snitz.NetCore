using Microsoft.AspNetCore.Mvc;
using MVCForum.ViewModels.Member;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MVCForum.View_Components
{
    public class MembersViewComponent : ViewComponent
    {
        private readonly IMember _memberService;
        private readonly ICategory _catService;
        public MembersViewComponent(IMember memberService, ICategory catService)
        {
            _memberService = memberService;
            _catService = catService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? max,string? template = null,MemberDetailModel? member = null)
        {
            if(template == "Public")
            {
                return await Task.FromResult((IViewComponentResult)View(template,member));

            }else if(template == "Edit")
            {
                return await Task.FromResult((IViewComponentResult)View(template,member));
            }
            else if (template == "CategoryForumList")
            {
                var key = "CategoryForumList_" + User.Identity?.Name;
                var categories = 
                CacheProvider.GetOrCreate(key, () => _catService.FetchCategoryForumList(User), TimeSpan.FromMinutes(10));

                return await Task.FromResult((IViewComponentResult)View(template,categories.ToList()));
            }
            else if (template == "CategoryForumJumpTo")
            {
                var key = "CategoryForumJumpTo_" + User.Identity?.Name;
                var categories = 
                CacheProvider.GetOrCreate(key, () => _catService.FetchCategoryForumList(User), TimeSpan.FromMinutes(10));

                return await Task.FromResult((IViewComponentResult)View(template,categories.ToList()));
            }
            var recentMembers = _memberService.GetRecent(max!.Value).ToList();
            if(template != null) {
                return await Task.FromResult((IViewComponentResult)View(template,recentMembers));
                }

            return await Task.FromResult((IViewComponentResult)View(recentMembers));
        }
    }
}

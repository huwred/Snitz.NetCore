using Microsoft.AspNetCore.Mvc;
using MVCForum.ViewModels.Member;
using SnitzCore.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MVCForum.View_Components
{
    public class MembersViewComponent : ViewComponent
    {
        private readonly IMember _memberService;
        public MembersViewComponent(IMember memberService)
        {
            _memberService = memberService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int max,string? template = null,MemberDetailModel? member = null)
        {
            if(template == "Public")
            {
                return await Task.FromResult((IViewComponentResult)View(template,member));

            }else if(template == "Edit")
            {
                return await Task.FromResult((IViewComponentResult)View(template,member));
            }

            var recentMembers = _memberService.GetRecent(max).ToList();
            if(template != null) {
                return await Task.FromResult((IViewComponentResult)View(template,recentMembers));
                }

            return await Task.FromResult((IViewComponentResult)View(recentMembers));
        }
    }
}

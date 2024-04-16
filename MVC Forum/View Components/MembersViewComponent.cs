using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using System.Linq;
using System.Net;
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

        public async Task<IViewComponentResult> InvokeAsync(int max)
        {
            var recentMembers = _memberService.GetRecent(max).ToList();
            return await Task.FromResult((IViewComponentResult)View(recentMembers));
        }
    }
}

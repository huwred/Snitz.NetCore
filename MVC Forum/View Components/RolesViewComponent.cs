using BbCodeFormatter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace MVCForum.View_Components
{
    public class RolesViewComponent : ViewComponent
    {
        private readonly IForum _forumService;
        private readonly UserManager<ForumUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolesViewComponent(IForum forumService, IWebHostEnvironment webHostEnvironment, UserManager<ForumUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _forumService = forumService;
            _environment = webHostEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
            // Constructor logic if needed
        }
        public async Task<IViewComponentResult> InvokeAsync(string template, int? id = null)
        {
            if(template == "Moderators" && id != null && id != 0)
            {
                var names = _forumService.GetModerators(id.Value);

                ViewBag.ForumId = id;
                return await Task.FromResult((IViewComponentResult)View(template, names));
            }
            if(template == "ModImages" && id != null && id != 0)
            {
                var names = _forumService.GetModerators(id.Value);

                ViewBag.ForumId = id;
                return await Task.FromResult((IViewComponentResult)View(template, names));
            }
            return Content(string.Empty);
        }
    }
}

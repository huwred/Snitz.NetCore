using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MVCForum.Controllers
{
    public class SnitzBaseController : Controller
    {
        protected ISnitzConfig _config;
        protected IMember _memberService;
        protected LanguageService  _languageResource;
        protected SnitzDbContext _snitzDbContext;
        protected IHttpContextAccessor? _httpContextAccessor;
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SnitzBaseController(IMember memberService, ISnitzConfig config,IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor)
        {

            _config = config;
            _memberService = memberService;
            _snitzDbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");

        }
        public IActionResult RefreshCaptcha()
        {
            return ViewComponent("Captcha");
        }
        public JsonResult AutoCompleteUsername(string term)
        {
            IEnumerable<string> result = _memberService.GetAll(User.IsInRole("Administrator")).Where(m=>m?.Status == 1 && m!.Name.ToLower().Contains(term.ToLower())).Select(m=>m!.Name);
            
            return Json(result);
        }
        public JsonResult AutoCompleteModerator(string term)
        {
            var modlist = CacheProvider.GetOrCreate("Moderators",()=>Moderators(),TimeSpan.FromMinutes(60));
            return Json(modlist);
        }

        private List<string> Moderators()
        {
            IEnumerable<string> result = _memberService.GetUsersInRoleAsync("Moderator").Result.Where(m=>m?.Status == 1).Select(m=>m!.Name);
            var oldmods = _snitzDbContext.Members.AsNoTracking().Where(m=>m.Level>1).Select(o => o.Name).ToList();
            return result.Union(oldmods).ToList();
        }

        public IActionResult Error()
        {
            return View();
        }

    }
}

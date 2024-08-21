using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service;

namespace MVCForum.Controllers
{
    public class SnitzController : Controller
    {
        protected ISnitzConfig _config;
        protected IMember _memberService;
        protected LanguageService  _languageResource;
        protected SnitzDbContext _snitzDbContext;
        protected IHttpContextAccessor _httpContextAccessor;
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        public SnitzController(IMember memberService, ISnitzConfig config,IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor)
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
            IEnumerable<string> result = _memberService.GetAll(User.IsInRole("Administrator")).Where(m=>m.Status == 1).Where(r => r!.Name.ToLower().Contains(term.ToLower())).Select(m=>m!.Name);

            return Json(result);
        }
        public static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }

    }
}

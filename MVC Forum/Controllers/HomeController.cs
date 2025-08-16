using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Home;
using MVCForum.ViewModels.Post;

namespace MVCForum.Controllers
{
    [DefaultBreadcrumb("mnuHome")]
    public class HomeController : SnitzBaseController
    {
        private readonly ISnitzCookie _snitzcookie;

        public HomeController(IMember memberService, ISnitzConfig config,IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor, ISnitzCookie snitzcookie) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _snitzcookie = snitzcookie;
        }

        public IActionResult Index()
        {            

            try
            {
                return View(new HomeIndexModel
                {
                    SearchQuery = ""
                });
            }
            catch (Exception e)
            {
                _logger.Error("BuildHomeIndexModel",e);
                return View("TempIndex");
            }
        }

        [Route("About")]
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SetLanguage(string? lang, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = _config.ForumUrl!;
            }
            if (string.IsNullOrWhiteSpace(lang))
            {
                lang = "en";
            }
            _snitzcookie.SetCookie(
                "CookieLang",
                lang,
                DateTime.UtcNow.AddMonths(12) 
            );

            try
            {
                NumberFormatInfo numberInfo = CultureInfo.CreateSpecificCulture(lang).NumberFormat;
                CultureInfo info = new CultureInfo(lang)
                {
                    NumberFormat = numberInfo
                };
                //later, we will if-else the language here
                //info.DateTimeFormat.DateSeparator = "/";
                //info.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                CultureInfo.CurrentUICulture = info;
                CultureInfo.CurrentCulture = info;
            }
            catch (Exception)
            {

            }
            //_logger.Warn("ReturnUrl4:" + returnUrl);
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return LocalRedirect("~/");
        }

    }
}

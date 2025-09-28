using BbCodeFormatter;
using BbCodeFormatter.Processors;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Home;
using MVCForum.ViewModels.Post;
using SmartBreadcrumbs.Attributes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MVCForum.Controllers
{
    [DefaultBreadcrumb("mnuHome")]
    public class HomeController : SnitzBaseController
    {
        private readonly ISnitzCookie _snitzcookie;
        private readonly IPost _postService;
        private readonly IAdRotator _banner;
        private readonly ICodeProcessor _bbcodeProcessor;

        public HomeController(ICodeProcessor bbcodeProcessor, IMember memberService, IAdRotator banner, ISnitzConfig config,IPost postservice, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor, ISnitzCookie snitzcookie) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _snitzcookie = snitzcookie;
            _postService = postservice;
            _banner = banner;
            _bbcodeProcessor = bbcodeProcessor;
        }

        //[ResponseCache(Duration = 240, Location = ResponseCacheLocation.Any)]
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
                return View("Error");
            }
        }
        public IActionResult Preview (string content)
        {
            var processed = _bbcodeProcessor.Format(content);
            return Content(processed);
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
        public PartialViewResult RefreshRecentTopics(int id = 0, int forumid= 0, bool sidebar=false)
        {
            return PartialView("_RecentTopics", _postService.GetAllTopicsAndRelated()
                .Take(5).ToList());
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

        public ActionResult RecordClick(string id)
        {
            // ..
            // log what you need here
            // ..
            var thisUrl = "/";

            var ads = _banner.GetAds("Admin");
            if (ads != null)
            {
                var singleOrDefault = ads.Adverts.SingleOrDefault(a => a.Id.ToString() == id);
                if (singleOrDefault != null)
                {
                    singleOrDefault.Clicks += 1;
                    thisUrl = singleOrDefault.Url;
                    _banner.Save(ads);
                }
            }
            // finally redirect to the link URL
            return Redirect(thisUrl);
        }
    }
}

using BbCodeFormatter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Home;
using NuGet.Configuration;
using SmartBreadcrumbs;
using SmartBreadcrumbs.Attributes;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service.Extensions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MVCForum.Controllers
{
    [DefaultBreadcrumb("mnuHome")]
    public class HomeController : SnitzBaseController
    {
        private readonly ISnitzCookie _snitzcookie;
        private readonly IPost _postService;
        private readonly IAdRotator _banner;
        private readonly ICodeProcessor _bbcodeProcessor;
        private readonly ISnitzConfig _config;
        private readonly IWebHostEnvironment _env;
        private string LandingPage;

        public HomeController(ICodeProcessor bbcodeProcessor, IMember memberService, IAdRotator banner, ISnitzConfig config,IPost postservice, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor, ISnitzCookie snitzcookie,IConfiguration settings,IWebHostEnvironment env) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _snitzcookie = snitzcookie;
            _postService = postservice;
            _banner = banner;
            _bbcodeProcessor = bbcodeProcessor;
            _config = config;
            LandingPage = settings.GetValue<string>("SnitzForums:LandingPage","")!;
            _env = env;
        }

        //[ResponseCache(Duration = 240, Location = ResponseCacheLocation.Any)]
        public IActionResult Index()
        {
            if (!string.IsNullOrWhiteSpace(LandingPage))
            {
                BreadcrumbManager.Options.DontLookForDefaultNode = true;
                return RedirectPermanent(_config.RootFolder + "/AllForums");
            }
            
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

        [Route("ForumUpload/")]
        public IActionResult ForumUpload(IFormFile file)
        {
            if (_memberService.Current() == null)
            {
                return BadRequest();
            }
            var location = UploadImageToServer(file);
            if(_logger.IsDebugEnabled)
                _logger.Debug("UploadImageToServer:" + location);
            return Json(new { location });
        }
       public string UploadImageToServer(IFormFile file)
        {
            var uploadFolder = Path.Combine(_env.WebRootPath, _config.ContentFolder, "Members");
            var memberid = _memberService.Current()!.Id;
            var uniqueFileName = "";
            var fullFilePath = "";
            if (file != null)
            {
                var uploadfilepath = Path.Combine(uploadFolder, memberid.ToString());
                uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                fullFilePath = Path.Combine(uploadfilepath, uniqueFileName);
                file.CopyTo(new FileStream(fullFilePath, FileMode.Create));
                var imagepath = StringExtensions.UrlCombine(_config.ContentFolder, "Members");
                return StringExtensions.UrlCombine(imagepath, memberid.ToString()) + "/" + uniqueFileName;
            }
            return "";
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

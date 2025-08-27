using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Home;
using MVCForum.ViewModels.Post;
using SmartBreadcrumbs.Attributes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

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
//        var passwordHasher = new PasswordHasher<IdentityUser>();

//        // Create a dummy user (can be replaced with your actual user model)
//        var user = new ForumUser();
//            user.UserName = "Administrator";

//            // Generate the hashed password
//            string password = "Passw0rd!";
//        string hashedPassword = passwordHasher.HashPassword(user, password);
//_logger.Info($"Hashed: {hashedPassword}");

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

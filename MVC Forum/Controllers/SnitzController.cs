using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
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


        public static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }

    }
}

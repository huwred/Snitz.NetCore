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
        protected static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public SnitzController(IMember memberService, ISnitzConfig config,IHtmlLocalizerFactory localizerFactory)
        {
            _config = config;
            _memberService = memberService;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
        }
    }
}

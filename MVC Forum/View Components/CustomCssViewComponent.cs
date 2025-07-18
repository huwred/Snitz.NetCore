using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using System.Threading.Tasks;

namespace MVCForum.View_Components
{
    public class SnitzThemeCssViewComponent : ViewComponent
    {
        private readonly ISnitzCookie _cookie;
        private readonly ISnitzConfig _config;

        public SnitzThemeCssViewComponent(ISnitzCookie snitzCookie,ISnitzConfig snitzConfig)
        {
            _cookie = snitzCookie;
            _config = snitzConfig;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var theme = _cookie.GetCookieValue("snitztheme");
            return await Task.FromResult((IViewComponentResult)View("theme",theme??_config.GetValueWithDefault("STRDEFAULTTHEME", "SnitzTheme")));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using System.Threading.Tasks;

namespace MVCForum.View_Components
{
    public class SnitzThemeCssViewComponent : ViewComponent
    {
        private readonly ISnitzCookie _cookie;

        public SnitzThemeCssViewComponent(ISnitzCookie snitzCookie)
        {
            _cookie = snitzCookie;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var theme = _cookie.GetCookieValue("snitztheme");
            return await Task.FromResult((IViewComponentResult)View("theme",theme??"SnitzTheme"));
        }
    }
}

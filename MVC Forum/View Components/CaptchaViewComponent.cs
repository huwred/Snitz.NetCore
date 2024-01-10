using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using MediaWiz.Forums.Helpers;

namespace MVCForum.View_Components
{
    public class CaptchaViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CaptchaViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<IViewComponentResult> InvokeAsync(Guid UserId)
        {
            Captcha captcha = new Captcha(200, 80, 6);
            TempData["b64"] = captcha.GenerateAsB64(Captcha.CaptchaType.Circle);
            _httpContextAccessor.HttpContext.Session.SetString("Captcha", captcha.GetAnswer());

            return await Task.FromResult((IViewComponentResult)View("Captcha"));
        }

    }
}

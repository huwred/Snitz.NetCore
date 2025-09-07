using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace MVCForum.View_Components
{
    public class AdRotatorViewComponent : ViewComponent
    {
        private readonly IAdRotator _adRotator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AdRotatorViewComponent(IAdRotator adRotator, IHttpContextAccessor httpContextAccessor)
        {
            _adRotator = adRotator;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IViewComponentResult> InvokeAsync(string template = "Admin")
        {
            var ads = _adRotator.GetAds(template);
            if(template == "Admin")
            {
                return await Task.FromResult((IViewComponentResult)View(template, ads));
            }

            if (ads.Adverts != null && ads.Adverts.Any())
            {
                int totalWeight = ads.Adverts.Sum(a => a.Weight);
                var selectedAd = _adRotator.GetAd(ads.Adverts, totalWeight);
                selectedAd.Impressions += 1;

                _adRotator.Save(selectedAd);
                return await Task.FromResult((IViewComponentResult)View(template, selectedAd));
            }
            else
            {
                return Content(string.Empty);
            }
        }
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Snitz.PhotoAlbum.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service;

namespace Snitz.PhotoAlbum.ViewComponents
{
    public class ImageAlbumViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly LanguageService _languageResource;
        private readonly IMember _memberService;
        private readonly IWebHostEnvironment _environment;
        public ImageAlbumViewComponent(IWebHostEnvironment hostingEnvironment,SnitzDbContext dbContext, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,IMember memberService)
        {
            _dbContext = dbContext;
            _config = config;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
            _memberService = memberService;
            _environment = hostingEnvironment;
        }
        public async Task<IViewComponentResult> InvokeAsync(string template)
        {
            if (template == "Config")
            {
                return await Task.FromResult((IViewComponentResult)View(template,_config.TableExists("FORUM_IMAGES")));
            }
            if (template == "Groups")
            {
                var albumgroups = _dbContext.Set<AlbumGroup>().AsQueryable();
                return await Task.FromResult((IViewComponentResult)View(template,albumgroups));
            }

            if (template == "Featured")
            {
                var ids = _dbContext.Set<AlbumImage>().Where(a=> !a.DoNotFeature).Select(a=>a.Id).ToList();
                Random r = new Random();
                int index = r.Next(0, ids.Count()); // list.Count for List<T>

                int oneRandom = ids[index];

                var photo = _dbContext.Set<AlbumImage>().AsNoTracking().Include(a=>a.Member).SingleOrDefault(a=> a.Id == oneRandom);
                if(photo != null)
                {
                    var image = Path.Combine(_environment.WebRootPath, _config.ContentFolder, "PhotoAlbum", photo.ImageName);
                    if(!File.Exists(image))
                    {
                        return await Task.FromResult((IViewComponentResult)View());
                    }
                }else{
                    return await Task.FromResult((IViewComponentResult)View());
                }


                return await Task.FromResult((IViewComponentResult)View(template,photo));

            }
            return await Task.FromResult((IViewComponentResult)View());
        }
    }
}

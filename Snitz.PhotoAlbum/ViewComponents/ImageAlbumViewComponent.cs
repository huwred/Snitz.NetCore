using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Snitz.PhotoAlbum.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Service;

namespace Snitz.PhotoAlbum.ViewComponents
{
    public class ImageAlbumViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly LanguageService _languageResource;
        private readonly IMember _memberService;
        public ImageAlbumViewComponent(SnitzDbContext dbContext, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,IMember memberService)
        {
            _dbContext = dbContext;
            _config = config;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
            _memberService = memberService;
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

                var photo = _dbContext.Set<AlbumImage>().Include(a=>a.Member).SingleOrDefault(a=> a.Id == oneRandom);

                return await Task.FromResult((IViewComponentResult)View(template,photo));

            }
            return await Task.FromResult((IViewComponentResult)View());
        }
    }
}

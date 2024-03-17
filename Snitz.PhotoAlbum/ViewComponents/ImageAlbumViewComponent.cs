using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snitz.PhotoAlbum.ViewComponents
{
    public class ImageAlbumViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        public ImageAlbumViewComponent(SnitzDbContext dbContext, ISnitzConfig config)
        {
            _dbContext = dbContext;
            _config = config;
        }
        public async Task<IViewComponentResult> InvokeAsync(string template)
        {
            if (template == "Config")
            {
                return await Task.FromResult((IViewComponentResult)View(template,_config.TableExists("FORUM_IMAGES")));
            }
            return await Task.FromResult((IViewComponentResult)View());
        }
    }
}

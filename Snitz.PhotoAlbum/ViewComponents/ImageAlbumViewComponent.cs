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
        /// <summary>
        /// Invokes the view component asynchronously with the specified template and optional parameters.
        /// </summary>
        /// <remarks>The behavior of the method depends on the value of the <paramref name="template"/>
        /// parameter: <list type="bullet"> <item> <description> If <paramref name="template"/> is "Config", the method
        /// checks if the "FORUM_IMAGES" table exists and passes the result to the view. </description> </item> <item>
        /// <description> If <paramref name="template"/> is "Groups", the method retrieves all album groups from the
        /// database and passes them to the view. </description> </item> <item> <description> If <paramref
        /// name="template"/> is "Categories", the method retrieves album categories associated with the specified
        /// <paramref name="memberid"/> and passes them to the view. </description> </item> <item> <description> If
        /// <paramref name="template"/> is "Featured", the method selects a random featured album image that is not
        /// private or marked as "DoNotFeature", and passes it to the view. If no valid image is found, an empty view is
        /// returned. </description> </item> </list> If the <paramref name="template"/> does not match any of the
        /// supported values, an empty view is returned.</remarks>
        /// <param name="template">The name of the template to render. Supported values include "Config", "Groups", "Categories", and
        /// "Featured".</param>
        /// <param name="memberid">The ID of the member to filter data for, used when the template is "Categories". Defaults to 0.</param>
        /// <param name="info">A boolean value indicating whether to display additional information, used when the template is "Featured".
        /// Defaults to <see langword="true"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="IViewComponentResult"/> representing the rendered view.</returns>
        public async Task<IViewComponentResult> InvokeAsync(string template, int memberid = 0, bool info = true)
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
            if(template == "Categories")
            {
                var categories = _dbContext.Set<AlbumCategory>().Where(ac=>ac.MemberId == memberid).ToList();
                return await Task.FromResult((IViewComponentResult)View(template,categories));
            }
            if (template == "Featured")
            {
                ViewBag.ShowInfo = info;
                Random r = new Random();
                var idcount = _dbContext.Set<AlbumImage>().AsNoTracking().Where(a=> !a.DoNotFeature && !a.IsPrivate).Select(a=>a.Id).Count();

                int index = r.Next(0, idcount); // list.Count for List<T>

                var photo = _dbContext.Set<AlbumImage>().AsNoTracking().Include(a=>a.Member).Where(a=> !a.DoNotFeature && !a.IsPrivate).Skip(index).Take(1).SingleOrDefault();

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

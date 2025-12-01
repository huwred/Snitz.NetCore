using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Snitz.PhotoAlbum.Models;
using SnitzCore.Data.Interfaces;
using System.IO;

namespace Snitz.PhotoAlbum.Helpers
{
    /// <summary>
    /// A custom <see cref="TagHelper"/> that generates an HTML <c>&lt;img&gt;</c> element for displaying album images.
    /// </summary>
    /// <remarks>This tag helper is used to render an image with optional fallback behavior, lazy loading, and
    /// configurable dimensions. The <c>album-image</c> tag supports attributes for specifying the image id,
    /// fallback image, CSS class, description, and dimensions. If the specified image does not exist, a fallback
    /// image is used.</remarks>
    [HtmlTargetElement("album-image", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AlbumImageTagHelper : TagHelper
    {
        /// <summary>
        /// Fallback image to use when the primary image is unavailable.
        /// </summary>
        public string? Fallback { get; set; }

        [HtmlAttributeName("class")]
        public string? Class { get; set; }

        /// <summary>
        /// Identifier of the associated image.
        /// </summary>
        public int? ImageId {get;set;}

        /// <summary>
        /// ALt-Tag Description for the image.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Width of the displayed image. 
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Height of the displayed image.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Original width of the image, in pixels.
        /// </summary>
        public int? OrgWidth { get; set; }

        /// <summary>
        /// Original height of the image, in pixels.
        /// </summary>
        public int? OrgHeight { get; set; }

        private readonly ISnitzConfig _config;
        private readonly string contentfolder;
        private readonly PhotoContext _dbContext = null!;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumImageTagHelper"/> class,  which provides functionality for
        /// generating image tags for album photos.
        /// </summary>
        /// <param name="urlHelperFactory">The factory used to create <see cref="IUrlHelper"/> instances for generating URLs.</param>
        /// <param name="actionContextAccesor">Provides access to the current action context, which is used to retrieve routing and HTTP context
        /// information.</param>
        /// <param name="environment">The hosting environment, used to access environment-specific settings and file paths.</param>
        /// <param name="config">The application configuration settings, used to retrieve the content folder path and other related settings.</param>
        /// <param name="dbcontext">The database context for accessing photo-related data.</param>
        public AlbumImageTagHelper(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccesor,IWebHostEnvironment environment,ISnitzConfig config, PhotoContext dbcontext)
        {
            this.urlHelperFactory = urlHelperFactory;
            this.actionContextAccesor = actionContextAccesor;
            _env = environment;
            _config = config;
            contentfolder = _config.ContentFolder;
            _dbContext = dbcontext;
        }
        private readonly IWebHostEnvironment _env;
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly IActionContextAccessor actionContextAccesor;
        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string? Src = null;
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccesor.ActionContext!);

            if (ImageId != null)
            {
                var orgimage = _dbContext.Set<AlbumImage>().Find(ImageId);
                Src = orgimage?.ImageName;
                OrgWidth = orgimage?.Width;
                OrgHeight = orgimage?.Height;
                if(Description == null)
                {
                    Description = orgimage?.Description;
                }
                if (orgimage.IsPrivate)
                {
                    Description = "Private Image";
                }
            }
            var path = Path.Combine(_env.WebRootPath, contentfolder, "PhotoAlbum", Src ?? "");
            if (!File.Exists(path))
            {
                Src = null;
            }

            Fallback ??= urlHelper.Content("~/Content/notfound_lg.jpg");
            var imagevirt = urlHelper.Content($"~/{contentfolder}/PhotoAlbum/{Src}");
            base.Process(context, output);
            output.TagMode = TagMode.SelfClosing;
            output.TagName = "img";
            output.Attributes.Add("loading","lazy");
            output.Attributes.Add("class", Class /*+ " lazy"*/);
            var scaled = _config.GetValue("STRTHUMBTYPE") == "scaled";

            if (Width.HasValue && OrgWidth > 0)
            {
                if (OrgWidth < Width && OrgHeight < Height)
                {
                    output.Attributes.Add("width",$"{OrgWidth}");
                    output.Attributes.Add("src", Src == null ? Fallback : $"{imagevirt}?width={OrgWidth}&format=jpg");
                }
                else
                {
                    output.Attributes.Add("width",$"{Width}");
                    output.Attributes.Add("height",$"{Height}");
                    if (scaled)
                    {
                        output.Attributes.Add("src", Src == null ? Fallback : $"{imagevirt}?width={Width}&rmode=crop&format=jpg");
                    }
                    else
                    {
                        output.Attributes.Add("src", Src == null ? Fallback : $"{imagevirt}?width={Width}&height={Height}&rmode=crop&format=jpg");
                    }
                }
            }
            else
            {
                output.Attributes.Add("width",$"{Width}");
                output.Attributes.Add("height",$"{Height}");
                if (scaled)
                {
                    output.Attributes.Add("src", Src == null ? Fallback : $"{imagevirt}?width={Width}&rmode=crop&format=jpg");
                }
                else
                {
                    output.Attributes.Add("src", Src == null ? Fallback : $"{imagevirt}?width={Width}&height={Height}&rmode=crop&format=jpg");
                }
            }
            output.Attributes.Add("alt",Description ?? Src);
        }
    }
}

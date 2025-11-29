using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data.Interfaces;
using System.IO;

namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// A custom <see cref="TagHelper"/> that generates an HTML <c>&lt;img&gt;</c> element for displaying album images.
    /// </summary>
    /// <remarks>This tag helper is used to render an image with optional fallback behavior, lazy loading, and
    /// configurable dimensions. The <c>album-image</c> tag supports attributes for specifying the image source,
    /// fallback image, CSS class, description, and dimensions. If the specified image source does not exist, a fallback
    /// image is used.</remarks>
    [HtmlTargetElement("album-image", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AlbumImageTagHelper : TagHelper
    {
        [HtmlAttributeName("src")]
        public string? Src { get; set; }

        //public int? DataId {get;set;}
        public string? Fallback { get; set; }

        [HtmlAttributeName("class")]
        public string? Class { get; set; }

        public int? ImageId {get;set;}
        public string? Description { get; set; }

        public int? Width { get; set; }
        public int? OrgWidth { get; set; }
        public int? Height { get; set; }

        public int? OrgHeight { get; set; }

        private readonly ISnitzConfig _config;
        private readonly string contentfolder;
        public AlbumImageTagHelper(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccesor,IWebHostEnvironment environment,ISnitzConfig config)
        {
            this.urlHelperFactory = urlHelperFactory;
            this.actionContextAccesor = actionContextAccesor;
            _env = environment;
            _config = config;
            contentfolder = _config.ContentFolder;
        }
        private readonly IWebHostEnvironment _env;
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly IActionContextAccessor actionContextAccesor;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if(ImageId != null)
            {
                //var orgimage = _dbContext.Set<AlbumImage>().Find(id);
            }
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccesor.ActionContext!);
            var path = Path.Combine(_env.WebRootPath, contentfolder, "PhotoAlbum", Src ?? "");
            if (!File.Exists(path))
            {
                Src = null;
            }

            Fallback ??= urlHelper.Content("~/Content/notfound_lg.jpg");
            var imagevirt = urlHelper.Content($"~/{contentfolder}/PhotoAlbum/{Src}");
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "img";
            output.Attributes.Add("loading","lazy");
            output.Attributes.Add("class", Class /*+ " lazy"*/);
            var scaled = _config.GetValue("STRTHUMBTYPE") == "scaled";
            //TODO: Config.GetValue("STRTHUMBTYPE") == "scaled" or "cropped??
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
            output.Attributes.Add("alt",Description);

        }
    }
}

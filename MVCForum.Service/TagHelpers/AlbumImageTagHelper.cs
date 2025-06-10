using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("album-image", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AlbumImageTagHelper : TagHelper
    {
        [HtmlAttributeName("src")]
        public string? Src { get; set; }

        public string? Fallback { get; set; }

        [HtmlAttributeName("class")]
        public string? Class { get; set; }

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
            var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccesor.ActionContext!);
            if(!File.Exists(_env.WebRootPath + $@"\{contentfolder}\PhotoAlbum\{Src}"))
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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SnitzCore.BackOffice.TagHelpers
{
    [HtmlTargetElement("rank-image", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RankImageTagHelper : TagHelper
    {
        [HtmlAttributeName("config-key")]
        public string? Key { get; set; }
        [HtmlAttributeName("config-val")]
        public string? Value { get; set; }
        [ViewContext]
        public ViewContext? ViewContext { set; get; }
        private readonly IWebHostEnvironment _environment;
        public RankImageTagHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //var physicalPath = _contextAccessor.Current.Server.MapPath("~/Content/rankimages");
            var files = Directory.GetFiles(Path.Combine(_environment.WebRootPath, "Content/rankimages/"), "*.gif");
            
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            foreach (var file in files)
            {
                
                var imagefile = Path.GetFileName(file);
                string selected = Value != null && imagefile.Contains(Value) ? "selected" : "";
                output.Content.AppendHtml($@"<img data-id=""rankImage_{Key}"" data-val=""{imagefile}"" src=""{ "/Content/rankimages/" + imagefile}"" title=""{imagefile}"" class=""rank {selected} rank-image rankImage_{Key}"" />");

            }

        }

    }
}
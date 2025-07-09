using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Interfaces;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SnitzCore.BackOffice.TagHelpers
{
    [HtmlTargetElement("rank-image", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RankImageTagHelper : TagHelper
    {
        [HtmlAttributeName("config-key")]
        public string? Key { get; set; }
        [HtmlAttributeName("config-val")]
        public string? Value { get; set; }

        public int? Repeat {get;set;}

        [ViewContext]
        public ViewContext? ViewContext { set; get; }
        private readonly IWebHostEnvironment _environment;
        private readonly ISnitzConfig _config;
        public RankImageTagHelper(IWebHostEnvironment environment,ISnitzConfig config)
        {
            _environment = environment;
            _config = config;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;   
            
            if (Value != null && Value.Contains("."))
            {
                var files = Directory.GetFiles(Path.Combine(_environment.WebRootPath, "Content\\rankimages\\"), "*.gif");

                foreach (var file in files)
                {
                
                    var imagefile = Path.GetFileName(file);
                    string selected = Value != null && imagefile.Contains(Value) ? "selected" : "";
                    output.Content.AppendHtml($@"<img data-id=""rankImage_{Key}"" data-val=""{imagefile}"" src=""{_config.RootFolder}/Content/rankimages/{imagefile}"" title=""{imagefile}"" class=""rank {selected} rank-image rankImage_{Key}"" />");

                }
            }
            else
            {
                for (int i = 0; i < Repeat; i++)
                {
                    output.Content.AppendHtml($@"<i class=""fa fa-star {Value}""></i>");
                }
            }


        }

    }
}
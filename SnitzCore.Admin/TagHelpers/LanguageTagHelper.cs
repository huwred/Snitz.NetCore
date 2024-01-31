using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using SnitzCore.Data;

namespace SnitzCore.BackOffice.TagHelpers
{
    [HtmlTargetElement("lang-res", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class LanguageTagHelper : TagHelper
    {
        public LanguageTagHelper(SnitzDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HtmlAttributeName("culture")]
        public string? Lang { get; set; }
        [HtmlAttributeName("resource")]
        public string? Resource { get; set; }

        private readonly SnitzDbContext _dbContext;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var test = _dbContext.LanguageResources.Where(r => r.Name == Resource && r.Culture == Lang);
            var tagid = Guid.NewGuid().ToString();
            output.TagName = "input";
            output.TagMode = TagMode.SelfClosing;
            output.Attributes.Add("id",tagid);
            output.Attributes.Add("data-id",Resource);
            output.Attributes.Add("data-lang",Lang);
            output.AddClass("form-control",HtmlEncoder.Default);
            
            if (test.Any())
            {
                foreach (var resource in test)
                {
                    output.Attributes.SetAttribute("value", resource.Value);
                    output.Content.AppendHtml(resource.Culture);
                }
            }

        }

    }
}

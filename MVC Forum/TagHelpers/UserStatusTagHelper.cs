using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("snitz-status-icon", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class UserStatusTagHelper : TagHelper
    {
        [HtmlAttributeName("status")]
        public string? Status { get; set; }
        [HtmlAttributeName("title")]
        public string? Title { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "i";

            output.AddClass("fa",HtmlEncoder.Default);
            output.AddClass(Status == "1" ? "fa-user" : "fa-lock", HtmlEncoder.Default);
            if (Title != null && Title.StartsWith("Zapped"))
            {
                output.AddClass("fa-bolt",HtmlEncoder.Default);
                
            }
            output.Attributes.Add("title",Title);
            output.AddClass("center",HtmlEncoder.Default);
        }
    }
}
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("snitz-forum-icon", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ForumImageTagHelper : TagHelper
    {
        [HtmlAttributeName("forumtype")]
        public string? Type { get; set; }
        [HtmlAttributeName("accesstype")]
        public string? Access { get; set; }

        public int Status { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "i";

            output.AddClass("fa",HtmlEncoder.Default);
            if (Status == 0)
            {
                output.AddClass("fa-lock",HtmlEncoder.Default);
            }
            else if (Type == "1")
            {
                output.AddClass("fa-link",HtmlEncoder.Default);
            }
            else if (Access != "0")
            {
                output.AddClass("fa-ban",HtmlEncoder.Default);
            }
            else
            {
                output.AddClass("fa-folder",HtmlEncoder.Default);
            }


            output.AddClass("center",HtmlEncoder.Default);
        }
    }
}

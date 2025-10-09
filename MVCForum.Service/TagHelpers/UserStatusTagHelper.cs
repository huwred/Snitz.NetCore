using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// A custom <see cref="TagHelper"/> that renders a Font Awesome icon representing a user's status.
    /// </summary>
    /// <remarks>This tag helper generates an <c>&lt;i&gt;</c> HTML element with appropriate CSS classes based
    /// on the user's status. The <c>status</c> attribute determines whether the icon represents an active or locked
    /// user, while the <c>title</c> attribute provides additional context as a tooltip. If the <c>title</c> starts with
    /// "zapped" (case-insensitive), a lightning bolt icon is used instead.</remarks>
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

            output.AddClass("fa", HtmlEncoder.Default);
            if (Title != null && Title.ToLowerInvariant().StartsWith("zapped"))
            {
                output.AddClass("fa-bolt", HtmlEncoder.Default);

            }
            else
            {
                output.AddClass(Status == "1" ? "fa-user" : "fa-user-lock", HtmlEncoder.Default);

            }
            output.Attributes.Add("title", Title);
            output.AddClass("center", HtmlEncoder.Default);
        }
    }
}
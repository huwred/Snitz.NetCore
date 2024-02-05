using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SnitzCore.BackOffice.TagHelpers
{
    [HtmlTargetElement("link-confirm", TagStructure = TagStructure.NormalOrSelfClosing)]

    public class ActionLinkConfirmTagHelper : TagHelper
    {
        [HtmlAttributeName("config-key")]
        public string? Key { get; set; }
        [HtmlAttributeName("config-class")]
        public string? TagClass { get; set; }
        [HtmlAttributeName("jq-selector")]
        public string? Selector { get; set; }
        [ViewContext]
        public ViewContext? ViewContext { set; get; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Selector ??= "confirm-delete";
            var tagid = Guid.NewGuid().ToString();
            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("id",tagid);
            //output.Attributes.Add("href","#");
            output.Attributes.Add("rel","nofollow");
            //output.Attributes.Add("title","Delete Item");
            output.Attributes.Add("data-bs-toggle","modal");
            output.Attributes.Add("data-id",Key);
            output.AddClass(Selector,HtmlEncoder.Default);
            output.Content.AppendHtml($@"<i class='" + TagClass + "'></i>");
        }

    }

}
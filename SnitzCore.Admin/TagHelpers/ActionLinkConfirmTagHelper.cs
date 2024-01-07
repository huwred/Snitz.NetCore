using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace SnitzCore.BackOffice.TagHelpers
{
    [HtmlTargetElement("link-confirm", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ActionLinkConfirmTagHelper : TagHelper
    {
        [HtmlAttributeName("config-key")]
        public string Key { get; set; }
        [HtmlAttributeName("config-class")]
        public string TagClass { get; set; }
        [ViewContext]
        public ViewContext ViewContext { set; get; }

        public ActionLinkConfirmTagHelper()
        {

        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var test = context.Items;
            var tagid = Guid.NewGuid().ToString();
            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("id",tagid);
            //output.Attributes.Add("href","#");
            output.Attributes.Add("rel","nofollow");
            //output.Attributes.Add("title","Delete Item");
            output.Attributes.Add("data-toggle","modal");
            output.Attributes.Add("data-id",Key);
            output.AddClass("confirm-delete",HtmlEncoder.Default);
            output.Content.AppendHtml($@"<i class='" + TagClass + "'></i>");
        }

    }
}
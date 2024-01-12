using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("button-confirm", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ButtonConfirmTagHelper : TagHelper
    {
        [HtmlAttributeName("config-key")]
        public string Key { get; set; }
        [HtmlAttributeName("title")]
        public string Title { get; set; }
        [HtmlAttributeName("config-class")]
        public string TagClass { get; set; }
        [HtmlAttributeName("href")]
        public string Href { get; set; }
        [ViewContext]
        public ViewContext ViewContext { set; get; }

        public ButtonConfirmTagHelper()
        {

        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var test = context.Items;
            var tagid = Guid.NewGuid().ToString();
            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("href",Href + Key);
            output.Attributes.Add("id",tagid);
            output.Attributes.Add("data-toggle","modal");
            output.Attributes.Add("data-id",Key);
            output.Attributes.Add("rel","nofollow");
            output.AddClass("btn",HtmlEncoder.Default);
            output.AddClass("btn-outline-danger",HtmlEncoder.Default);
            output.AddClass("confirm-delete",HtmlEncoder.Default);
            output.Content.AppendHtml($@"<i class='" + TagClass + $"'></i><span class='d-none d-md-inline'> {Title}</span>");
        }

    }
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
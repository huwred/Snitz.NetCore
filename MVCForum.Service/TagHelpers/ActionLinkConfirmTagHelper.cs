using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Service.TagHelpers
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
        public string? Title { get; set; }
        public string? Class { get; set; }
        public string? Href { get; set; }
        private readonly ISnitzConfig _snitzConfig;

        public ActionLinkConfirmTagHelper(ISnitzConfig snitzconfig)
        {
            _snitzConfig = snitzconfig;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Selector ??= "confirm-delete";
            var tagid = Guid.NewGuid().ToString();
            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("id",tagid);
            output.Attributes.Add("title",Title);
            output.Attributes.Add("rel","noopener noreferrer nofollow");
            output.Attributes.Add("href",Href!.Replace("~",_snitzConfig.RootFolder));
            //output.Attributes.Add("title","Delete Item");
            //output.Attributes.Add("data-bs-toggle","modal");
            if (Key != null)
            {
                output.Attributes.Add("data-id",Key);
            }
            
            if (TagClass != null)
            {
                output.Content.AppendHtml($@"<i class='" + TagClass + "'></i>");
                output.AddClass(Selector,HtmlEncoder.Default);
            }
            else
            {
                output.Content.AppendHtml(Title);
                output.Attributes.Add("class", Class);
                output.AddClass(Selector,HtmlEncoder.Default);
            }
        }

    }
}
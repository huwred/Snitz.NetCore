using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("topic-buttons", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TopicButtonsTagHelper : TagHelper
    {

        [HtmlAttributeName("topic-print")]
        public int PrintTopic { get; set; }
        [HtmlAttributeName("topic-email")]
        public int SendTopic { get; set; }
        [HtmlAttributeName("topic-share")]
        public int SocialMedia { get; set; }
        public Func<string, string>? TextLocalizerDelegate { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";

            //var tagSpan = new TagBuilder("span");
            if (PrintTopic > 0)
            {
                var tagButton = new TagBuilder("button");
                tagButton.Attributes.Add("id", "PrintTopic");
                tagButton.Attributes.Add("class", "btn btn-light");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", PrintTopic.ToString());
                tagButton.Attributes.Add("data-href", "/Topic/Print");
                if (TextLocalizerDelegate != null) tagButton.Attributes.Add("title", TextLocalizerDelegate("tipPrintTopic"));
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-print p-2\"></i>");
                output.Content.AppendHtml(tagButton);
            }
            if (SendTopic > 0)
            {
                var tagButton = new TagBuilder("button");
                tagButton.Attributes.Add("id", "SendTopic");
                tagButton.Attributes.Add("class", "btn btn-light");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", SendTopic.ToString());
                tagButton.Attributes.Add("data-href", "/Topic/Print");
                if (TextLocalizerDelegate != null) tagButton.Attributes.Add("title", TextLocalizerDelegate("tipSendTopic"));
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-envelope p-2\"></i>");
                output.Content.AppendHtml(tagButton);
            }
            if (SocialMedia > 0)
            {
                var tagButton = new TagBuilder("button");
                tagButton.Attributes.Add("id", "ShareTopic");
                tagButton.Attributes.Add("class", "btn btn-light");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", SocialMedia.ToString());
                tagButton.Attributes.Add("data-href", "/Topic/Print");
                if (TextLocalizerDelegate != null) tagButton.Attributes.Add("title", TextLocalizerDelegate("SocialShare"));
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-share-alt p-2\"></i>");
                output.Content.AppendHtml(tagButton);
            }

            output.AddClass("btn-group",HtmlEncoder.Default);
            //output.PreElement.AppendHtml(@"<div class='btn-group' role='group'>");
            //output.PreElement.AppendHtml(tagSpan);
            //output.PostElement.AppendHtml("</div>");

        }
    }
}

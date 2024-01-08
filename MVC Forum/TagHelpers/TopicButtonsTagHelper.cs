using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("topic-buttons", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TopicButtonsTagHelper : TagHelper
    {
        private readonly IHtmlGenerator _generator;
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        [HtmlAttributeName("topic-print")]
        public int PrintTopic { get; set; }
        [HtmlAttributeName("topic-email")]
        public int SendTopic { get; set; }
        [HtmlAttributeName("topic-share")]
        public int SocialMedia { get; set; }
        public TopicButtonsTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var tagSpan = new TagBuilder("span");
            if (PrintTopic > 0)
            {
                var tagButton = new TagBuilder("button");
                tagButton.Attributes.Add("id", "PrintTopic");
                tagButton.Attributes.Add("class", "btn btn-light");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", "button");
                tagButton.Attributes.Add("data-href", "/Topic/Print");
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-print fa-2x p-2\"></i>");
                tagSpan.InnerHtml.AppendHtml(tagButton);

            }
            if (SendTopic > 0)
            {
                var tagButton = new TagBuilder("button");
                tagButton.Attributes.Add("id", "SendTopic");
                tagButton.Attributes.Add("class", "btn btn-light");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", "button");
                tagButton.Attributes.Add("data-href", "/Topic/Print");
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-envelope fa-2x p-2\"></i>");
                tagSpan.InnerHtml.AppendHtml(tagButton);

            }
            if (SocialMedia > 0)
            {
                var tagButton = new TagBuilder("button");
                tagButton.Attributes.Add("id", "ShareTopic");
                tagButton.Attributes.Add("class", "btn btn-light");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", "button");
                tagButton.Attributes.Add("data-href", "/Topic/Print");
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-twitter fa-2x p-2\"></i>");
                tagSpan.InnerHtml.AppendHtml(tagButton);

            }

            output.PreElement.AppendHtml(@"<div class='button-group'>");
            output.PreElement.AppendHtml(tagSpan);
            output.PostElement.AppendHtml("</div>");

        }
    }
}

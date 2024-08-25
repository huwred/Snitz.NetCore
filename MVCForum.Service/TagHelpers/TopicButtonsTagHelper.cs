using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using SnitzCore.Data.Interfaces;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("topic-buttons", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TopicButtonsTagHelper : TagHelper
    {
        private readonly ISnitzConfig _config;
        private readonly IUrlHelperFactory _urlHelper;

        public TopicButtonsTagHelper(ISnitzConfig config, IUrlHelperFactory urlHelper)
        {
            _config = config;
            _urlHelper = urlHelper;
        }
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("topic-print")]
        public int PrintTopic { get; set; }
        [HtmlAttributeName("topic-email")]
        public int SendTopic { get; set; }
        [HtmlAttributeName("topic-share")]
        public int SocialMedia { get; set; }
        public string? Class { get; set; }
        public Func<string, string>? TextLocalizerDelegate { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";

            //var tagSpan = new TagBuilder("span");
            if (PrintTopic > 0)
            {
                var tagButton = new TagBuilder("a");
                tagButton.Attributes.Add("id", "print-topic");
                tagButton.Attributes.Add("class", "btn btn-outline-primary");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", PrintTopic.ToString());
                tagButton.Attributes.Add("href", "~/Topic/Print/" + PrintTopic.ToString());
                tagButton.Attributes.Add("rel", "noopener nofollow");
                tagButton.Attributes.Add("target", "_blank");
                if (TextLocalizerDelegate != null) tagButton.Attributes.Add("title", TextLocalizerDelegate("tipPrintTopic"));
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-print p-2\"></i>");
                output.Content.AppendHtml(tagButton);
            }
            if (SendTopic > 0)
            {
                var tagButton = new TagBuilder("button");
                tagButton.Attributes.Add("id", "SendTopic");
                tagButton.Attributes.Add("class", "btn btn-outline-primary sendto-link");
                tagButton.Attributes.Add("type", "button");
                tagButton.Attributes.Add("data-id", SendTopic.ToString());
                tagButton.Attributes.Add("data-href", "~/Topic/SendTo");
                if (TextLocalizerDelegate != null) tagButton.Attributes.Add("title", TextLocalizerDelegate("tipSendTopic"));
                tagButton.InnerHtml.AppendHtml("<i class=\"fa fa-envelope p-2\"></i>");
                output.Content.AppendHtml(tagButton);
            }
            if (SocialMedia > 0)
            {
                var forumTitle = _config.ForumTitle;
                var pageUrl = ViewContext.HttpContext.Request.GetEncodedUrl();

                var fbButton = new TagBuilder("a");
                fbButton.Attributes.Add("id", "ShareFacebook");
                fbButton.Attributes.Add("class", Class ?? "btn btn-outline-primary ");
                fbButton.Attributes.Add("data-id", SocialMedia.ToString());
                fbButton.Attributes.Add("href", $"https://www.facebook.com/share.php?u={pageUrl}&title={forumTitle}");
                fbButton.Attributes.Add("rel", "noopener nofollow");
                fbButton.Attributes.Add("target", "_blank");
                if (TextLocalizerDelegate != null) fbButton.Attributes.Add("title", TextLocalizerDelegate("FacebookShare"));
                fbButton.InnerHtml.AppendHtml("<i class=\"fa fa-facebook-square p-2\"></i>");
                output.Content.AppendHtml(fbButton);

                var twButton = new TagBuilder("a");
                twButton.Attributes.Add("id", "ShareTwitter");
                twButton.Attributes.Add("class", Class ?? "btn btn-outline-primary ");
                //twButton.Attributes.Add("type", "button");
                twButton.Attributes.Add("data-id", SocialMedia.ToString());
                twButton.Attributes.Add("href", $"https://twitter.com/home?status={forumTitle}+{pageUrl}");
                twButton.Attributes.Add("rel", "noopener nofollow");
                twButton.Attributes.Add("target", "_blank");
                if (TextLocalizerDelegate != null) twButton.Attributes.Add("title", TextLocalizerDelegate("TwitterShare"));
                twButton.InnerHtml.AppendHtml("<i class=\"fa fa-twitter-square p-2\"></i>");
                output.Content.AppendHtml(twButton);

                //var disButton = new TagBuilder("button");
                //disButton.Attributes.Add("id", "ShareTopic");
                //disButton.Attributes.Add("class", "btn btn-light");
                //disButton.Attributes.Add("type", "button");
                //disButton.Attributes.Add("data-id", SocialMedia.ToString());
                //disButton.Attributes.Add("data-href", "/Topic/Print");
                //if (TextLocalizerDelegate != null) disButton.Attributes.Add("title", TextLocalizerDelegate("DiscordShare"));
                //disButton.InnerHtml.AppendHtml("<i class=\"fab fa-discord p-2\"></i>");
                //output.Content.AppendHtml(disButton);
            }

            output.AddClass("btn-group", HtmlEncoder.Default);

        }
    }
}

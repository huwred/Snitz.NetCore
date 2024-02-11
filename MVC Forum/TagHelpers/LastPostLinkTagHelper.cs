using System;
using System.Collections.Generic;
using MailKit;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MVCForum.Extensions;
using SnitzCore.Service.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("lastpost-link", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class LastPostLinkTagHelper : TagHelper
    {
        public int TopicId { get; set; }
        public int? ReplyId { get; set; }
        public DateTime? PostDate { get; set; }
        public Func<string, string>? TextLocalizerDelegate { get; set; }

        public LastPostLinkTagHelper() {}

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var link = new TagBuilder("a");

            link.Attributes.Add("rel","index,follow");
            link.Attributes.Add("href", $"/Topic/{TopicId}/?replyid={ReplyId}");
            link.Attributes.Add("title",TextLocalizerDelegate("tipLastPost"));
            link.InnerHtml.AppendHtml(@"<i class=""fa fa-arrow-right""></i>");

            output.TagName = "span";
            output.Content.AppendHtml($@"<time datetime=""{PostDate?.ToLocalTime().ToTimeagoDate()}"" class=""timeago"" aria-label=""Posted on {PostDate?.ToLocalTime()}"">{PostDate?.ToLocalTime().ToForumDisplay()}</time>&nbsp;");
            output.Content.AppendHtml(link);

        }

    }
}

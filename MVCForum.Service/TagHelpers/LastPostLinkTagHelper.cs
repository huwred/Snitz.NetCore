using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Service.Extensions;

namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// TagHelper to innsert a link to the last post in a topic]]>
    /// </summary>
    [HtmlTargetElement("lastpost-link", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class LastPostLinkTagHelper : TagHelper
    {
        private string? webrootpath;
        public int TopicId { get; set; }
        /// <summary>
        /// ID of last post
        /// </summary>
        public int? ReplyId { get; set; }
        public bool? JumpTo { get; set; } = false;
        /// <summary>
        /// Date of the last post
        /// </summary>
        public DateTime? PostDate { get; set; }

        public bool Archived {get;set;} = false;
        /// <summary>
        /// Delegate function for language translation, should be set as
        /// delegate(string s) { return Localizer[s].Value; }
        /// </summary>
        public Func<string, string>? TextLocalizerDelegate { get; set; }

        public LastPostLinkTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            webrootpath = httpContextAccessor.HttpContext?.Request.PathBase;

        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (webrootpath == null || webrootpath == "/") { webrootpath = ""; }
            var link = new TagBuilder("a");

            link.Attributes.Add("rel", "index,follow");
            link.Attributes.Add("data-toggle", "tooltip");
            if (Archived)
            {
                link.Attributes.Add("href", $"{webrootpath}/Topic/Archived/{TopicId}/?replyid={ReplyId}");
            }
            else
            {
                link.Attributes.Add("href", $"{webrootpath}/Topic/{TopicId}/?replyid={ReplyId}");
            }
            
            if (TextLocalizerDelegate != null) link.Attributes.Add("title", TextLocalizerDelegate("tipLastPost"));
            link.InnerHtml.AppendHtml(@"<i class=""fa fa-arrow-right""></i>");

            output.TagName = "span";
            output.Content.AppendHtml($@"<time data-toggle=""tooltip"" datetime=""{PostDate?.ToTimeagoDate()}"" class=""timeago"" aria-label=""Posted on {PostDate?.ToLocalTime()}"">{PostDate?.ToLocalTime().ToForumDisplay()}</time>&nbsp;");
            if ((bool)JumpTo!)
            {
                output.Content.AppendHtml(link);
            }

        }

    }
}

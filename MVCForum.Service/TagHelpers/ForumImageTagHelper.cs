using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Text.Encodings.Web;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("snitz-forum-icon", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ForumImageTagHelper : TagHelper
    {
        private readonly ISnitzCookie _cookie;
        public ForumImageTagHelper(ISnitzCookie snitzCookie)
        {
            _cookie = snitzCookie;
        }
        [HtmlAttributeName("lastpost")]
        public DateTime? LastPost { get; set; }
        [HtmlAttributeName("forumtype")]
        public string? Type { get; set; }
        [HtmlAttributeName("accesstype")]
        public string? Access { get; set; }

        public int Status { get; set; }

        public int? ForumId { get; set; }

        public bool IsAdministrator { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            bool newclass = false;
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "i";
            if (LastPost.HasValue)
            {
                var lasthere = _cookie.GetLastVisitDate()?.FromForumDateStr();
                if (lasthere.HasValue)
                {
                    if (LastPost > lasthere)
                    {
                        newclass = true;
                    }
                }
            }
            output.AddClass("fa", HtmlEncoder.Default);
            if (Status == 0)
            {
                output.AddClass("fa-lock", HtmlEncoder.Default);
                output.Attributes.Add("title", "Locked Forum");
            }
            else if (Type == "1")
            {
                output.AddClass("fa-link", HtmlEncoder.Default);
                if (ForumId != null)
                {
                    if (IsAdministrator && Type == ((int)ForumType.WebLink).ToString())
                    {
                        output.AddClass("weblink-edit", HtmlEncoder.Default);
                        output.Attributes.Add("title", "Weblink");
                        output.Attributes.Add("data-id", ForumId);
                    }
                }
            }
            else if (Access != "0")
            {
                output.AddClass("fa-ban", HtmlEncoder.Default);
            }
            else
            {
                output.AddClass("fa-folder", HtmlEncoder.Default);
            }

            if ((newclass))
            {
                output.AddClass("newposts", HtmlEncoder.Default);
                output.Attributes.Add("title", "Contains new posts");
            }
            output.AddClass("center", HtmlEncoder.Default);
        }
    }
}

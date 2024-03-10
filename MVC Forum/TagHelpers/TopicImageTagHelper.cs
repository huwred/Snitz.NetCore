using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using System;
using System.Text.Encodings.Web;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("snitz-topic-icon", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TopicImageTagHelper : TagHelper
    {
        private readonly ISnitzConfig _config;
        private readonly ISnitzCookie _cookie;

        public TopicImageTagHelper(ISnitzConfig snitzConfig,ISnitzCookie snitzCookie)
        {
            _config = snitzConfig;
            _cookie = snitzCookie;
        }

        [HtmlAttributeName("status")]
        public string? Status { get; set; }

        [HtmlAttributeName("sticky")]
        public bool Sticky { get; set; }

        [HtmlAttributeName("replies")]
        public int Replies { get; set; }

        [HtmlAttributeName("views")]
        public string? Views { get; set; }
        [HtmlAttributeName("lastpost")]
        public DateTime? LastPost { get; set; }
        public bool Answered { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "i";

            output.AddClass("fa",HtmlEncoder.Default);
            if (Sticky && _config.GetIntValue("STRSTICKYTOPIC") == 1)
            {
                output.Attributes.Add("title","Sticky Topic");
                output.AddClass("fa-thumb-tack",HtmlEncoder.Default);
                output.AddClass("center",HtmlEncoder.Default);
                return;
            }
            if (Status == "1")
            {
                output.Attributes.Add("title","Locked");
                output.AddClass("fa-lock",HtmlEncoder.Default);
            }
            else if (Replies == 0)
            {
                output.Attributes.Add("title","No replies");
                output.AddClass("fa-frown-o",HtmlEncoder.Default);
            }
            else if (Replies > 100)
            {
                output.Attributes.Add("title","Super charged Topic");
                output.AddClass("fa-rocket",HtmlEncoder.Default);
            }
            else if (Replies > _config.GetIntValue("INTHOTTOPICNUM",25))
            {
                output.Attributes.Add("title","Hot Topic");
                output.AddClass("fa-fire",HtmlEncoder.Default);
            }
            else if (Answered)
            {
                output.Attributes.Add("title","Topic answered");
                output.AddClass("fa-folder",HtmlEncoder.Default);
            }
            else 
            {
                output.Attributes.Add("title","No new posts");
                output.AddClass("fa-folder-o",HtmlEncoder.Default);
            }

            if (LastPost.HasValue)
            {
                var lasthere = _cookie.GetLastVisitDate()?.FromForumDateStr();
                if (lasthere.HasValue)
                {
                    if (LastPost > lasthere)
                    {
                        if (Status == "1")
                        {
                            output.Attributes.Add("title","Locked, Contains new posts");
                            output.AddClass("fa-lock",HtmlEncoder.Default);
                        }
                        else
                        {
                            output.AddClass("new-posts",HtmlEncoder.Default);
                            output.Attributes.Add("title","Contains new posts");
                        }
                        
                    }
                }
            }
            output.AddClass("center",HtmlEncoder.Default);
        }
    }
}

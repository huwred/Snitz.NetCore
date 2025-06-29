using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("snitz-topic-icon", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TopicImageTagHelper : TagHelper
    {
        private readonly ISnitzConfig _config;
        private readonly ISnitzCookie _cookie;

        public TopicImageTagHelper(ISnitzConfig snitzConfig, ISnitzCookie snitzCookie)
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

        [HtmlAttributeName("plugin-icon")]
        public string? PluginIcon {get; set;}
        /// <summary>
        ///             <span class="fa-stack fa-1x">
        ///             <i class="fa fa-folder-o fa-stack-2x"></i>
        ///             //<i class="fa fa-lock fa-stack-1x "></i>
        ///             </span>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string locked = Status == "0" ? ", Locked" : "";
            string newposts = "";
            bool newclass = false;
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "span";
            output.AddClass("fa-stack", HtmlEncoder.Default);
            output.AddClass("fa-1x", HtmlEncoder.Default);

            TagHelperOutput mainTag = new TagHelperOutput(
                tagName: "i",

                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (s, t) =>
                {
                    return Task.Factory.StartNew<TagHelperContent>(() => new DefaultTagHelperContent());
                }
            );
            mainTag.TagMode = TagMode.StartTagAndEndTag;

            TagHelperOutput overlayTag = new TagHelperOutput(
                tagName: "i",

                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (s, t) =>
                {
                    return Task.Factory.StartNew<TagHelperContent>(() => new DefaultTagHelperContent());
                }
            );
            overlayTag.TagMode = TagMode.StartTagAndEndTag;

            if (LastPost.HasValue)
            {
                var lasthere = _cookie.GetLastVisitDate()?.FromForumDateStr();
                if (lasthere.HasValue)
                {
                    if (LastPost > lasthere)
                    {
                        newposts = ", Contains new posts";
                        newclass = true;
                    }
                }
            }
            if(PluginIcon != null)
            {
                output.Attributes.Add("title", $"{locked}{newposts}");
                mainTag.AddClass("fa", HtmlEncoder.Default);
                mainTag.AddClass(PluginIcon, HtmlEncoder.Default);
                mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
            }
            else if (newclass)
            {
                output.Attributes.Add("title", $"{locked}{newposts}");
                mainTag.AddClass("fa", HtmlEncoder.Default);
                mainTag.AddClass("fa-folder-o", HtmlEncoder.Default);
                mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
            }
            else if (Answered)
            {
                output.Attributes.Add("title", $"Topic answered{locked}{newposts}");
                mainTag.AddClass("fa", HtmlEncoder.Default);
                mainTag.AddClass("fa-folder", HtmlEncoder.Default);
                mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
            }
            else
            {
                mainTag.AddClass("fa", HtmlEncoder.Default);
                mainTag.AddClass("fa-folder-o", HtmlEncoder.Default);
                mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
                output.Attributes.Add("title", $"No new posts{locked}");
            }
            if (Sticky && _config.GetIntValue("STRSTICKYTOPIC") == 1)
            {
                output.Attributes.Add("title", $"Sticky Topic{locked}{newposts}");
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-thumb-tack", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Replies == 0)
            {
                output.Attributes.Add("title", $"No replies{locked}{newposts}");
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-frown-o", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Replies > 100)
            {
                output.Attributes.Add("title", $"Super charged Topic{locked}{newposts}");
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-rocket", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Replies > _config.GetIntValue("INTHOTTOPICNUM", 25))
            {
                output.Attributes.Add("title", $"Hot Topic{locked}{newposts}");
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-fire", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Status == "0")
            {
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-lock", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }

            if (newclass) { mainTag.AddClass("newposts", HtmlEncoder.Default); }

            output.Content.AppendHtml(mainTag);
            output.Content.AppendHtml(overlayTag);
        }
    }
}

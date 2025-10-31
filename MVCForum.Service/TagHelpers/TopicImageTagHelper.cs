using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service.Extensions;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// A custom <see cref="TagHelper"/> that generates an icon representing the status of a forum topic.
    /// </summary>
    /// <remarks>This tag helper is used to render a visual representation of a forum topic's state, such as
    /// whether it is locked, sticky, has new posts, or is a blog post. The generated output is a styled <c>span</c>
    /// element containing one or more <c>i</c> elements with appropriate Font Awesome classes.</remarks>
    [HtmlTargetElement("snitz-topic-icon", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TopicImageTagHelper : TagHelper
    {
        private readonly LanguageService  _languageResource;
        private readonly ISnitzConfig _config;
        private readonly ISnitzCookie _cookie;
        private readonly IActionContextAccessor _actionAccessor;
        private readonly SnitzCore.Data.IMember _memberService;
        public TopicImageTagHelper(ISnitzConfig snitzConfig, ISnitzCookie snitzCookie,IActionContextAccessor actionAccessor,SnitzCore.Data.IMember memberservice,IHtmlLocalizerFactory localizerFactory)
        {
            _config = snitzConfig;
            _cookie = snitzCookie;
            _actionAccessor = actionAccessor;
            _memberService = memberservice;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
        }

        [HtmlAttributeName("status")]
        public string? Status { get; set; }

        [HtmlAttributeName("sticky")]
        public bool Sticky { get; set; }

        public bool? Blog { get; set; }

        [HtmlAttributeName("replies")]
        public int Replies { get; set; }

        [HtmlAttributeName("views")]
        public string? Views { get; set; }
        [HtmlAttributeName("lastpost")]
        public DateTime? LastPost { get; set; }
        public bool Answered { get; set; }

        [HtmlAttributeName("plugin-icon")]
        public string? PluginIcon {get; set;}

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            string locked = Status == "0" ? "_locked" : "";
            string newposts = "_read";
            string? icon = "ico_topic";
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
                var user = _actionAccessor.ActionContext?.HttpContext?.User;
                var lasthere = _cookie.GetLastVisitDate()?.FromForumDateStr().LocalTime(_cookie);
                if (user != null && user.Identity!.IsAuthenticated)
                {
                    lasthere = _memberService.GetByUsername(user.Identity.Name!)!.LastLogin.FromForumDateStr().LocalTime(_cookie);
                }
                if (lasthere.HasValue)
                {
                    if (LastPost.Value > lasthere)
                    {
                        newposts = "_unread";
                        newclass = true;
                    }
                }
            }
            switch (Status)
            {
                case "2" :
                    output.Attributes.Add("title", _languageResource.GetString($"{icon}_unread_topic-unmoderated"));
                    mainTag.AddClass("fa", HtmlEncoder.Default);
                    mainTag.AddClass("fa-triangle-exclamation", HtmlEncoder.Default);
                    mainTag.AddClass("fa-2x", HtmlEncoder.Default);

                    output.Attributes.Add("data-toggle","tooltip");
                    output.Content.AppendHtml(mainTag);
                    return;
                case "3" :
                    output.Attributes.Add("title", _languageResource.GetString($"{icon}_unread_topic-onhold"));
                    mainTag.AddClass("fa-regular", HtmlEncoder.Default);
                    mainTag.AddClass("fa-circle-pause", HtmlEncoder.Default);
                    mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);

                    output.Attributes.Add("data-toggle","tooltip");
                    output.Content.AppendHtml(mainTag);
                    return;
                case "99" :
                    mainTag.AddClass("fa", HtmlEncoder.Default);
                    mainTag.AddClass("fa-circle-pause", HtmlEncoder.Default);
                    mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
                    output.Attributes.Add("title", _languageResource.GetString($"lblDraftPost"));
                    break;
                case "0" :
                    output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}"));
                    mainTag.AddClass("fa", HtmlEncoder.Default);
                    mainTag.AddClass("fa-folder-closed", HtmlEncoder.Default);
                    mainTag.AddClass("fa-2x", HtmlEncoder.Default);
                    output.Attributes.Add("data-toggle","tooltip");
                    break;

                default:
                    if(Blog != null && Blog == true)
                    {
                        output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}_blog"));
                        mainTag.AddClass("fa", HtmlEncoder.Default);
                        mainTag.AddClass("fa-blog", HtmlEncoder.Default);
                        mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
                    }
                    else if (PluginIcon != null)
                    {
                        output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}"));
                        mainTag.AddClass("fa", HtmlEncoder.Default);
                        mainTag.AddClass(PluginIcon, HtmlEncoder.Default);
                        mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
                    }
                    else if (newclass)
                    {
                        output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}"));
                        mainTag.AddClass("fa", HtmlEncoder.Default);
                        mainTag.AddClass("fa-folder-open", HtmlEncoder.Default);
                        mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
                    }
                    else if (Answered)
                    {
                        output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}_answered"));
                        mainTag.AddClass("fa", HtmlEncoder.Default);
                        mainTag.AddClass("fa-folder", HtmlEncoder.Default);
                        mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
                    }         
                    else
                    {
                        mainTag.AddClass("fa", HtmlEncoder.Default);
                        mainTag.AddClass("fa-folder", HtmlEncoder.Default);
                        mainTag.AddClass("fa-stack-2x", HtmlEncoder.Default);
                        output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}"));
                    }
                    break;
            }



            if (Sticky && _config.GetIntValue("STRSTICKYTOPIC") == 1)
            {
                icon = "ico_sticky";
                output.Attributes.RemoveAll("title");
                output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}") );
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-thumb-tack", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Replies == 0 && Blog != true)
            {
                output.Attributes.RemoveAll("title");
                output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}_noreplies"));
                overlayTag.AddClass("fa-regular", HtmlEncoder.Default);
                overlayTag.AddClass("fa-frown", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Replies > 100)
            {
                output.Attributes.RemoveAll("title");
                output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}_superhot"));
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-rocket", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Replies > _config.GetIntValue("INTHOTTOPICNUM", 25))
            {
                output.Attributes.RemoveAll("title");
                output.Attributes.Add("title",_languageResource.GetString($"{icon}{newposts}{locked}_hot"));
                overlayTag.AddClass("fa", HtmlEncoder.Default);
                overlayTag.AddClass("fa-fire", HtmlEncoder.Default);
                overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
            }
            else if (Status == "0")
            {
                output.Attributes.RemoveAll("title");
                //overlayTag.AddClass("fa", HtmlEncoder.Default);
                //overlayTag.AddClass("fa-lock", HtmlEncoder.Default);
                //overlayTag.AddClass("fa-stack-1x", HtmlEncoder.Default);
                output.Attributes.Add("title", _languageResource.GetString($"{icon}{newposts}{locked}"));
            }

            if (newclass) { mainTag.AddClass("newposts", HtmlEncoder.Default); }
            output.Attributes.Add("data-toggle","tooltip");
            output.Content.AppendHtml(mainTag);
            output.Content.AppendHtml(overlayTag);
        }
    }
}

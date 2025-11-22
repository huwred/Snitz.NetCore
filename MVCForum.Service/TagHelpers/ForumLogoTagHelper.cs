using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Text.Encodings.Web;

namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// A custom <see cref="TagHelper"/> that renders an icon representing the status and type of a forum.
    /// </summary>
    /// <remarks>This tag helper generates an HTML <c>&lt;i&gt;</c> element with appropriate CSS classes and
    /// attributes to visually represent the forum's state, type, and access level. It supports customization through
    /// various attributes, such as <c>lastpost</c>, <c>forumtype</c>, and <c>accesstype</c>.</remarks>
    [HtmlTargetElement("snitz-forum-logo", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ForumLogoTagHelper : TagHelper
    {
        private readonly ISnitzCookie _cookie;
        private readonly Data.IMember _memberService;
        private readonly IActionContextAccessor _actionAccessor;
        private readonly LanguageService  _languageResource;
        private readonly ISnitzConfig _config;
        private readonly string _currentTheme;

        public ForumLogoTagHelper(ISnitzConfig config,ISnitzCookie snitzCookie,Data.IMember memberservice,IActionContextAccessor actionAccessor,IHtmlLocalizerFactory localizerFactory)
        {
            _cookie = snitzCookie;
            _memberService = memberservice;
            _actionAccessor = actionAccessor;
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
            _currentTheme = snitzCookie.GetCookieValue("snitztheme") ?? "";
            _config = config;
        }

        public bool UseTheme { get; set; } = false;
        public string Logo { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.SelfClosing;
            output.TagName = "img";
            if (UseTheme && !string.IsNullOrWhiteSpace(_currentTheme))
            {
                output.Attributes.Add("src", $"{_config.RootFolder}/images/{_currentTheme}/{Logo}?height=50");

            }
            else
            {
                output.Attributes.Add("src", $"{_config.RootFolder}/images/{Logo}?height=50");
            }
            output.Attributes.Add("loading", "lazy");
            output.Attributes.Add("height", "50");
            output.Attributes.Add("alt", _languageResource.GetString("ForumLogo"));
        }
    }
}

using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data;
using System;

namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// TagHelper to add link to Member's profile
    /// </summary>
    [HtmlTargetElement("member-link", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class MemberLinkTagHelper : TagHelper
    {
        private readonly IMember _member;
        private string? webrootpath;

        public MemberLinkTagHelper(IMember memberService, IHttpContextAccessor httpContextAccessor)
        {
            _member = memberService;
            webrootpath = httpContextAccessor.HttpContext?.Request.PathBase;
        }
        public int? MemberId { get; set; }
        public string? MemberName { get; set; }

        /// <summary>
        /// Delegate function for language translation, should be set as
        /// delegate(string s) { return Localizer[s].Value; }
        /// </summary>
        public Func<string, string>? TextLocalizerDelegate { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (webrootpath == null || webrootpath == "/") { webrootpath = ""; }
            output.TagName = "a";
            output.Attributes.Add("rel", "noopener noreferrer nofollow");
            output.Attributes.Add("data-toggle","tooltip");
            if (TextLocalizerDelegate != null) output.Attributes.Add("title", TextLocalizerDelegate("tipViewProfile"));
            if (MemberName != null)
            {
                output.Attributes.Add("href", $"{webrootpath}/Account/Detail/{Url.Encode(MemberName)}");
                output.Content.AppendHtml($"{MemberName}");
            }
            else
            {
                var member = _member.GetById(MemberId);
                if (member != null)
                {
                    output.Attributes.Add("href", $"{webrootpath}/Account/Detail/{Url.Encode(member.Name)}");
                    output.Content.AppendHtml($"{member.Name}");
                }
            }

        }
    }
}

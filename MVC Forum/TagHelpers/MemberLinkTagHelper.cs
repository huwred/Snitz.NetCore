using Flurl;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data;
using System;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("member-link", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class MemberLinkTagHelper :TagHelper
    {
        private readonly IMember _member;

        public MemberLinkTagHelper(IMember memberService)
        {
            _member = memberService;
        }
        public int? MemberId { get; set; }
        public string? MemberName { get; set; }
        public Func<string, string>? TextLocalizerDelegate { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            
            output.TagName = "a";
            output.Attributes.Add("rel","nofollow");
            if (TextLocalizerDelegate != null) output.Attributes.Add("title", TextLocalizerDelegate("tipViewProfile"));
            if (MemberName != null)
            {
                output.Attributes.Add("href", $"~/Account/Detail/{Url.Encode(MemberName)}");
                output.Content.AppendHtml($"{MemberName}");
            }
            else
            {
                var member = _member.GetById(MemberId);
                if (member != null)
                {
                    output.Attributes.Add("href", $"~/Account/Detail/{Url.Encode(MemberName)}");
                    output.Content.AppendHtml($"{member.Name}");
                }
            }

        }
    }
}

using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service.Extensions;
using System;
using System.Text.Encodings.Web;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("snitz-datetime", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SnitzDateTimeDisplay : TagHelper
    {
        [HtmlAttributeName("datetime")]
        public DateTime? SnitzDate { get; set; }

        public bool? FreindlyTime { get; set; } = true;

        [HtmlAttributeName("format")]
        public string? Format { get; set; }

        private readonly LanguageService  _languageResource;
        public SnitzDateTimeDisplay(IHtmlLocalizerFactory localizerFactory )
        {
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
            Format = _languageResource.GetString("dateLong");
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "time";
            if(FreindlyTime == true)
            { output.AddClass("timeago", HtmlEncoder.Default); }
            output.Attributes.Add("datetime",SnitzDate?.ToTimeagoDate());
            output.Attributes.Add("aria-label", $"{SnitzDate?.ToLocalTime()}");
            output.PreContent.SetHtmlContent(SnitzDate?.ToLocalTime().ToForumDisplay());

        }
    }
}

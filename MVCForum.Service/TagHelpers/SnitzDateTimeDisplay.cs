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
    /// <summary>
    /// A custom <see cref="TagHelper"/> that renders a <c>&lt;time&gt;</c> HTML element to display a date and time with
    /// optional formatting and "friendly time" support.
    /// </summary>
    /// <remarks>This tag helper is used to render a <c>&lt;time&gt;</c> element with attributes and content
    /// based on the provided date, format, and friendly time settings. It supports rendering the date in a
    /// user-friendly "time ago" format or a custom format string.</remarks>
    [HtmlTargetElement("snitz-datetime", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SnitzDateTimeDisplay : TagHelper
    {
        [HtmlAttributeName("datetime")]
        public DateTime? SnitzDate { get; set; }

        public bool? FriendlyTime { get; set; } = true;

        [HtmlAttributeName("format")]
        public string? Format { get; set; }

        private readonly LanguageService  _languageResource;
        private readonly ISnitzConfig _snitzconfig;

        public SnitzDateTimeDisplay(IHtmlLocalizerFactory localizerFactory ,ISnitzConfig snitzconfig)
        {
            _languageResource = (LanguageService)localizerFactory.Create("SnitzController", "MVCForum");
            Format = _languageResource.GetString("dateLong");
            _snitzconfig = snitzconfig;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "time";
            if(FriendlyTime == true && _snitzconfig.IsEnabled("INTUSETIMEAGO"))
            { output.AddClass("timeago", HtmlEncoder.Default); }
            output.Attributes.Add("datetime",SnitzDate?.ToTimeagoDate());
            output.Attributes.Add("aria-label", $"{SnitzDate?.ToLocalTime()}");
            output.Attributes.Add("data-toggle","tooltip");
            if (!string.IsNullOrWhiteSpace(Format))
            {
                output.PreContent.SetHtmlContent(SnitzDate?.ToCustomDisplay(Format));
            }
            else
            {
                output.PreContent.SetHtmlContent(SnitzDate?.ToForumDisplay(_snitzconfig));
            }

        }
    }
}

using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Service.Extensions;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("snitz-datetime", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SnitzDateTimeDisplay : TagHelper
    {
        [HtmlAttributeName("datetime")]
        public DateTime? SnitzDate { get; set; }

        public bool? FreindlyTime { get; set; } = true;

        [HtmlAttributeName("format")]
        public string? Format { get; set; } = "dd MMM yyyy HH:mm";

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

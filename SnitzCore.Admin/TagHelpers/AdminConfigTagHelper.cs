using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System.Formats.Asn1;
using System.Reflection;
using System.Resources;
using System.Text.Encodings.Web;

namespace SnitzCore.BackOffice.TagHelpers
{
    [HtmlTargetElement("admin-config", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AdminConfigTagHelper : TagHelper
    {
        [HtmlAttributeName("config-key")]
        public string? Key { get; set; }
        [HtmlAttributeName("config-val")]
        public string? Value { get; set; }
        [HtmlAttributeName("config-label")]
        public string? Label { get; set; }
        [HtmlAttributeName("config-req")]
        public bool Required { get; set; }
        [HtmlAttributeName("placeholder")]
        public string? PlaceHolder { get; set; }
        [HtmlAttributeName("config-type")]
        public string? ControlType { get; set; }
        [HtmlAttributeName("disabled")]
        public bool Disabled { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            
            var valtype = Value != null && Value.IsNumeric() ? "number" : "text";
            var required = "";
            var disabled = Disabled ? "disabled" : "";

            if (valtype == "number")
            {
                try
                {
                    var num = Convert.ToInt64(Value);
                    if (num < 2)
                    {
                        valtype = "checkbox";
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (!String.IsNullOrWhiteSpace(ControlType))
            {
                valtype = ControlType;
            }
            base.Process(context, output);


            switch (valtype)
            {
                case "checkbox":
                    var ischecked = "";
                    if (Value == "1")
                    {
                        ischecked = "checked";
                    }
                    output.TagName = "div";
                    output.Content.AppendHtml($@"<input type=""{valtype}"" role=""switch"" name=""{Key}"" id=""{Key}"" value=""1"" class=""form-check-input"" {required} {disabled} {ischecked} style=""transform: scale(1.4);""/>");
                    output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{Key}"" value=""0"" {required} {disabled} checked style=""display:none""/>");
                    output.Content.AppendHtml($@"<label class=""form-check-label"" for=""{Key}"">{Label}</label></label>");
                    output.AddClass("form-check",HtmlEncoder.Default);
                    output.AddClass("form-switch",HtmlEncoder.Default);
                    output.AddClass("mb-3",HtmlEncoder.Default);
                    output.TagMode = TagMode.StartTagAndEndTag;

                    break;
                case "textarea":
                    output.TagName = "div";
                    output.AddClass("mb-3",HtmlEncoder.Default);
                    output.Content.AppendHtml($@"<label for=""{Key}"" class=""form-label"">{Label}</label>");
                        output.Content.AppendHtml($@"<textarea type=""{valtype}"" name=""{Key}"" id=""{Key}"" class=""form-control"" {required} {disabled} rows=""4"" placeholder=""{PlaceHolder}"">{Value}</textarea>");
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "hidden":
                    output.TagName = "div";
                    output.Content.AppendHtml(
                        $@"<input type=""{valtype}"" name=""{Key}"" id=""{Key}"" value=""{Value}"" />");
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "datepicker":
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{Key}"">{Key}</label>");
                        output.Content.AppendHtml(@"<div class=""datepicker date input-group"">");
                        output.Content.AppendHtml($@"<input name=""{Key}"" type=""text"" placeholder=""Choose Date"" class=""form-control"" id=""fecha2""/>");
                        output.Content.AppendHtml(@"<div class=""input-group-append"">
                            <span class=""input-group-text""><i class=""fa fa-calendar""></i></span>
                        </div></div>");

                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;

                default:
                    output.TagName = "div";
                    output.AddClass("mb-3",HtmlEncoder.Default);
                    output.Content.AppendHtml($@"<label for=""{Key}"" class=""form-label"">{Label}</label>");
                        output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{Key}"" id=""{Key}"" value=""{Value}"" class=""form-control"" {required} {disabled} placeholder=""{PlaceHolder}""/>");
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
            }

        }

    }
}

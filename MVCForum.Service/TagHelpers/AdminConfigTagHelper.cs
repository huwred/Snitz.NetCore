using BbCodeFormatter;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;

namespace SnitzCore.Services.TagHelpers
{
    /// <summary>
    /// A TagHelper that renders an HTML element for configuring administrative settings.
    /// </summary>
    /// <remarks>This TagHelper generates various input controls (e.g., textboxes, checkboxes, textareas,
    /// etc.) based on the specified attributes. It supports customization of the input type, placeholder text, labels,
    /// and other properties. The generated HTML structure is tailored for use in administrative configuration
    /// forms.</remarks>
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
        public string? HelpText {get;set;}
        [HtmlAttributeName("disabled")]
        public bool Disabled { get; set; }
        public Func<string, string>? TextLocalizerDelegate { get; set; }

        private readonly ICodeProcessor _bbCodeProcessor;

        public AdminConfigTagHelper(ICodeProcessor bbCodeProcessor)
        {

            _bbCodeProcessor = bbCodeProcessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var helpIcon = $@"&nbsp;<i class=""fa fa-question-circle fs-4"" data-bs-toggle=""tooltip"" data-bs-html=""true"" title=""{HelpText??Label}""></i>";

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
            if (Value.IsBoolean())
            {
                valtype = "checkbox";
            }

            if (!String.IsNullOrWhiteSpace(ControlType))
            {
                valtype = ControlType;
            }
            base.Process(context, output);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            switch (valtype)
            {
                case "checkbox":
                    var ischecked = "";
                    if (Value != null && Value != "0")
                    {
                        ischecked = "checked";
                    }

                    output.Content.AppendHtml($@"<input type=""{valtype}"" role=""switch"" name=""{Key}"" id=""{Key}"" value=""1"" class=""form-check-input"" {required} {disabled} {ischecked} style=""transform: scale(1.4);""/>");
                    output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{Key}"" value=""0"" {required} {disabled} checked style=""display:none""/>");
                    if(!String.IsNullOrEmpty(HelpText)) output.Content.AppendHtml(helpIcon);
                    output.Content.AppendHtml($@"&nbsp;<label class=""form-check-label"" for=""{Key}"" >{Label}</label></label>");
                    output.AddClass("form-check",HtmlEncoder.Default);
                    output.AddClass("form-switch",HtmlEncoder.Default);
                    output.AddClass("mb-3",HtmlEncoder.Default);

                    break;
                case "textarea":
                    output.AddClass("mb-3",HtmlEncoder.Default);
                    output.Content.AppendHtml($@"<label for=""{Key}"" class=""form-label"">{Label}</label>");
                    if(!String.IsNullOrEmpty(HelpText)) output.Content.AppendHtml(helpIcon);
                    output.Content.AppendHtml($@"<textarea type=""{valtype}"" name=""{Key}"" id=""{Key}"" class=""form-control"" {required} {disabled} rows=""4"" placeholder=""{PlaceHolder}"">{Value}</textarea>");
                    break;
                case "hidden":
                    output.Content.AppendHtml(
                        $@"<input type=""{valtype}"" name=""{Key}"" id=""{Key}"" value=""{Value}"" />");
                    break;
                case "select-subs" :
                    output.AddClass("mb-3",HtmlEncoder.Default);

                    var items = GetEnumSelectList(Value!);
                    StringBuilder options = new StringBuilder();
                    foreach (var item in items)
                    {
                        if (item.Selected)
                        {
                            options.Append($"<option value=\"{item.Value}\" selected>{item.Text}</option>");
                        }
                        else
                        {
                            options.Append($"<option value=\"{item.Value}\">{item.Text}</option>");
                        }
                    }
                    output.Content.AppendHtml($@"<label for=""{Key}"" class=""form-label"">{Label}</label>");
                    if(!String.IsNullOrEmpty(HelpText)) output.Content.AppendHtml(helpIcon);
                    output.Content.AppendHtml($@"<select class=""form-select"" name=""{Key}"" {disabled}><option>-- Select Me --</option>{options}</select>");

                    output.Content.AppendHtml($@"<div class=""invalid-feedback"">You must select a value for {Key}.</div>");
                    break;
                case "datepicker":
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml($@"<label for=""{Key}"">{Key}</label>");
                    if(!String.IsNullOrEmpty(HelpText)) output.Content.AppendHtml(helpIcon);
                        output.Content.AppendHtml(@"<div class=""datepicker date input-group"">");
                        output.Content.AppendHtml($@"<input name=""{Key}"" type=""text"" placeholder=""Choose Date"" class=""form-control"" id=""fecha2"" {disabled}/>");
                        output.Content.AppendHtml(@"<div class=""input-group-append"">
                            <span class=""input-group-text""><i class=""fa fa-calendar""></i></span>
                        </div></div>");
                    break;
                default:
                    output.AddClass("mb-3",HtmlEncoder.Default);
                    output.Content.AppendHtml($@"<label for=""{Key}"" class=""form-label"">{Label}</label>");
                    if(!String.IsNullOrEmpty(HelpText)) output.Content.AppendHtml(helpIcon);
                    output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{Key}"" id=""{Key}"" value=""{Value}"" class=""form-control"" {required} {disabled} placeholder=""{PlaceHolder}"" />");
                    break;
            }

        }
        private IEnumerable<SelectListItem> GetEnumSelectList(string selected)
        {
            var enumValues = Enum.GetValues(typeof(SubscriptionLevel)).Cast<SubscriptionLevel>();
            return enumValues.Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString(),
                Selected = ((int)v).ToString() == selected
            }).ToList();

            //return (Enum.GetValues(typeof(T)).Cast<T>().Select(
            //    enu => new SelectListItem() { Text = enu.ToString(), Value = (int)enu,Selected = selected==enu.ToString()})).ToList();
        }
    }
}

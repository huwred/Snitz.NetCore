using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;

namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// TagHelper to render input fields for data in the FORUM_CONFIG_NEW table
    /// </summary>
    [HtmlTargetElement("snitz-config", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SnitzConfigTagHelper : TagHelper
    {
        private readonly ILogger<SnitzConfigTagHelper> _logger;
        [HtmlAttributeName("property-val")]
        public string? Value { get; set; }
        [HtmlAttributeName("property-req")]
        public bool Required { get; set; }
        [HtmlAttributeName("config-type")]
        public string? ControlType { get; set; }
        [HtmlAttributeName("property-info")]
        public PropertyInfo? PropertyInfo { get; set; }
        [HtmlAttributeName("can-edit")]
        public bool CanEdit { get; set; }
        public string? Name { get; set; }
        public Func<string, string>? TextLocalizerDelegate { get; set; }

        public SnitzConfigTagHelper(ILogger<SnitzConfigTagHelper> logger)
        {
            _logger = logger;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var valtype = Value != null && Value.IsNumeric() ? "number" : "text";
            var required = "";
            var disabled = "";

            if (PropertyInfo != null && PropertyInfo.PropertyReadOnly())
            {
                disabled = "disabled";
            }
            if (Required)
            {
                required = @"required=""required""";
            }
            if (valtype == "number")
            {
                if (long.TryParse(Value, out long num) && num < 2)
                {
                    valtype = "checkbox";
                }
            }
            if (!string.IsNullOrWhiteSpace(ControlType))
            {
                valtype = ControlType;
            }

            if (PropertyInfo != null && PropertyInfo.SystemProperty())
            {
                valtype = "hidden";
            }
            if (PropertyInfo != null && PropertyInfo.PropertyFieldType() != "text")
            {
                valtype = PropertyInfo.PropertyFieldType();
            }
            base.Process(context, output);
            var displayName = PropertyInfo?.Name;
            try
            {
                if (PropertyInfo != null) displayName = PropertyInfo.GetPropertyDisplayName<Member>();
                if (displayName != null && TextLocalizerDelegate != null)
                {
                    displayName = TextLocalizerDelegate(displayName);
                }

            }
            catch (Exception)
            {
                // ignored
            }

            switch (valtype)
            {
                case "checkbox":
                    var ischecked = "";
                    if (Value == "1")
                    {
                        ischecked = "checked";
                    }
                    output.TagName = "div";
                    output.Content.AppendHtml($@"<input type=""{valtype}"" role=""switch"" name=""{PropertyInfo?.Name}"" id=""{PropertyInfo?.Name}"" value=""1"" class=""form-check-input"" {required} {disabled} {ischecked} style=""transform: scale(1.4);""/>");
                    output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{PropertyInfo?.Name}"" value=""0"" {required} {disabled} checked style=""display:none""/>");
                    output.Content.AppendHtml($@"<label class=""form-check-label"" for=""{PropertyInfo?.Name}"">{displayName ?? PropertyInfo?.Name}</label>");
                    output.AddClass("form-check", HtmlEncoder.Default);
                    output.AddClass("form-switch", HtmlEncoder.Default);
                    output.AddClass("mb-3", HtmlEncoder.Default);
                    if (!CanEdit)
                    {
                        output.AddClass("d-none", HtmlEncoder.Default);
                    }
                    output.TagMode = TagMode.StartTagAndEndTag;

                    break;
                case "textarea":
                    output.TagName = "div";
                    output.AddClass("mb-3", HtmlEncoder.Default);
                    output.Content.AppendHtml($@"<label class=""form-label"" for=""{PropertyInfo?.Name}"">{displayName ?? PropertyInfo?.Name}</label>");
                    if (CanEdit)
                    {
                        var id = PropertyInfo?.Name;
                        if(PropertyInfo?.Name == "Signature")
                        {
                            id = "msg-text";
                        }
                        output.Content.AppendHtml($@"<textarea type=""{valtype}"" title=""{PropertyInfo?.Name}"" name=""{PropertyInfo?.Name}"" id=""{id}"" class=""form-control"" {required} {disabled} rows=""3"">{Value}</textarea>");
                        output.Content.AppendHtml($@"<div class=""invalid-feedback"">You must enter a value for {PropertyInfo?.Name}.</div>");
                    }
                    else
                    {
                        output.Content.AppendHtml($@"<label class=""form-label m-2"" >{Value}</label>");
                    }
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "hidden":
                    output.TagName = "div";
                    output.Content.AppendHtml(
                        $@"<input type=""{valtype}"" name=""{PropertyInfo?.Name}"" id=""{PropertyInfo?.Name}"" value=""{Value}"" />");
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "select":
                    output.TagName = "div";
                    output.AddClass("mb-3", HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{PropertyInfo?.Name}"" class=""form-label"">{displayName ?? PropertyInfo?.Name}</label>");
                    if (!CanEdit)
                    {
                        output.AddClass("d-none", HtmlEncoder.Default);
                    }
                    output.Content.AppendHtml(GenerateDropDownList(required));
                    output.Content.AppendHtml($@"<div class=""invalid-feedback"">You must select a value for {PropertyInfo?.Name}.</div>");

                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "datepicker":
                    output.TagName = "div";
                    output.AddClass("mb-3", HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{PropertyInfo?.Name}"">{displayName ?? PropertyInfo?.Name}</label>");
                    if (CanEdit)
                    {
                        output.Content.AppendHtml($@"<div class=""input-group date"" data-provide=""datepicker"">");
                        output.Content.AppendHtml($@"<input name=""{Name ?? PropertyInfo?.Name}"" type=""text"" placeholder=""Choose Date"" class=""form-control"" id=""fecha2"" {required} {disabled}/>");
                        output.Content.AppendHtml($@"<span class=""input-group-addon btn btn-default-outline""><i class=""fa fa-calendar""></i></span></div>");
                        output.Content.AppendHtml($@"<div class=""invalid-feedback"">You must select a value for {PropertyInfo?.Name}.</div>");
                    }
                    else
                    {
                        output.Content.AppendHtml($@"<label class=""form-label m-2"" >{Value}</label>");
                    }

                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "file":
                    output.TagName = "div";
                    output.AddClass("mb-3", HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""inputFile"" class=""form-label"">{displayName ?? PropertyInfo?.Name}</label>");
                    if (CanEdit)
                    {
                        output.Content.AppendHtml($@"<input name=""{PropertyInfo?.Name}"" type=""file"" class=""form-control"" id=""inputFile"" aria-describedby=""fileHelp"" {required} {disabled}/>");
                        output.Content.AppendHtml($@"<label class=""form-text"" for=""inputFile"">{Value ?? "Choose file"}</label>");
                    }
                    else
                    {
                        output.Content.AppendHtml($@"<label class=""form-label m-2"" for=""inputFile"">{Value ?? "Choose file"}</label>");
                    }
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                default:
                    output.TagName = "div";
                    output.AddClass("mb-3", HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label class=""form-label""  for=""{PropertyInfo?.Name}"">{displayName ?? PropertyInfo?.Name}</label>");
                    if (CanEdit)
                    {
                        output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{Name ?? PropertyInfo?.Name}"" id=""{PropertyInfo?.Name}"" value=""{Value}"" class=""form-control"" {required} {disabled}/>");
                        output.Content.AppendHtml($@"<div class=""invalid-feedback"">You must provide a value for {PropertyInfo?.Name}.</div>");
                    }
                    else
                    {
                        output.Content.AppendHtml($@"<label class=""form-label m-2"" >{Value}</label>");
                    }

                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
            }

        }
        private IHtmlContent GenerateDropDownList(string required)
        {
            TagBuilder tb = new TagBuilder("select");
            if (PropertyInfo != null)
            {
                var enumname = PropertyInfo.GetSelectEnum();
                Type? test = EnumExtensions.GetEnumType(enumname);
                if (enumname != null)
                {
                    tb.AddCssClass("form-select");
                    tb.Attributes.Add("Name", PropertyInfo.Name);
                    tb.MergeAttribute("id", PropertyInfo.Name + "-dd");
                    tb.InnerHtml.AppendHtml(required);
                    foreach (SelectListItem item in test!.GetEnumSelectList())
                    {
                        TagBuilder op = new TagBuilder("option");
                        op.Attributes.Add("value", item.Value);
                        if (TextLocalizerDelegate != null) op.InnerHtml.AppendHtml(TextLocalizerDelegate(item.Text));
                        tb.InnerHtml.AppendHtml(op);

                        if (item.Value == Value)
                        {
                            op.Attributes.Add("selected", "selected");
                        }
                    }
                }
            }

            tb.InnerHtml.AppendHtml(GenerateInput());
            return tb;
        }

        private IHtmlContent GenerateInput()
        {
            TagBuilder tb = new TagBuilder("input");

            tb.TagRenderMode = TagRenderMode.SelfClosing;
            tb.MergeAttribute("name", PropertyInfo?.Name);
            tb.MergeAttribute("type", "hidden");
            tb.MergeAttribute("value", Value);
            return tb;
        }

    }
}

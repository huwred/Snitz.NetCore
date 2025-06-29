using System;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnitzCore.Services.TagHelpers
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
        public Func<string, string>? TextLocalizerDelegate { get; set; }

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
                    output.Content.AppendHtml($@"&nbsp;<label class=""form-check-label"" for=""{Key}"">{Label}</label></label>");
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
                case "select-subs" :
                    output.TagName = "div";
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
                    output.Content.AppendHtml(

                        $@"<label for=""{Key}"" class=""form-label"">{Label}</label>
                            <select class=""form-select"" name=""{Key}"" {disabled}>
                                <option>-- Select Me --</option>{options}

                            </select>");

                    //output.Content.AppendHtml(GenerateDropDownList(required));
                    output.Content.AppendHtml($@"<div class=""invalid-feedback"">You must select a value for {Key}.</div>");

                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "datepicker":
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{Key}"">{Key}</label>");
                        output.Content.AppendHtml(@"<div class=""datepicker date input-group"">");
                        output.Content.AppendHtml($@"<input name=""{Key}"" type=""text"" placeholder=""Choose Date"" class=""form-control"" id=""fecha2"" {disabled}/>");
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
        //private IHtmlContent GenerateDropDownList(string required)
        //{
        //    TagBuilder tb = new TagBuilder("select");
        //    if (PropertyInfo != null)
        //    {
        //        var enumname = PropertyInfo.GetSelectEnum();
        //        Type? test = EnumExtensions.GetEnumType (enumname);
        //        if (enumname != null)
        //        {
        //            tb.AddCssClass("form-select");
        //            tb.Attributes.Add("Name",PropertyInfo.Name);
        //            tb.MergeAttribute("id", PropertyInfo.Name + "-dd");
        //            tb.InnerHtml.AppendHtml(required);
        //            foreach (SelectListItem item in test!.GetEnumSelectList())
        //            {
        //                TagBuilder op = new TagBuilder("option");
        //                op.Attributes.Add("value",item.Value);
        //                if (TextLocalizerDelegate != null) op.InnerHtml.AppendHtml(TextLocalizerDelegate(item.Text));
        //                tb.InnerHtml.AppendHtml(op);
                    
        //                if (item.Value == Value)
        //                {
        //                    op.Attributes.Add("selected","selected");
        //                }
        //            }
        //        }
        //    }

        //    tb.InnerHtml.AppendHtml(this.GenerateInput());
        //    return tb;
        //}
        //private IHtmlContent GenerateInput()
        //{
        //    TagBuilder tb = new TagBuilder("input");

        //    tb.TagRenderMode = TagRenderMode.SelfClosing;
        //    tb.MergeAttribute("name", PropertyInfo?.Name);
        //    tb.MergeAttribute("type", "hidden");
        //    tb.MergeAttribute("value", Value);
        //    return tb;
        //}
    }
}

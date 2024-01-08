using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MVCForum.Extensions;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;
using System.Reflection;
using System.Text.Encodings.Web;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("snitz-config", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SnitzConfigTagHelper : TagHelper
    {

        [HtmlAttributeName("property-val")]
        public string Value { get; set; }
        [HtmlAttributeName("property-req")]
        public bool Required { get; set; }

        [HtmlAttributeName("property-info")]
        public PropertyInfo PropertyInfo { get; set; }
        [HtmlAttributeName("can-edit")]
        public bool CanEdit { get; set; }
        public SnitzConfigTagHelper() { }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var valtype = Value != null && Value.IsNumeric() ? "number" : "text";
            var required = "";
            var disabled = "";

            if (PropertyInfo.PropertyReadOnly())
            {
                disabled = "disabled";
            }
            if (Required)
            {
                required = @"required=""required""";
            }
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
                catch (Exception e)
                {

                }

            }


            if (PropertyInfo.SystemProperty())
            {
                valtype = "hidden";
            }
            if (PropertyInfo.PropertyFieldType() != "text")
            {
                valtype = PropertyInfo.PropertyFieldType();
            }            
            base.Process(context, output);
            var displayName = PropertyInfo.Name;
            try
            {
                displayName = PropertyInfo.GetPropertyDisplayName<Member>();
            }
            catch (Exception e)
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
                    output.Content.AppendHtml($@"<label class=""form-check-label"">");
                    output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{PropertyInfo.Name}"" id=""{PropertyInfo.Name}"" value=""1"" class=""form-check-input"" {required} {disabled} {ischecked}/>");
                    output.Content.AppendHtml($@"<label for=""{PropertyInfo.Name}"">{displayName??PropertyInfo.Name}</label></label>");
                    output.AddClass("form-check",HtmlEncoder.Default);
                    if (!CanEdit)
                    {
                        output.AddClass("d-none",HtmlEncoder.Default);
                    }                    
                    output.TagMode = TagMode.StartTagAndEndTag;

                    break;
                case "textarea":
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml($@"<label class=""form-label"" for=""{PropertyInfo.Name}"">{displayName??PropertyInfo.Name}</label>");
                    if (CanEdit)
                    {
                        output.Content.AppendHtml($@"<textarea type=""{valtype}"" name=""{PropertyInfo.Name}"" id=""{PropertyInfo.Name}"" class=""form-control"" {required} {disabled} rows=""3"">{Value}</textarea>");
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
                        $@"<input type=""{valtype}"" name=""{PropertyInfo.Name}"" id=""{PropertyInfo.Name}"" value=""{Value}"" />");
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "select" :
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{PropertyInfo.Name}"">{displayName ?? PropertyInfo.Name}</label>");
                    if (!CanEdit)
                    {
                        output.AddClass("d-none",HtmlEncoder.Default);
                    }
                    output.Content.AppendHtml(GenerateDropDownList());
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "datepicker":
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{PropertyInfo.Name}"">{displayName??PropertyInfo.Name}</label>");
                    if (CanEdit)
                    {
                        output.Content.AppendHtml($@"<div class=""datepicker date input-group"">");
                        output.Content.AppendHtml($@"<input name=""{PropertyInfo.Name}"" type=""text"" placeholder=""Choose Date"" class=""form-control"" id=""fecha2""/>");
                        output.Content.AppendHtml($@"<div class=""input-group-append"">
                            <span class=""input-group-text""><i class=""fa fa-calendar""></i></span>
                        </div></div>");
                    }
                    else
                    {
                        output.Content.AppendHtml($@"<label class=""form-label m-2"" >{Value}</label>");
                    }

                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                case "file":
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{PropertyInfo.Name}"">{displayName??PropertyInfo.Name}</label>
                            <div class=""custom-file"" id=""customFile"">");
                    if (CanEdit)
                    {
                        output.Content.AppendHtml($@"<input name=""{PropertyInfo.Name}"" type=""file"" class=""custom-file-input"" id=""exampleInputFile"" aria-describedby=""fileHelp""/>");
                        output.Content.AppendHtml($@"<label class=""form-label"" for=""exampleInputFile"">{Value??"Choose file"}</label>");
                    }
                    else
                    {
                        output.Content.AppendHtml($@"<label class=""form-label m-2"" for=""exampleInputFile"">{Value??"Choose file"}</label>");
                    }
                    output.Content.AppendHtml($@"</div>");
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
                default:
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label class=""form-label""  for=""{PropertyInfo.Name}"">{displayName??PropertyInfo.Name}</label>");
                    if (CanEdit)
                    {
                        output.Content.AppendHtml($@"<input type=""{valtype}"" name=""{PropertyInfo.Name}"" id=""{PropertyInfo.Name}"" value=""{Value}"" class=""form-control"" {required} {disabled}/>");
                    }
                    else
                    {
                        output.Content.AppendHtml($@"<label class=""form-label m-2"" >{Value}</label>");
                    }
                    
                    output.TagMode = TagMode.StartTagAndEndTag;
                    break;
            }

        }
        private IHtmlContent GenerateDropDownList()
        {
            TagBuilder tb = new TagBuilder("select");
            var enumname = PropertyInfo.GetSelectEnum();
            Type test = EnumExtensions.GetEnumType (enumname);
            if (enumname != null)
            {
                tb.AddCssClass("form-control");
                tb.Attributes.Add("Name",PropertyInfo.Name);
                tb.MergeAttribute("id", PropertyInfo.Name + "-dd");
                foreach (SelectListItem item in test.GetEnumSelectList())
                {
                    TagBuilder op = new TagBuilder("option");
                    op.Attributes.Add("value",item.Value);
                    op.InnerHtml.AppendHtml(item.Text);
                    tb.InnerHtml.AppendHtml(op);
                    if (item.Value == Value)
                    {
                        op.Attributes.Add("selected","selected");
                    }
                }
            }

            tb.InnerHtml.AppendHtml(this.GenerateInput());
            return tb;
        }

        private IHtmlContent GenerateInput()
        {
            TagBuilder tb = new TagBuilder("input");

            tb.TagRenderMode = TagRenderMode.SelfClosing;
            tb.MergeAttribute("name", PropertyInfo.Name);
            tb.MergeAttribute("type", "hidden");
            tb.MergeAttribute("value", Value);
            return tb;
        }

    }
}

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
        public string Key { get; set; }
        [HtmlAttributeName("config-val")]
        public string Value { get; set; }
        [HtmlAttributeName("config-label")]
        public string Label { get; set; }
        [HtmlAttributeName("config-req")]
        public bool Required { get; set; }
        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }
        [HtmlAttributeName("config-type")]
        public string ControlType { get; set; }
        public AdminConfigTagHelper() { }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            
            var valtype = Value != null && Value.IsNumeric() ? "number" : "text";
            var required = "";
            var disabled = "";

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
                    output.Content.AppendHtml($@"<label class=""form-check-label"" for=""{Key}"">{Label}</label></label>");
                    output.AddClass("form-check",HtmlEncoder.Default);
                    output.AddClass("form-switch",HtmlEncoder.Default);
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
                //case "select" :
                //    output.TagName = "div";
                //    output.AddClass("form-group",HtmlEncoder.Default);
                //    output.Content.AppendHtml(
                //        $@"<label for=""{Key}"">{Key}</label>");
                //    output.Content.AppendHtml(GenerateDropDownList());
                //    output.TagMode = TagMode.StartTagAndEndTag;
                //    break;
                case "datepicker":
                    output.TagName = "div";
                    output.AddClass("form-group",HtmlEncoder.Default);
                    output.Content.AppendHtml(
                        $@"<label for=""{Key}"">{Key}</label>");
                        output.Content.AppendHtml($@"<div class=""datepicker date input-group"">");
                        output.Content.AppendHtml($@"<input name=""{Key}"" type=""text"" placeholder=""Choose Date"" class=""form-control"" id=""fecha2""/>");
                        output.Content.AppendHtml($@"<div class=""input-group-append"">
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
        //private IHtmlContent GenerateDropDownList()
        //{
        //    TagBuilder tb = new TagBuilder("select");
        //    var enumname = PropertyInfo.GetSelectEnum();
        //    Type test = EnumExtensions.GetEnumType (enumname);
        //    if (enumname != null)
        //    {
        //        tb.AddCssClass("form-control");
        //        tb.Attributes.Add("Name",PropertyInfo.Name);
        //        tb.MergeAttribute("id", PropertyInfo.Name + "-dd");
        //        foreach (SelectListItem item in test.GetEnumSelectList())
        //        {
        //            TagBuilder op = new TagBuilder("option");
        //            op.Attributes.Add("value",item.Value);
        //            op.InnerHtml.AppendHtml(item.Text);
        //            tb.InnerHtml.AppendHtml(op);
        //            if (item.Value == Value)
        //            {
        //                op.Attributes.Add("selected","selected");
        //            }
        //        }
        //    }

        //    tb.InnerHtml.AppendHtml(this.GenerateInput());
        //    return tb;
        //}
    }
}

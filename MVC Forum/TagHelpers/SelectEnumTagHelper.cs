using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;
using System;
using System.Globalization;
using System.Linq;
using SnitzCore.Service.Extensions;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("enum-select")]
    public class SelectEnumTagHelper : TagHelper
    {

        /// <summary>
        /// (int)MyEnum.ValueName
        /// </summary>
        public int SelectedValue { get; set; }

        public DateTime? LastVisit { get; set; }
        /// <summary>
        /// typeof(MyEnum)
        /// </summary>
        public Type EnumType { get; set; } = null!;

        /// <summary>
        /// A delegate function for getting locaized value.
        /// </summary>
        public Func<string, string>? TextLocalizerDelegate { get; set; }

        /// <summary>
        /// start creating select-enum tag helper
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.Attributes.Add("class","form-select mb-2");

            foreach (int e in Enum.GetValues(EnumType))
            {
                var op = new TagBuilder("option");


                op.Attributes.Add("value", $"{e}");

                var displayText = TextLocalizerDelegate == null
                    ? GetEnumFieldDisplayName(e)
                    : GetEnumFieldLocalizedDisplayName(e);

                op.InnerHtml.Append(displayText);

                if (e == SelectedValue)
                    op.Attributes.Add("selected", "selected");

                output.Content.AppendHtml(op);
            }
        }

        private string GetEnumFieldDisplayName(int value)
        {
            // get enum field name
            var fieldName = Enum.GetName(EnumType, value);

            //get Display(Name = "Field name")
            if (fieldName != null)
            {
                var displayName = EnumType.GetField(fieldName)?.GetCustomAttributes(false).OfType<DisplayAttribute>().SingleOrDefault()?.Name;

                return displayName ?? fieldName;
            }

            return fieldName ??  "";
        }

        private string GetEnumFieldLocalizedDisplayName(int value)
        {
            var text = GetEnumFieldDisplayName(value);

            if (LastVisit != null)
            {
                if (TextLocalizerDelegate != null)
                    return TextLocalizerDelegate(text).Replace("[[LASTVISIT]]", LastVisit.Value.ToForumDateTimeDisplay());
            }

            if (TextLocalizerDelegate != null)
            {
                return TextLocalizerDelegate(text);
            }

            return text;
        }
    }
}

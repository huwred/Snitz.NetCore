using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("option", Attributes = "selected-val")]
    public class OptionSelectTagHelper : TagHelper
    {
        [HtmlAttributeName("selected-val")]
        public string? Selected { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var tag = output.Attributes["value"];
            if (Selected != null && Selected == (string)tag.Value)
            {
                output.Attributes.Add("selected", "selected");
            }

        }
    }
}
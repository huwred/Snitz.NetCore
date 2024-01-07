using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "snitz-if")]
    public class IncludeIfTagHelper : TagHelper
    {
        /// <summary>
        /// A boolean expression
        /// If it evaluates to true then it will be kept in the page
        /// </summary>
        [HtmlAttributeName("snitz-if")]
        public bool? Predicate { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!Predicate.HasValue || !Predicate.Value)
            {
                //output.AddClass("hidden",HtmlEncoder.Default);

                output.SuppressOutput();
            }
        }
    }
}
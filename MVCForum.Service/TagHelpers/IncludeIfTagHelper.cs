using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SnitzCore.Service.TagHelpers
{
    /// <summary>
    /// A TagHelper that conditionally includes or suppresses an HTML element based on a boolean predicate.
    /// </summary>
    /// <remarks>Use this TagHelper to conditionally render an HTML element in the output. The element will be
    /// included only if the specified predicate evaluates to <see langword="true"/>. If the predicate is <see
    /// langword="false"/> or <see langword="null"/>, the element will be suppressed and will not appear in the rendered
    /// output.</remarks>
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
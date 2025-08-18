using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;


    /// <summary>
    /// The default input and select tag helpers don't add the <c>required</c> attribute to required
    /// elements, so this does it instead
    /// </summary>
    [HtmlTargetElement("input", Attributes = "asp-for")]
    [HtmlTargetElement("select", Attributes = "asp-for")]
    [HtmlTargetElement("textarea", Attributes = "asp-for")]
    public class RequiredInputTagHelper : TagHelper
    {
        public override int Order { get => int.MaxValue; }

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            // Don't do anything if there's already a required attribute
            if (context.AllAttributes["required"] is not null)
                return;

            // Don't make check boxes required, that indicates that it must be checked
            if (For.Model is bool)
                return;

            // The property has [Required] or it is a value type (e.g. int) and is not nullable (e.g. int?)
            if (For.ModelExplorer.Metadata.ValidatorMetadata.Any(a => a is RequiredAttribute)
                || For.ModelExplorer.ModelType.IsValueType
                    && (!For.ModelExplorer.ModelType.IsGenericType
                        || For.ModelExplorer.ModelType.IsGenericType
                            && For.ModelExplorer.ModelType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                output.Attributes.Add(new TagHelperAttribute("required"));
        }
    }

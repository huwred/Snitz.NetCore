using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace SnitzCore.Service.TagHelpers
{
    internal class ModalContext
    {
        public IHtmlContent Body { get; set; }
        public IHtmlContent Footer { get; set; }

    }

    public enum ModalSize { Default, Small, Large }

    /// <summary>
    /// Represents a tag helper for rendering a Bootstrap modal dialog.
    /// </summary>
    /// <remarks>This tag helper generates the HTML structure for a Bootstrap modal dialog, including the
    /// modal header, body, and footer. The modal can be customized using the <see cref="Size"/>, <see cref="Title"/>,
    /// and <see cref="Id"/> properties. Child elements such as <c>modal-body</c> and <c>modal-footer</c> can be used to
    /// define the content of the modal.</remarks>
    [HtmlTargetElement("modal", TagStructure = TagStructure.NormalOrSelfClosing)]
    [RestrictChildren("modal-body", "modal-footer")]
    public class ModalTagHelper : TagHelper
    {
        public ModalSize Size { get; set; }

        public string Title { get; set; }

        public string Id { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var modalContext = new ModalContext();
            context.Items.Add(typeof(ModalTagHelper), modalContext);
            await output.GetChildContentAsync();

            output.TagName = "div";
            output.Attributes.SetAttribute("role", "dialog");
            output.Attributes.SetAttribute("id", Id);
            output.Attributes.SetAttribute("aria-labelledby", $"Label_{context.UniqueId}");
            output.Attributes.SetAttribute("tabindex", "-1");
            output.Attributes.SetAttribute("aria-hidden", "true");

            var classNames = "modal fade";
            if (output.Attributes.ContainsName("class"))
                classNames = string.Concat(output.Attributes["class"].Value, " ", classNames);
            output.Attributes.SetAttribute("class", classNames);

            var size = Size == ModalSize.Small ? "modal-sm" : Size == ModalSize.Large ? "modal-lg" : string.Empty;
            var template = $@"<div class='modal-dialog {size} modal-dialog-centered modal-dialog-scrollable' role='document'>
                <div class='modal-content'>
                    <div class='modal-header bg-primary'>
                        <h4 class='modal-title text-bg-primary' id='Label_{context.UniqueId}'>{Title}</h4>
                        <button type='button' class='btn btn-md btn-danger' data-bs-dismiss='modal' aria-label='Close'><span aria-hidden='true'><i class=""fa fa-times"" aria-hidden=""true""></i></span></button>
                    </div>";

            output.Content.AppendHtmlLine("\n" + template);

            output.Content.AppendHtml("<div class='modal-body'>");
            if (modalContext.Body != null)
                output.Content.AppendHtml(modalContext.Body);
            output.Content.AppendHtmlLine("</div>");

            if (modalContext.Footer != null)
            {
                output.Content.AppendHtml("<div class='modal-footer'>");
                output.Content.AppendHtml(modalContext.Footer);
                output.Content.AppendHtmlLine("</div>");
            }

            output.Content.AppendHtmlLine("</div>\n</div>");
        }
    }

    /// <summary>
    /// Represents the body content of a modal dialog.
    /// </summary>
    /// <remarks>This tag helper is used within a <c>&lt;modal&gt;</c> element to define the content of the
    /// modal's body section. The content inside the <c>&lt;modal-body&gt;</c> element is captured and assigned to the
    /// modal's body.</remarks>
    [HtmlTargetElement("modal-body", ParentTag = "modal")]
    public class ModalBodyTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var childContent = await output.GetChildContentAsync();
            var modalContext = (ModalContext)context.Items[typeof(ModalTagHelper)];
            modalContext.Body = childContent;
            output.SuppressOutput();
        }
    }

    /// <summary>
    /// Represents the footer section of a modal dialog, allowing customization of its content and optional dismiss
    /// button.
    /// </summary>
    /// <remarks>This tag helper is used in conjunction with the <c>&lt;modal&gt;</c> tag helper to define the
    /// footer content of a modal dialog. If the <see cref="DismissText"/> property is set, a dismiss button will be
    /// added to the footer with the specified text.</remarks>
    [HtmlTargetElement("modal-footer", ParentTag = "modal")]
    public class ModalFooterTagHelper : TagHelper
    {
        public string DismissText { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var childContent = await output.GetChildContentAsync();
            var modalContext = (ModalContext)context.Items[typeof(ModalTagHelper)];

            var footerContent = new DefaultTagHelperContent();
            footerContent.AppendHtml(childContent);
            if (DismissText != null)
                footerContent.AppendFormat("<button type='button' class='btn btn-default' data-bs-dismiss='modal'>{0}</button>\n", DismissText);

            modalContext.Footer = footerContent;
            output.SuppressOutput();
        }
    }


}

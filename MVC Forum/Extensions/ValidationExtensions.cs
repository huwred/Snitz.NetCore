using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Linq;

namespace SnitzCore.Data.Extensions
{
    public static class ValidationExtensions
    {

        public static IHtmlContent MyValidationSummary(
            this IHtmlHelper htmlHelper,
            bool excludePropertyErrors,
            string message,
            object htmlAttributes)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var viewData = htmlHelper.ViewData;
            var modelState = viewData.ModelState;
            if (modelState.IsValid)
            {
                return HtmlString.Empty;
            }

            var modelErrors = excludePropertyErrors ?
                modelState[string.Empty]?.Errors :
                modelState.SelectMany(entry => entry.Value.Errors);

            if (modelErrors == null || !modelErrors.Any())
            {
                return HtmlString.Empty;
            }

            var tagBuilder = new TagBuilder("div");
            tagBuilder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            tagBuilder.AddCssClass("validation-summary-errors");

            if (!string.IsNullOrEmpty(message))
            {
                var messageSpan = new TagBuilder("span");
                messageSpan.InnerHtml.SetContent(message);
                tagBuilder.InnerHtml.AppendHtml(messageSpan);
            }

            var unorderedList = new TagBuilder("ul");
            unorderedList.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            foreach (var modelError in modelErrors)
            {
                var listItem = new TagBuilder("li");
                listItem.InnerHtml.AppendHtml(modelError.ErrorMessage);
                unorderedList.InnerHtml.AppendHtml(listItem);
            }

            tagBuilder.InnerHtml.AppendHtml(unorderedList);

            return tagBuilder;
        }

    }
}
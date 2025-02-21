using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("renderstoredcontent")]
    public class RenderStoredContentTagHelper : TagHelper
    {
        private const string KeyAttributeName = "asp-key";
        private const string _storageKey = "storecontent";

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext? ViewContext { get; set; }

        [HtmlAttributeName(KeyAttributeName)]
        public required string Key { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = String.Empty;

            var storageProvider = ViewContext!.TempData;
            Dictionary<string, List<HtmlString>>? storage;

            if (!storageProvider.ContainsKey(_storageKey) || !(storageProvider[_storageKey] is Dictionary<string, List<HtmlString>>))
            {
                return;
            }

            storage = storageProvider[_storageKey] as Dictionary<string, List<HtmlString>>;
            string html = "";

            if (String.IsNullOrEmpty(Key))
            {
                html = String.Join("", storage!.Values.SelectMany(x => x).ToList());
            }
            else
            {
                if (!storage!.ContainsKey(Key)) return;
                html = String.Join("", storage[Key]);
            }

            TagBuilder tagBuilder = new TagBuilder("dummy");
            tagBuilder.InnerHtml.AppendHtml(html) ;
            output.Content.AppendHtml(html);
        }
    } 
}

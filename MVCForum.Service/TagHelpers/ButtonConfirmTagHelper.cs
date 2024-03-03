using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SnitzCore.Service.TagHelpers;

[HtmlTargetElement("button-confirm", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ButtonConfirmTagHelper : TagHelper
{
    [HtmlAttributeName("config-key")]
    public string? Key { get; set; }
    [HtmlAttributeName("title")]
    public string? Title { get; set; }
    [HtmlAttributeName("config-class")]
    public string? TagClass { get; set; }
    [HtmlAttributeName("selector")]
    public string? Selector { get; set; }
    [HtmlAttributeName("href")]
    public string? Href { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {

        var tagid = Guid.NewGuid().ToString();
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.Add("href",Href + Key);
        output.Attributes.Add("id",tagid);
        output.Attributes.Add("title",Title);
        output.Attributes.Add("data-bs-toggle","modal");
        output.Attributes.Add("data-id",Key);
        output.Attributes.Add("rel","nofollow");
        output.AddClass("btn",HtmlEncoder.Default);
        output.AddClass("btn-outline-danger",HtmlEncoder.Default);
        output.AddClass(Selector,HtmlEncoder.Default);
        output.Content.AppendHtml($@"<i class='" + TagClass + $"'></i><span class='d-none d-md-inline'> {Title}</span>");
    }

}
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Service.TagHelpers;

/// <summary>
/// TagHelper to display member avatar
/// </summary>
[HtmlTargetElement("snitz-avatar", TagStructure = TagStructure.NormalOrSelfClosing)]
public class MemberImageTagHelper : TagHelper
{
    [HtmlAttributeName("member")]
    public string? MemberName {get;set;}
    /// <summary>
    /// Members avatar file
    /// </summary>
    [HtmlAttributeName("src")]
    public string? SourceFile { get; set; }
    /// <summary>
    /// Path to a fallback image if member has no avatar
    /// </summary>
    [HtmlAttributeName("def-src")]
    public string? Fallback { get; set; }
    /// <summary>
    /// Any extra css classes needed
    /// </summary>
    [HtmlAttributeName("class")]
    public string? Classes { get; set; }

    private readonly ISnitzConfig _config;
    public MemberImageTagHelper(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccesor, IWebHostEnvironment environment, ISnitzConfig config)
    {
        this.urlHelperFactory = urlHelperFactory;
        this.actionContextAccesor = actionContextAccesor;
        _env = environment;
        _config = config;
    }
    private readonly IWebHostEnvironment _env;
    private readonly IUrlHelperFactory urlHelperFactory;
    private readonly IActionContextAccessor actionContextAccesor;
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccesor.ActionContext!);
        if (_config.ContentFolder != "Content")
        {
            SourceFile = SourceFile?.Replace("/Content/", "/" + _config.ContentFolder + "/");
        }
        if (!File.Exists(_env.WebRootPath + SourceFile?.Replace("/", @"\").Replace("~", "")))
        {
            SourceFile = null;
        }
        base.Process(context, output);
        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "img";
        output.Attributes.Add("class", Classes);
        output.Attributes.Add("loading", "lazy");
        output.Attributes.Add("src", urlHelper.Content(SourceFile) ?? urlHelper.Content(Fallback));

        output.Attributes.Add("alt", urlHelper.Content(MemberName));

    }
}
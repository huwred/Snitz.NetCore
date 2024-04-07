using System.IO;
using System.Numerics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Interfaces;

namespace MVCForum.TagHelpers;

[HtmlTargetElement("snitz-avatar", TagStructure = TagStructure.NormalOrSelfClosing)]
public class MemberImageTagHelper : TagHelper
{
    [HtmlAttributeName("src")]
    public string? SourceFile { get; set; }
    [HtmlAttributeName("def-src")]
    public string? Fallback { get; set; }
    [HtmlAttributeName("class")]
    public string? Classes { get; set; }

    private readonly ISnitzConfig _config;
    public MemberImageTagHelper(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccesor,IWebHostEnvironment environment,ISnitzConfig config)
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
            SourceFile = SourceFile.Replace("/Content/", "/" + _config.ContentFolder + "/");
        }
        if(!File.Exists(_env.WebRootPath + urlHelper.Content(SourceFile)))
        {
            SourceFile = null;
        }
        base.Process(context, output);
        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "img";
        output.Attributes.Add("class", Classes);
        output.Attributes.Add("src", SourceFile??Fallback);


        //output.AddClass("center",HtmlEncoder.Default);
    }
}
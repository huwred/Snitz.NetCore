using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

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
    public MemberImageTagHelper(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccesor,IWebHostEnvironment environment)
    {
        this.urlHelperFactory = urlHelperFactory;
        this.actionContextAccesor = actionContextAccesor;
        _env = environment;
    }
    private readonly IWebHostEnvironment _env;
    private readonly IUrlHelperFactory urlHelperFactory;
    private readonly IActionContextAccessor actionContextAccesor;
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccesor.ActionContext!);
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
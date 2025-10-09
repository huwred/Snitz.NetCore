using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Service.TagHelpers;

/// <summary>
/// A custom <see cref="TagHelper"/> that generates an HTML <c>&lt;img&gt;</c> element for displaying a member's avatar.
/// </summary>
/// <remarks>This tag helper supports rendering a member's avatar image based on their ID or username. If no
/// avatar is available,  a fallback image can be specified. Additional attributes such as CSS classes and tooltips can
/// also be applied.</remarks>
[HtmlTargetElement("snitz-avatar", TagStructure = TagStructure.NormalOrSelfClosing)]
public class MemberImageTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the unique identifier of the member.
    /// </summary>
    public int? MemberId {get;set;}

    /// <summary>
    /// Gets or sets the name of the member associated with the HTML attribute.
    /// </summary>
    [HtmlAttributeName("member")]
    public string? MemberName {get;set;}
    /// <summary>
    /// Members avatar file
    /// </summary>
    [HtmlAttributeName("src")]
    public string? SourceFile { get; set; }
    /// <summary>
    /// Path to a fallback image if member has no Avatar
    /// </summary>
    [HtmlAttributeName("def-src")]
    public string? Fallback { get; set; }
    /// <summary>
    /// Any extra css classes needed
    /// </summary>
    [HtmlAttributeName("class")]
    public string? Classes { get; set; }

    public string? Title {get;set;}

    private readonly ISnitzConfig _config;
    private readonly IWebHostEnvironment _env;
    private readonly IUrlHelperFactory urlHelperFactory;
    private readonly IActionContextAccessor actionContextAccesor;
    private readonly IMember _member;

    public MemberImageTagHelper(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccesor, IWebHostEnvironment environment, ISnitzConfig config, IMember forummember)
    {
        this.urlHelperFactory = urlHelperFactory;
        this.actionContextAccesor = actionContextAccesor;
        _env = environment;
        _config = config;
        _member = forummember;
    }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccesor.ActionContext!);
        if(MemberId != null)
        {
            var member = _member.GetById(MemberId);
            
            if(member != null && member.PhotoUrl != null)
            {
                MemberName = member.Name;
                if (member.PhotoUrl.ToLowerInvariant().StartsWith("http"))
                {
                    SourceFile = member.PhotoUrl;
                }
                else
                {
                    SourceFile = "~/Content/Avatar/" + member.PhotoUrl;
                    var path = Path.Combine(_env.WebRootPath, _config.ContentFolder , "Avatar",member.PhotoUrl);
                    if (!File.Exists(path))
                    {
                        SourceFile = null;
                    } 
                }
           
            }
        }
        else if (MemberName != null)
        {
            var member = _member.GetByUsername(MemberName);
            if(member != null && member.PhotoUrl != null)
            {
                if (member.PhotoUrl.ToLowerInvariant().StartsWith("http"))
                {
                    SourceFile = member.PhotoUrl;
                }
                else
                {
                    SourceFile = "~/Content/Avatar/" + member.PhotoUrl;
                    var path = Path.Combine(_env.WebRootPath, _config.ContentFolder , "Avatar",member.PhotoUrl);
                    if (!File.Exists(path))
                    {
                        SourceFile = null;
                    } 
                }
            }
        }
        else if (!string.IsNullOrEmpty(SourceFile))
        {
                if (!SourceFile.ToLowerInvariant().StartsWith("http"))
                {
                    var path = Path.Combine(_env.WebRootPath, _config.ContentFolder, "Avatar", SourceFile);
                    if (!File.Exists(path))
                    {
                        SourceFile = null;
                    }
                    else
                    {
                        SourceFile = "~/Content/Avatar/" + SourceFile;
                    }
                }

        }

        if (_config.ContentFolder != "Content")
        {
            SourceFile = SourceFile?.Replace("/Content/", "/" + _config.ContentFolder + "/");
        }

        base.Process(context, output);
        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "img";
        if(Title != null)
        {
            output.Attributes.Add("title", Title);
            output.Attributes.Add("data-toggle", "tooltip");
        }
        output.Attributes.Add("class", Classes);
        output.Attributes.Add("loading", "lazy");
        output.Attributes.Add("src", urlHelper.Content(SourceFile) ?? urlHelper.Content(Fallback));

        output.Attributes.Add("alt", MemberName);

    }
}
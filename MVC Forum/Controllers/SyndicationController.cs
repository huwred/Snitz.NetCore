using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using System.IO;
using System.Text;
using System.Xml;

namespace MVCForum.Controllers
{
    public class SyndicationController : SnitzBaseController
    {
    private readonly ISyndicationXmlService _syndicationXmlService;
    private readonly XmlWriterSettings _xmlWriterSettings = new()
    {
        Encoding = Encoding.UTF8,
        NewLineHandling = NewLineHandling.Entitize,
        NewLineOnAttributes = true,
        Indent = true
    };

        public SyndicationController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory, SnitzDbContext dbContext, IHttpContextAccessor httpContextAccessor,
            ISyndicationXmlService syndicationXmlService) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _syndicationXmlService = syndicationXmlService;
        }

        [Route("forum/feed.rss")]
    [ResponseCache(Duration = 900)]
    public FileContentResult Rss()
    {
        using var stream = new MemoryStream();
        using (var xmlWriter = XmlWriter.Create(stream, _xmlWriterSettings))
        {
            var feed = _syndicationXmlService.GenerateRssXml("The MediaWizards Forums", "Latest Posts");
            feed.WriteTo(xmlWriter);
            xmlWriter.Flush();
        }
        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }

    [Route("forum/feed.atom")]
    [ResponseCache(Duration = 900)]
    public FileContentResult Atom()
    {
        using var stream = new MemoryStream();
        using (var xmlWriter = XmlWriter.Create(stream, _xmlWriterSettings))
        {
            var feed = _syndicationXmlService.GenerateAtomXml("The MediaWizards Forums", "Latest Posts");
            feed.WriteTo(xmlWriter);
            xmlWriter.Flush();
        }
        return File(stream.ToArray(), "application/atom+xml; charset=utf-8");
    }

    }
}

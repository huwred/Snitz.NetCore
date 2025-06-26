using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;

namespace MVCForum.Controllers
{
    public class RssActionResult : ActionResult
    {
        public SyndicationFeed Feed { get; set; }
        public override void ExecuteResult(ActionContext context)
        {
            if (Feed == null)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound; // Updated to use HttpStatusCode  
                return;
            }
            context.HttpContext.Response.ContentType = "application/rss+xml";

            var rssFormatter = new Atom10FeedFormatter(Feed);
            using (var writer = XmlWriter.Create(context.HttpContext.Response.Body)) // Updated to use Response.Body  
            {
                rssFormatter.WriteTo(writer);
            }
        }
    }
}
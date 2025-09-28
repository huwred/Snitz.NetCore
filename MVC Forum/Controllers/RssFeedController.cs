using BbCodeFormatter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.Extensions;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace MVCForum.Controllers
{
    public class RssFeedController : SnitzBaseController
    {
        private RssFeed feeds;
        public RssFeedController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory, SnitzDbContext dbContext, IHttpContextAccessor httpContextAccessor,ICodeProcessor bbcodeProcessor) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            feeds = new RssFeed(config,dbContext,bbcodeProcessor,memberService);
        }

        [ResponseCache(Duration = 480, Location = ResponseCacheLocation.Any)]
        public IActionResult Active()
        {
            var feed = feeds.ActiveFeed(ControllerContext.HttpContext.Request);
            if (feed == null)
            {
                return NotFound();
            }
            return new RssActionResult { Feed = feed };
        }

        [ResponseCache(Duration = 480, Location = ResponseCacheLocation.Any)]
        public IActionResult Topic(int id)
        {
            var feed = feeds.TopicFeed(id, ControllerContext.HttpContext.Request);
            if (feed == null)
            {
                return NotFound();
            }
            return new RssActionResult { Feed = feed };
        }

        [ResponseCache(Duration = 480, Location = ResponseCacheLocation.Any)]
        public IActionResult Forum(ICollection<int> id)
        {
            var feed = feeds.ForumFeed(id.ToList(), ControllerContext.HttpContext.Request);
            if (feed == null)
            {
                return NotFound();
            }
            return new RssActionResult { Feed = feed };
        }
    }
}

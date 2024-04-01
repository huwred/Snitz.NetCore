using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartBreadcrumbs.Attributes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Forum;
using MVCForum.ViewModels.Home;
using MVCForum.ViewModels.Post;

namespace MVCForum.Controllers
{
    [DefaultBreadcrumb("mnuHome")]
    public class HomeController : SnitzController
    {
        private readonly IPost _postService;

        public HomeController(IMember memberService, ISnitzConfig config,IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor, IPost postService) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _postService = postService;
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public IActionResult Index()
        {            
            if (User.Identity is { IsAuthenticated: true })
            {
                _memberService.SetLastHere(User);
            }
            try
            {
                var model = BuildHomeIndexModel();
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error("BuildHomeIndexModel",e);
                return View("TempIndex");
            }
        }

        private HomeIndexModel BuildHomeIndexModel()
        {
            var latestPosts = _postService.GetLatestPosts(_config.GetIntValue("INTRECENTCOUNT",10))
            .Select(post => new PostListingModel
            {
                Id = post.Id,
                Title = post.Title,
                Message = post.Content,
                AuthorName = post.Member?.Name ?? "Unknown",
                AuthorId = post.Member!.Id,
                //AuthorRating = post.User?.Rating ?? 0,
                Created = post.Created.FromForumDateStr(),
                LastPostDate = !post.LastPostDate.IsNullOrEmpty() ? post.LastPostDate.FromForumDateStr() : null,
                //LastPostAuthorName = post.LastPostAuthorId != null ? _memberService.GetById(post.LastPostAuthorId!.Value)?.Name : "",
                //Forum = GetForumListingForPost(post),
                RepliesCount = post.ReplyCount,
                ViewCount = post.ViewCount,
                UnmoderatedReplies = post.UnmoderatedReplies,
                IsSticky = post.IsSticky == 1,
                Status = post.Status,
                Answered = post.Answered,
                //HasPoll = _postService.HasPoll(post.Id),
            });

            return new HomeIndexModel
            {
                LatestPosts = latestPosts,
                SearchQuery = ""
            };
        }

        private ForumListingModel GetForumListingForPost(Post post)
        {
            var forum = post.Forum;

            return new ForumListingModel
            {
                Id = forum!.Id,
                Title = forum.Title,
                AccessType = forum.Privateforums,
                ForumType = (ForumType)forum.Type,
                Url = forum.Url,
                DefaultView = (DefaultDays)forum.Defaultdays,
                ForumModeration = forum.Moderation,
                Polls = forum.Polls
                //ImageUrl = forum.ImageUrl
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult CookiePolicy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SetLanguage(string lang, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMonths(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}

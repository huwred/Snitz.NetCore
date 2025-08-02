using Microsoft.AspNetCore.Mvc;
using MVCForum.ViewModels.Post;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MVCForum.View_Components
{
    public class WidgetsViewComponent : ViewComponent
    {
        private readonly IPost _postService;
        private readonly ISnitzConfig _config;
        public WidgetsViewComponent(IPost postService,ISnitzConfig snitzConfig)
        {
            _postService = postService;
            _config = snitzConfig;
        }
        public async Task<IViewComponentResult> InvokeAsync(string template, string width="col-3", string[]? widgets = null, int forumid = 0)
        {
            if(widgets == null || widgets.Length == 0)
            {
                // If no widgets are provided, return an empty view
                return Content(string.Empty);
            }
            var vm = new SidebarViewModel
            {
                Width = width,
                Options = widgets,
                ForumId = forumid
            };
            if (widgets.Contains("latest"))
            {
                vm.LatestPosts = _postService.GetLatestPosts(_config.GetIntValue("INTRECENTCOUNT", 10))
                .Select(post => new PostListingModel
                {
                    Id = post.Id,
                    Topic = post,
                    Title = post.Title,
                    Message = post.Content,
                    AuthorName = post.Member?.Name ?? "Unknown",
                    AuthorId = post.Member!.Id,
                    Created = post.Created.FromForumDateStr(),
                    LastPostDate = !string.IsNullOrEmpty(post.LastPostDate) ? post.LastPostDate.FromForumDateStr() : null,
                    RepliesCount = post.ReplyCount,
                    ViewCount = post.ViewCount,
                    UnmoderatedReplies = post.UnmoderatedReplies,
                    IsSticky = post.IsSticky == 1,
                    Status = post.Status,
                    Answered = post.Answered,
                });
            }
            return await Task.FromResult((IViewComponentResult)View(template,vm));
        }
    }

    public class  SidebarViewModel
    {
        public string[]? Options;
        public string Width = "col-3";
        public int ForumId;
        public IEnumerable<PostListingModel>? LatestPosts;
    }
}

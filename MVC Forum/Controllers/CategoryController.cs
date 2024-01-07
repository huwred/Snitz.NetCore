using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.Tokens;
using MVCForum.Models.Forum;
using MVCForum.Models.Post;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Linq;

namespace MVCForum.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IForum _forumService;
        private readonly IMember _memberService;
        private readonly IPost _postService;
        private readonly ISnitzConfig _config;
        private readonly ISnitzCookie _cookie;

        public CategoryController(IForum forumService,IMember memberService,IPost postService, ISnitzConfig config,ISnitzCookie snitzCookie)
        {
            _forumService = forumService;
            _memberService = memberService;
            _postService = postService;
            _config = config;
            _cookie = snitzCookie;
        }
        [Breadcrumb("Forums",FromAction = "Index",FromController = typeof(HomeController))]
        //[OutputCache]
        public IActionResult Index(int id)
        {
            _memberService.SetLastHere(User);
           
            var forums = _forumService.GetAll().Select(forum => new ForumListingModel()
            {
                Id = forum.Id,
                Title = forum.Title,
                Description = forum.Description,
                CategoryId = forum.CategoryId,
                CategoryName = forum.Category!.Name!,
                Topics = forum.TopicCount,
                Posts = forum.ReplyCount,
                DefaultView = (DefaultDays)forum.Defaultdays,
                LastPostDateTime = !forum.LastPost.IsNullOrEmpty() ? forum.LastPost.FromForumDateStr() : null,
                LastPostAuthorId = forum.LastPostAuthorId,
                LastPostTopicId = forum.LatestTopicId,
                LastPostReplyId = forum.LatestReplyId,
                LastPostAuthor = _memberService.GetById(forum.LastPostAuthorId).Result,
                AccessType = forum.Privateforums,
                ForumType = (ForumType)forum.Type,
                Url = forum.Url
                
            });
            if (id > 0)
            {
                forums = forums.Where(f => f.CategoryId == id);
                var forumPage = new MvcBreadcrumbNode("", "Category", "Forums");
                var topicPage = new MvcBreadcrumbNode("", "Category", forums.First().CategoryName) { Parent = forumPage,RouteValues = new{id=forums.First().CategoryId}};
                ViewData["BreadcrumbNode"] = topicPage; 
            }
            var latestPosts = _postService.GetLatestPosts(10)
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
                LastPostAuthorName = _memberService.GetById(post.LastPostAuthorId).Result?.Name,
                Forum = GetForumListingForPost(post),
                RepliesCount = post.ReplyCount,
                ViewCount = post.ViewCount,
                IsSticky = post.IsSticky == 1,
                Status = post.Status
            });
            var model = new ForumIndexModel()
            {
                ForumList = forums,
                LatestPosts = latestPosts
            };
            return View("index",model);
        }
        private ForumListingModel GetForumListingForPost(Post post)
        {
            Forum forum = post.Forum!;

            return new ForumListingModel
            {
                Id = forum.Id,
                Title = forum.Title,
                AccessType = forum.Privateforums,
                ForumType = (ForumType)forum.Type,
                Url = forum.Url,
                DefaultView = (DefaultDays)forum.Defaultdays,
                CategoryId = forum.CategoryId,
                //ImageUrl = forum.ImageUrl
            };
        }

        public IActionResult Edit(int id)
        {
            throw new System.NotImplementedException();
        }

        public IActionResult Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
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
using MVCForum.Models.Category;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class CategoryController : Controller
    {
        private readonly IForum _forumService;
        private readonly IMember _memberService;
        private readonly IPost _postService;
        private readonly ISnitzConfig _config;
        private readonly ISnitzCookie _cookie;
        private readonly ICategory _categoryService;

        
        public CategoryController(IForum forumService,IMember memberService,IPost postService, ISnitzConfig config,ISnitzCookie snitzCookie,ICategory categoryService)
        {
            _forumService = forumService;
            _memberService = memberService;
            _postService = postService;
            _config = config;
            _cookie = snitzCookie;
            _categoryService = categoryService;
        }
        [Breadcrumb("Forums",FromAction = "Index",FromController = typeof(HomeController))]
        
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
            else
            {
                var forumPage = new MvcBreadcrumbNode("", "Category", "Forums");
                ViewData["BreadcrumbNode"] = forumPage;
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

        //[Breadcrumb("Forums",FromAction = "Index",FromController = typeof(HomeController))]
        public IActionResult Edit(int id)
        {
            var cat = _categoryService.GetById(id);
            var forumPage = new MvcBreadcrumbNode("", "Category", "Categories");
            var topicPage = new MvcBreadcrumbNode("", "Category", "New Category") { Parent = forumPage};
            CategoryViewModel vm = new CategoryViewModel();

            if (id > 0)
            {
                vm.Id = cat.Id;
                vm.Name = cat.Name!;
                vm.Status = (Status)cat.Status!;
                vm.Subscription = (CategorySubscription)cat.Subscription!;
                vm.Moderation = (ModerationLevel)cat.Moderation!;
                vm.Sort = cat.Sort;
                topicPage = new MvcBreadcrumbNode("", "Category", cat.Name) { Parent = forumPage};

            }
            ViewData["BreadcrumbNode"] = topicPage;
            return View("CreateEdit",vm);
        }
        public IActionResult Create()
        {
            var forumPage = new MvcBreadcrumbNode("", "Category", "Categories");
            var topicPage = new MvcBreadcrumbNode("", "Category", "New Category") { Parent = forumPage};
            CategoryViewModel vm = new CategoryViewModel();

            ViewData["BreadcrumbNode"] = topicPage;
            return View("CreateEdit",vm);
        }
        public IActionResult Delete(int id)
        {
            return RedirectToAction("Index","Category");;
        }
    }
}

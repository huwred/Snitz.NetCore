using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Http;
using MVCForum.ViewModels.Forum;
using MVCForum.ViewModels.Category;
using MVCForum.ViewModels.Post;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class CategoryController : SnitzController
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly ICategory _categoryService;

        public CategoryController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IForum forumService, IPost postService, ICategory categoryService) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _forumService = forumService;
            _postService = postService;
            _categoryService = categoryService;
        }

        
        [Breadcrumb("AllForums",FromAction = "Index",FromController = typeof(HomeController))]
        [Route("Forums")]
        [Route("Category/{id?}")]
        [Route("Category/Index/{id}")]
        public IActionResult Index(int id)
        {
            if (User.Identity is { IsAuthenticated: true })
            {
                _memberService.SetLastHere(User);
            }
            var forums = _forumService.GetAll().Select(forum => new ForumListingModel()
            {
                Id = forum.Id,
                Title = forum.Title,
                Description = forum.Description,
                CategoryId = forum.CategoryId,
                CategoryName = forum.Category?.Name,
                Topics = forum.TopicCount,
                Posts = forum.ReplyCount,
                DefaultView = (DefaultDays)forum.Defaultdays,
                LastPostDateTime = !forum.LastPost.IsNullOrEmpty() ? forum.LastPost.FromForumDateStr() : null,
                LastPostAuthorId = forum.LastPostAuthorId,
                LastPostTopicId = forum.LatestTopicId,
                LastPostReplyId = forum.LatestReplyId,
                LastPostAuthor = forum.LastPostAuthorId != null && forum.LastPostAuthorId != 0 ? _memberService.GetById(forum.LastPostAuthorId) : null,
                AccessType = forum.Privateforums,
                ForumType = (ForumType)forum.Type,
                Url = forum.Url,
                Status = forum.Status
                
            }).ToList();
            if (id > 0)
            {
                forums = forums.Where(f => f.CategoryId == id).ToList();
                var forumPage = new MvcBreadcrumbNode("Forums", "Category", "ttlForums");
                var topicPage = new MvcBreadcrumbNode("", "Category", forums.First().CategoryName) { Parent = forumPage,RouteValues = new{id=forums.First().CategoryId}};
                ViewData["BreadcrumbNode"] = topicPage; 
            }
            else
            {
                var forumPage = new MvcBreadcrumbNode("", "Category", "ttlForums");
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
                LastPostAuthorName = post.LastPostAuthorId != null ? _memberService.GetById(post.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = post.LastPostReplyId,
                Forum = GetForumListingForPost(post),
                RepliesCount = post.ReplyCount,
                ViewCount = post.ViewCount,
                IsSticky = post.IsSticky == 1,
                Status = post.Status,
                Answered = post.Answered
            });
            var model = new ForumIndexModel()
            {
                ForumList = forums,
                LatestPosts = latestPosts
            };
                return View("Index",model);
        }

        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            var forumPage = new MvcBreadcrumbNode("", "Category", "Categories");
            var topicPage = new MvcBreadcrumbNode("", "Category", "New Category") { Parent = forumPage};
            CategoryViewModel vm = new CategoryViewModel();

            ViewData["BreadcrumbNode"] = topicPage;
            return View("CreateEdit",vm);
        }
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [Route("Category/CreateEdit")]
        public IActionResult CreateEdit(CategoryViewModel model)
        {
            var forumPage = new MvcBreadcrumbNode("", "Category", "Categories");
            var topicPage = new MvcBreadcrumbNode("", "Category", "New Category") { Parent = forumPage};
            ViewData["BreadcrumbNode"] = topicPage;
            if (ModelState.IsValid)
            {
                Category newCategory = new()
                {
                    Name = model.Name, 
                    Status = (short)model.Status,
                    Sort = model.Sort,
                    Moderation = (int)model.Moderation,
                    Subscription = (int)(model.Subscription)
                };
                if (model.Id != 0)
                {
                    newCategory.Id = model.Id;
                    _categoryService.Update(newCategory);
                }
                else
                {
                    _categoryService.Create(newCategory);
                }
                return RedirectToAction("Index","Category",new{id = newCategory.Id});
            }


            return View("CreateEdit",model);
        }
        [Authorize(Roles = "Administrator")]
        public IActionResult Delete(int id)
        {
            _categoryService.Delete(id);
            return Json(new { redirectToUrl = Url.Action("Index", "Category") });

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
                Status = forum.Status
                //ImageUrl = forum.ImageUrl
            };
        }


        public async Task<IActionResult> EmptyCategory(int id)
        {
            await _categoryService.DeleteForums(id);
            return Json(new { redirectToUrl = "/Category" });
        }

    }
}

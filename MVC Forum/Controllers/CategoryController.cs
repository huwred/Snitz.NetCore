using Microsoft.AspNetCore.Mvc;
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
using Microsoft.EntityFrameworkCore;
using MVCForum.ViewModels.Forum;
using MVCForum.ViewModels.Category;
using MVCForum.ViewModels.Post;
using System.Net;
using System.Text.RegularExpressions;
using System;
using Microsoft.Extensions.Hosting;
using SnitzCore.Service;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class CategoryController : SnitzBaseController
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly ICategory _categoryService;
        private readonly ISnitzCookie _cookie;
        private readonly IGroups _groupservice;

        public CategoryController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IForum forumService, IPost postService,ISnitzCookie snitzCookie, ICategory categoryService,IGroups groups) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _forumService = forumService;
            _postService = postService;
            _categoryService = categoryService;
            _cookie = snitzCookie;
            _groupservice = groups;
        }

        
        [Breadcrumb("AllForums",FromAction = "Index",FromController = typeof(HomeController))]
        [Route("AllForums")]
        [Route("Category/{id?}")]
        [Route("Category/Index/{id}")]
        public IActionResult Index(int id, int groupId = 0)
        {
            if (_config.GetIntValue("STRGROUPCATEGORIES") ==1)
            {
                if (groupId == 0)
                {
                    var groupcookie = _cookie.GetCookieValue("GROUP");
                    if (groupcookie != null)
                    {
                        groupId = Convert.ToInt32(groupcookie);
                    }
                }
                _cookie.SetCookie("GROUP", groupId.ToString());                
            }
            ViewBag.GroupId = groupId;

            var categories = _categoryService.GetAll().OrderBy(c=>c.Sort).ToList();
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
                LastPostDateTime = !string.IsNullOrEmpty(forum.LastPost) ? forum.LastPost.FromForumDateStr() : null,
                LastPostAuthorId = forum.LastPostAuthorId,
                LastPostTopicId = forum.LatestTopicId,
                LastPostReplyId = forum.LatestReplyId,
                AccessType = forum.Privateforums,
                ForumModeration = forum.Moderation,
                ForumType = (ForumType)forum.Type,
                Url = forum.Url,
                Status = forum.Status,
                Order = forum.Order,
                CategorySubscription = (CategorySubscription)forum.Category?.Subscription,
                ForumSubscription = (ForumSubscription)forum.Subscription,
                ArchivedCount = forum.ArchivedTopics
                
            });            
            
            if(groupId > 1)
            {
                var catfilter = _groupservice.GetGroups(groupId).Select(g=>g.CategoryId).ToList();
                categories = categories
                .Where(f =>  catfilter.Contains(f.Id))
                .OrderBy(c=>c.Sort).ToList();

                forums = _forumService.GetAll().Where(f=>catfilter.Contains(f.CategoryId)).Select(forum => new ForumListingModel()
                {
                    Id = forum.Id,
                    Title = forum.Title,
                    Description = forum.Description,
                    CategoryId = forum.CategoryId,
                    CategoryName = forum.Category?.Name,
                    Topics = forum.TopicCount,
                    Posts = forum.ReplyCount,
                    DefaultView = (DefaultDays)forum.Defaultdays,
                    LastPostDateTime = !string.IsNullOrEmpty(forum.LastPost) ? forum.LastPost.FromForumDateStr() : null,
                    LastPostAuthorId = forum.LastPostAuthorId,
                    LastPostTopicId = forum.LatestTopicId,
                    LastPostReplyId = forum.LatestReplyId,
                    AccessType = forum.Privateforums,
                    ForumModeration = forum.Moderation,
                    ForumType = (ForumType)forum.Type,
                    Url = forum.Url,
                    Status = forum.Status,
                    Order = forum.Order,
                    CategorySubscription = (CategorySubscription)forum.Category?.Subscription,
                    ForumSubscription = (ForumSubscription)forum.Subscription,
                    ArchivedCount = forum.ArchivedTopics
                
                });

            }


            if (id > 0)
            {
                categories = categories.Where(f => f.Id == id).ToList();
                //forums = forums.Where(f => f.CategoryId == id).ToList();
                var forumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
                var topicPage = new MvcBreadcrumbNode("", "Category", categories?.OrderBy(c => c.Name)?.FirstOrDefault()?.Name) { Parent = forumPage,RouteValues = new{id=id}};
                ViewData["BreadcrumbNode"] = topicPage; 
            }
            else
            {
                var forumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
                ViewData["BreadcrumbNode"] = forumPage;
            }
            
            var latestPosts = _postService.GetLatestPosts(_config.GetIntValue("INTRECENTCOUNT",10))
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
                LatestReply = post.LastPostReplyId,
                Forum = id > 0 ? GetForumListingForPost(post) : null,
                RepliesCount = post.ReplyCount,
                ViewCount = post.ViewCount,
                UnmoderatedReplies = post.UnmoderatedReplies,
                IsSticky = post.IsSticky == 1,
                Status = post.Status,
                Answered = post.Answered,
                AllowRating = post.AllowRating,
                ForumAllowRating = post.Forum.Rating
            });
            var model = new ForumIndexModel()
            {
                Categories = categories,
                ForumList = forums,
                LatestPosts = latestPosts
            };
                return View("Index",model);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cat = _categoryService.GetById(id);
            var forumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
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
        [HttpGet]
        [Route("Category/Create")]
        public IActionResult Create()
        {
            var forumPage = new MvcBreadcrumbNode("", "Category", _languageResource.GetString("lblCategories"));
            var topicPage = new MvcBreadcrumbNode("", "Category", _languageResource.GetString("tipNewCategory")) { Parent = forumPage};

            ViewData["BreadcrumbNode"] = topicPage;
            return View("CreateEdit",new CategoryViewModel());
        }
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [Route("Category/CreateEdit")]
        public IActionResult CreateEdit(CategoryViewModel model)
        {
            var forumPage = new MvcBreadcrumbNode("", "Category", _languageResource.GetString("lblCategories"));
            var topicPage = new MvcBreadcrumbNode("", "Category", _languageResource.GetString("tipNewCategory")) { Parent = forumPage};
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
        [HttpGet]
        [Authorize]
        [Route("Category/Subscribe/")]
        public IActionResult Subscribe(int id)
        {
            var category = _categoryService.GetById(id);
            var member = _memberService.Current();
            if(member == null)
            {
                return Content("Error");
            }
            _snitzDbContext.MemberSubscription.Add(new MemberSubscription()
            {
                MemberId = member.Id,
                CategoryId = category.Id,
                ForumId = 0,
                PostId = 0
            });
            _snitzDbContext.SaveChanges();
            return Content("OK");
        }
        [HttpGet]
        [Authorize]
        [Route("Category/UnSubscribe/")]
        public IActionResult UnSubscribe(int id)
        {
            var member = _memberService.Current();
            if(member == null)
            {
                return Content("Error");
            }
            _snitzDbContext.MemberSubscription.Where(s => s.MemberId == member.Id && s.CategoryId == id && s.ForumId == 0)
                .ExecuteDelete();
            return Content("OK");
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
                Status = forum.Status,
                ForumModeration = forum.Moderation,
                Polls = forum.Polls,
                ArchivedCount = forum.ArchivedTopics
            };
        }


        public async Task<IActionResult> EmptyCategory(int id)
        {
            await _categoryService.DeleteForums(id);
            return Json(new { redirectToUrl = "~/Category" });
        }

    }
}

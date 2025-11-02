using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using MVCForum.ViewModels.Category;
using MVCForum.ViewModels.Forum;
using MVCForum.ViewModels.Post;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        //[ResponseCache(Duration = 240, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["id"])]
        public IActionResult Index(int id, int groupId = -1)
        {
            if (_config.IsEnabled("STRGROUPCATEGORIES"))
            {

                if(groupId == -1)
                {
                    var defId = _groupservice.GetNames().SingleOrDefault(n=>n.Name == "Default Categories")?.Id;
                    groupId = defId ?? 0;
                    if(groupId == 0)
                    {
                        var groupcookie = _cookie.GetCookieValue("GROUP");
                        if (groupcookie != null)
                        {
                            groupId = Convert.ToInt32(groupcookie);
                        }
                    }
                }

                _cookie.SetCookie("GROUP", groupId.ToString(), DateTime.UtcNow.AddMonths(1));                
            }
            ViewBag.GroupId = groupId;

            var categories = _categoryService.GetAll().ToList();

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
                CategorySubscription = forum.Category?.Subscription != null ? (CategorySubscription)forum.Category.Subscription : null,
                ForumSubscription = (ForumSubscription)forum.Subscription,
                ArchivedCount = forum.ArchivedTopics,
                PostAuth = forum.Postauth != null ? (PostAuthType)forum.Postauth : PostAuthType.Anyone,
                ReplyAuth = forum.Replyauth != null ? (PostAuthType)forum.Replyauth : PostAuthType.Anyone,               
            });            
            
            if(groupId > 1)
            {
                var catfilter = _groupservice.GetGroups(groupId).Select(g=>g.CategoryId).ToList();
                categories = categories
                .Where(f =>  catfilter.Contains(f.Id)).ToList();

                forums = forums.Where(f=>catfilter.Contains(f.CategoryId));

            }


            if (id > 0)
            {
                categories = categories.Where(f => f.Id == id).ToList();
                if (!categories.Any())
                {
                    ViewBag.Error = "Category not found";
                    return View ("Error");
                }
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

            var model = new ForumIndexModel()
            {
                Categories = categories,
                ForumList = forums
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
            return View("CreateEdit",new CategoryViewModel(){Status = Status.Closed});
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
                    Subscription = (int)(model.Subscription),
                    Forums = new List<Forum>()
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
                CacheProvider.Remove("CatList");
                return RedirectToAction("Index","Category",new{id = newCategory.Id});
            }
            else
            {
                _logger.Error("Validation errors creating/editing category");
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
            _snitzDbContext.MemberSubscriptions.Add(new MemberSubscription()
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
            _snitzDbContext.MemberSubscriptions.Where(s => s.MemberId == member.Id && s.CategoryId == id && s.ForumId == 0)
                .ExecuteDelete();
            return Content("OK");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EmptyCategory(int id)
        {
            await _categoryService.DeleteForums(id);
            return Json(new { redirectToUrl = "~/Category" });
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MVCForum.Models.Post;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class TopicController : Controller
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        private readonly IMember _memberService;
        private readonly UserManager<ForumUser> _userManager;
        public TopicController(IPost postService, IForum forumService, UserManager<ForumUser> userManager,IMember memberService)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _memberService = memberService;
        }

        public IActionResult Index(int id,int page = 1, int pagesize = 20, string sortdir="asc", int? replyid = null)
        {
            var post = _postService.GetTopicWithRelated(id);
            if (!HttpContext.Session.Keys.Contains("TopicId_"+ id))
            {
                HttpContext.Session.SetInt32("TopicId_"+ id,1);
                post.ViewCount += 1;
                _postService.UpdateViewCount(post.Id);
            }


            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var catPage = new MvcBreadcrumbNode("", "Category", post.Category?.Name){ Parent = homePage,RouteValues = new{id=post.Category?.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", post.Forum?.Title){ Parent = catPage,RouteValues = new{id=post.ForumId}};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", post.Title) { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;
            
            ViewData["Title"] = post.Title;
            var totalCount = post.Replies?.Count();
            var pageCount = (int)Math.Ceiling((double)totalCount / pagesize);

            PagedList<PostReply> pagedReplies = PagedReplies(page, pagesize, sortdir, post);
            //todo: if we have a replyid, is it in the current set, otherwise skip forwards
            if (replyid.HasValue)
            {
                while (pagedReplies.All(p => p.Id != replyid))
                {
                    page += 1;
                    pagedReplies = PagedReplies(page, pagesize, sortdir, post);                    
                }
            }

            var replies = BuildPostReplies(pagedReplies);
            var model = new PostIndexModel()
            {
                Id = post.Id,
                Title = post.Title,
                Author = post.Member,
                AuthorId = post.Member.Id,
                Views = post.ViewCount,
                IsLocked = post.Status == 0,
                //AuthorRating = post.User?.Rating ?? 0,
                AuthorName = post.Member.Name ?? "Unknown",
                //AuthorImageUrl = post.User?.ProfileImageUrl ?? "/images/avatar.png",
                Created = post.Created.FromForumDateStr(),
                Content = post.Content,
                Replies = replies,
                ForumId = post.Forum.Id,
                ForumName = post.Forum.Title,
                PageNum = page,
                PageCount = pageCount,
                SortDir = sortdir
            };
            var routeValuesDictionary = new RouteValueDictionary
            {
                { "page", page }
            };
            return View(model);
        }

        [Authorize]
        public IActionResult Create(int id)
        {
            var member = _memberService.GetById(User).Result;
            var forum = _forumService.GetById(id);
            var model = new NewPostModel()
            {
                Id = 0,
                ForumName = forum.Title,
                ForumId = id,
                IsPost = true,
                AuthorName = User.Identity?.Name,
                UseSignature = member.SigDefault == 1,
                Lock = false,
                Sticky = false,
                DoNotArchive = false,
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category.Name){ Parent = homePage,RouteValues = new{id=forum.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Create", "Topic", "New Post") { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;

            return View(model);
        }
        [Authorize]
        public IActionResult Quote(int id)
        {

            var member = _memberService.GetById(User).Result;
            var topic = _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetById(topic.ForumId);
            var model = new NewPostModel()
            {
                TopicId = id,
                ForumName = forum.Title,
                ForumId = topic.ForumId,
                IsPost = false,
                Content = $"[quote]{topic.Content}[/quote=Originally posted by {topic.Member.Name}]",
                AuthorName = member.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 1,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category.Name){ Parent = homePage,RouteValues = new{id=forum.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Reply with Quote") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public IActionResult Edit(int id)
        {

            var member = _memberService.GetById(User).Result;
            var topic = _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetById(topic.ForumId);
            var model = new NewPostModel()
            {
                Id = id,
                TopicId = id,
                ForumName = forum.Title,
                ForumId = topic.ForumId,
                Title = topic.Title,
                IsPost = true,
                Content = topic.Content,
                AuthorName = member.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = topic.Created.FromForumDateStr(),
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category.Name){ Parent = homePage,RouteValues = new{id=forum.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Edit", "Topic", "Edit Post") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public IActionResult QuoteReply(int id)
        {

            var member = _memberService.GetById(User).Result;
            var reply = _postService.GetReply(id);
            
            var topic = _postService.GetTopicWithRelated(reply.PostId);
            var model = new NewPostModel()
            {
                Id = 0,
                TopicId = topic.Id,
                ForumName = topic.Forum.Title,
                ForumId = topic.ForumId,
                IsPost = false,
                Content = $"[quote]{reply.Content}[/quote=Originally posted by {reply.Member.Name}]<br/>",
                AuthorName = member.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 1,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = DateTime.UtcNow
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var catPage = new MvcBreadcrumbNode("", "Category", topic.Category.Name){ Parent = homePage,RouteValues = new{id=topic.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", topic.Forum.Title){ Parent = catPage,RouteValues = new{id=topic.Forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Reply with Quote") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public IActionResult EditReply(int id)
        {

            var member = _memberService.GetById(User).Result;
            var reply = _postService.GetReply(id);
            var topic = _postService.GetTopicWithRelated(reply.PostId);
            var model = new NewPostModel()
            {
                Id = id,
                TopicId = topic.Id,
                ForumName = topic.Forum.Title,
                ForumId = topic.ForumId,
                IsPost = false,
                Content = reply.Content,
                AuthorName = member.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = reply.Created.FromForumDateStr(),
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var catPage = new MvcBreadcrumbNode("", "Category", topic.Category.Name){ Parent = homePage,RouteValues = new{id=topic.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", topic.Forum.Title){ Parent = catPage,RouteValues = new{id=topic.Forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Edit Reply") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPost(NewPostModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            var post = BuildPost(model, user.MemberId);
            if (model.TopicId != 0)
            {
                post.Created = model.Created.ToForumDateStr();
                post.LastEdit = DateTime.UtcNow.ToForumDateStr();
                post.LastEditby = user.MemberId;
                await _postService.Update(post);
            }
            else
            {
                await _postService.Create(post);
            }
            

            // TODO: Implement User Rating Management
            return RedirectToAction("Index", "Topic", new { id = post.Id });
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReply(NewPostModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var post = BuildReply(model, user.MemberId);
            if (model.Id != 0)
            {
                post.Created = model.Created.ToForumDateStr();
                post.LastEdited = DateTime.UtcNow.ToForumDateStr();
                post.LastEditby = user.MemberId;
                await _postService.Update(post);
            }
            else
            {
                await _postService.Create(post);
            }
            if (model.Lock || model.Sticky)
            {
                //TODO: update topic status
            }
            // TODO: Implement User Rating Management
            return RedirectToAction("Index", "Topic", new { id = post.PostId });
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteReply(int id)
        {

            var member = _memberService.GetById(User).Result;
            var post = _postService.GetReply(id);
            //if this isn't the last post then can't delete it
            if ((post.MemberId == member.Id && post.Topic.LastPostReplyId != id) && !member.Roles.Contains("Admin"))
            {
                ModelState.AddModelError("","Unable to delete this reply");
                return Json(new { result = false, error = "Unable to delete this reply" });

            }
            await _postService.DeleteReply(id);
            return Json(new { result = true, data = post.Topic.Id });

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var member = _memberService.GetById(User).Result;
            var post = _postService.GetTopicWithRelated(id);
            if (member.Roles.Contains("Admin") || post.MemberId == member.Id)
            {
                await _postService.DeleteTopic(id);
                return RedirectToAction("Index","Forum",new{id=post.ForumId});

            }
            
            return Json(new { result = false, error = "Error Deleting Topic" });

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LockTopic(int id, int status)
        {

            var member = _memberService.GetById(User).Result;

            if (!member.Roles.Contains("Admin"))
            {
                ModelState.AddModelError("","Unable to lock this reply");
                return Json(new { result = false, error = "Unable to lock Post" });

            }

            var result = await _postService.LockTopic(id, (short)status);
            return result ? Json(new { result = result, data = id }) : Json(new { result = result, error = "Unable to toggle Status" });
            
        }
        public IActionResult Print(int id)
        {
            throw new NotImplementedException();
        }
        public IActionResult Send(int id)
        {
            throw new NotImplementedException();
        }

        private Post BuildPost(NewPostModel model, int memberid)
        {
            var forum = _forumService.GetById(model.ForumId);

            return new Post()
            {
                Id = model.TopicId,
                Title = model.Title,
                Content = model.Content,
                Created = DateTime.UtcNow.ToForumDateStr(),
                MemberId = memberid,
                //Member = user,
                ForumId = model.ForumId,
                CategoryId = forum.CategoryId,
                //Forum = forum,
                IsSticky = (short)(model.Sticky ? 1 : 0),
                ArchiveFlag = model.DoNotArchive ? 1 : 0,
                Status = (short)(model.Lock ? 0 : 1),
                Sig = (short)(model.UseSignature ? 1 : 0),
            };

        }
        private PostReply BuildReply(NewPostModel model, int memberid)
        {
            var topic = _postService.GetTopicWithRelated(model.TopicId);

            return new PostReply()
            {
                Id = model.Id,
                Content = model.Content,
                Created = DateTime.UtcNow.ToForumDateStr(),
                Status = (short)(model.Lock ? 0 : 1),
                MemberId = memberid,
                PostId = topic.Id,
                ForumId = topic.ForumId,
                CategoryId = topic.CategoryId,
                Sig = (short)(model.UseSignature ? 1 : 0),
            };

        }
        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<PostReply> replies)
        {
            return replies.Select(reply => new PostReplyModel()
            {
                Id = reply.Id,
                AuthorId = reply.Member.Id,
                Author = reply.Member,
                AuthorName = reply.Member.Name,
                //AuthorImageUrl = reply.User.ProfileImageUrl,
                Created = reply.Created.FromForumDateStr(),
                Content = reply.Content,
                AuthorPosts = reply.Member.Posts,
                AuthorRole = reply.Member.Level
            });
        }
        private PagedList<PostReply> PagedReplies(int page, int pagesize, string sortdir, Post post)
        {
            PagedList<PostReply> pagedReplies = sortdir == "asc"
                ? new PagedList<PostReply>(post.Replies.OrderBy(r => r.Created), page, pagesize)
                : new PagedList<PostReply>(post.Replies.OrderByDescending(r => r.Created), page, pagesize);
            return pagedReplies;
        }

    }
}

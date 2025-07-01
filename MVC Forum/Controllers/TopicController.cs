using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Localization;
using Snitz.PhotoAlbum.ViewModels;
using MVCForum.ViewModels.Post;
using MVCForum.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading;
using BbCodeFormatter;
using Hangfire;
using System.Net;
using System.Text.RegularExpressions;
using Snitz.Events.Models;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class TopicController : SnitzBaseController
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        private readonly UserManager<ForumUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _mailSender;
        private readonly ISubscriptions _processSubscriptions;
        private readonly ICodeProcessor _bbcodeProcessor;
        private readonly HttpContext? _httpcontext;
        private readonly IPrivateMessage _pmService;
        private readonly EventContext _eventsContext;

        public TopicController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IPost postService, IForum forumService, UserManager<ForumUser> userManager,IWebHostEnvironment environment,
            IEmailSender mailSender, ISubscriptions processSubscriptions, ICodeProcessor bbcodeProcessor,
            IPrivateMessage pmService,EventContext eventsContext) : base(memberService, config, localizerFactory,dbContext, httpContextAccessor)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _environment = environment;
            _mailSender = mailSender;
            _processSubscriptions = processSubscriptions;
            _bbcodeProcessor = bbcodeProcessor;
            _httpcontext = httpContextAccessor.HttpContext;
            _pmService = pmService;
            _eventsContext = eventsContext;
        }

        //[Route("{id:int}")]
        [Route("Topic/{id}")]
        [Route("Topic/Index/{id}")]
        [Route("Topic/Posts/{id}")]
        public IActionResult Index(int id,int page = 1, int pagesize = 0, string sortdir="", int? replyid = null)
        {
            if(sortdir == "")
            {
                sortdir = _config.GetValueWithDefault("STRTOPICSORT","asc");
            }
            if(TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
                if((string)TempData["Error"]! == "floodcheck")
                {
                    var timeout = _config.GetIntValue("STRFLOODCHECKTIME", 30);
                    ViewBag.Error = _languageResource.GetString("FloodcheckErr", timeout);
                }
                TempData["Error"] = null;
                return View("Error");
            }
            bool signedin = false;
            ViewBag.RequireAuth = false;
            if (User.Identity is { IsAuthenticated: true })
            {
                signedin = true;
            }
            var post = _postService.GetTopicAsync(id).Result;
            if(post == null)
            {
                ViewBag.Error = "No Topic Found with that ID";
                return View("Error");
            }    
            //if we have a replyid, does it exist in ths topic?
            if (replyid.HasValue && post.Replies.Any())
            {
                //no reply in that topic, so reset the jumpto replyid
                if (!post.Replies.Any(r => r.Id == replyid))
                {
                    replyid = null;
                }
            }
            var haspoll = _postService.HasPoll(id);

            bool passwordrequired = false;
            bool notallowed = false;
            bool ismoderator = User.IsInRole($"Forum_{post.ForumId}");
            bool isadministrator = User.IsInRole("Administrator");

            notallowed = CheckAuthorisation(post.Forum!.Privateforums, signedin, ismoderator, isadministrator, ref passwordrequired);
            if (!isadministrator && passwordrequired)
            {
                var auth = _httpcontext!.Session.GetString("Pforum_" + post.ForumId) == null ? "" : _httpcontext.Session.GetString("Pforum_" + post.ForumId);
                if (auth != post.Forum.Password)
                {
                    ViewBag.RequireAuth = true;
                }
            }

            if (post.ReplyCount > 0 || post.UnmoderatedReplies > 0)
            {
                post = _postService.GetTopicWithRelated(id).Result;
            }
            if (!HttpContext.Session.Keys.Contains("TopicId_"+ id))
            {
                HttpContext.Session.SetInt32("TopicId_"+ id,1);
                post!.ViewCount += 1;
                _postService.UpdateViewCount(post.Id);
            }

            if (HttpContext.Session.GetInt32("TopicPageSize") != null && pagesize == 0)
            {
                pagesize = HttpContext.Session.GetInt32("TopicPageSize")!.Value;
            }
            else if (pagesize == 0)
            {
                pagesize = 10;
            }
            HttpContext.Session.SetInt32("TopicPageSize",pagesize);

            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", post!.Category?.Name){ Parent = homePage,RouteValues = new{id=post.Category?.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", post.Forum?.Title){ Parent = catPage,RouteValues = new{id=post.ForumId}};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", post.Title) { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;
            
            ViewData["Title"] = post.Title;
            var totalCount = post.Replies.Count();
            var pageCount = 1;
            if (totalCount > 0)
            {
                pageCount = (int)Math.Ceiling((double)totalCount! / pagesize);
            }
            if(page == -1)
            {
                page = pageCount;
                //pagedReplies = PagedReplies(page, pagesize, sortdir, post); 
            }
            PagedList<PostReply>? pagedReplies = PagedReplies(page, pagesize, sortdir, post);
            //we have a replyid, is it in the current set, otherwise skip forwards
            if (replyid.HasValue && totalCount > 1)
            {
                while (pagedReplies != null && pagedReplies.All(p => p.Id != replyid))
                {
                    page += 1;
                    pagedReplies = PagedReplies(page, pagesize, sortdir, post);                    
                }
            } 

            IEnumerable<PostReplyModel> replies = new HashSet<PostReplyModel>();
            if (pagedReplies != null)
            {
                replies = BuildPostReplies(pagedReplies);

            }
            var model = new PostIndexModel()
            {
                Id = post.Id,
                Title = post.Title,
                Author = post.Member!,
                AuthorId = post.Member!.Id,
                ShowSig = post.Sig == 1,
                Views = post.ViewCount,
                Status = post.Status,
                IsLocked = post.Status == 0 || post.Forum?.Status == 0,
                IsSticky = post.IsSticky == 1,
                HasPoll = haspoll,
                Answered = post.Answered,
                //AuthorRating = post.User?.Rating ?? 0,
                AuthorName = post.Member?.Name ?? "Unknown",
                Created = post.Created.FromForumDateStr(),
                Content = post.Content,
                Replies = replies,
                ForumId = post.Forum!.Id,
                ForumName = post.Forum.Title,
                PageNum = page,
                PageCount = pageCount,
                PageSize = pagesize,
                SortDir = sortdir,
                Edited = post.LastEdit?.FromForumDateStr(),
                EditedBy = post.LastEditby == null ? "" : _memberService.GetMemberName(post.LastEditby.Value),
                AllowTopicRating = post.AllowRating == 1 && _config.GetIntValue("INTTOPICRATING")==1 ,
                AllowRating = post.Forum.Rating==1 && _config.GetIntValue("INTTOPICRATING")==1 && !_memberService.HasRatedTopic(post.Id,_memberService.Current()?.Id),
                Rating = post.GetTopicRating()
            };

            return View(model);
        }
        public IActionResult Archived(int id,int page = 1, int pagesize = 0, string sortdir="desc", int? replyid = null)
        {
            bool signedin = false;
            ViewBag.RequireAuth = false;
            if (User.Identity is { IsAuthenticated: true })
            {
                signedin = true;
            }
            var post = _postService.GetArchivedTopic(id);
            if(post == null)
            {
                ViewBag.Error = "No Topic Found with that ID";
                return View("Error");
            }            
            var haspoll = false;

            bool passwordrequired = false;
            bool notallowed = false;
            bool ismoderator = User.IsInRole($"Forum_{post.ForumId}");
            bool isadministrator = User.IsInRole("Administrator");

            notallowed = CheckAuthorisation(post!.Forum!.Privateforums, signedin, ismoderator, isadministrator, ref passwordrequired);
            if (!isadministrator && passwordrequired)
            {
                var auth = _httpcontext!.Session.GetString("Pforum_" + post.ForumId) == null ? "" : _httpcontext.Session.GetString("Pforum_" + post.ForumId);
                if (auth != post.Forum.Password)
                {
                    ViewBag.RequireAuth = true;
                }
            }

            if (post.ReplyCount > 0 || post.UnmoderatedReplies > 0)
            {
                post = _postService.GetArchivedTopicWithRelated(id);
            }
            if (!HttpContext.Session.Keys.Contains("TopicId_"+ id))
            {
                HttpContext.Session.SetInt32("TopicId_"+ id,1);
                post!.ViewCount += 1;
                _postService.UpdateViewCount(post.Id);
            }

            if (HttpContext.Session.GetInt32("TopicPageSize") != null && pagesize == 0)
            {
                pagesize = HttpContext.Session.GetInt32("TopicPageSize")!.Value;
            }
            else if (pagesize == 0)
            {
                pagesize = 10;
            }
            HttpContext.Session.SetInt32("TopicPageSize",pagesize);

            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", post!.Category?.Name){ Parent = homePage,RouteValues = new{id=post.Category?.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", post.Forum?.Title){ Parent = catPage,RouteValues = new{id=post.ForumId}};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", post.Subject) { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;
            
            ViewData["Title"] = post.Subject;
            var totalCount = post.Replies.Count();
            var pageCount = 1;
            if (totalCount > 0)
            {
                pageCount = (int)Math.Ceiling((double)totalCount! / pagesize);
            }

            PagedList<ArchivedReply>? pagedReplies = PagedReplies(page, pagesize, sortdir, post);
            //todo: if we have a replyid, is it in the current set, otherwise skip forwards
            if (replyid.HasValue)
            {
                while (pagedReplies != null && pagedReplies.All(p => p.Id != replyid))
                {
                    page += 1;
                    pagedReplies = PagedReplies(page, pagesize, sortdir, post);                    
                }
            }

            IEnumerable<PostReplyModel>? replies = null;
            if (pagedReplies != null)
            {
                replies = BuildPostReplies(pagedReplies);

            }
            var model = new PostIndexModel()
            {
                Id = post.Id,
                Title = post.Subject,
                Author = post.Member!,
                AuthorId = post.Member!.Id,
                ShowSig = post.Sig == 1,
                Views = post.ViewCount,
                Status = post.Status,
                IsLocked = post.Status == 0 || post.Forum?.Status == 0,
                IsSticky = post.IsSticky == 1,
                HasPoll = haspoll,
                Answered = false,
                //AuthorRating = post.User?.Rating ?? 0,
                AuthorName = post.Member?.Name ?? "Unknown",
                Created = post.Created.FromForumDateStr(),
                Content = post.Message,
                Replies = replies,
                ForumId = post.Forum!.Id,
                ForumName = post.Forum.Title,
                PageNum = page,
                PageCount = pageCount,
                PageSize = pagesize,
                SortDir = sortdir,
                Edited = post.LastEdit?.FromForumDateStr(),
                EditedBy = post.LastEditby == null ? "" : _memberService.GetMemberName(post.LastEditby.Value),
                Archived = true
            };

            return View("Index",model);
        }

        [Authorize]
        public async Task<IActionResult> Create(int id)
        {
            var member = await _memberService.GetById(User);
            if (_config.GetIntValue("STRFLOODCHECK") == 1 && !User.IsInRole("Administrator"))
            {
                var timeout = _config.GetIntValue("STRFLOODCHECKTIME", 30);
                if (member!.Lastpostdate.FromForumDateStr() > DateTime.UtcNow.AddSeconds(-timeout))
                {
                    TempData["Error"] = "floodcheck";
                    ViewBag.Error = _languageResource.GetString("FloodcheckErr", timeout);
                    return View("Error");
                }
            }
            var forum = _forumService.GetWithPosts(id);
            var model = new NewPostModel()
            {
                Id = 0,
                CatId = forum.CategoryId,
                ForumName = forum.Title,
                ForumId = id,
                Forums = _forumService.ForumList(),
                IsPost = true,
                AuthorName = User.Identity?.Name!,
                UseSignature = member!.SigDefault == 1,
                AllowRating = forum.Rating == 1,
                IsAuthor = true,
                Lock = false,
                Sticky = false,
                DoNotArchive = false,
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name){ Parent = homePage,RouteValues = new{id=forum.Category!.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Create", "Topic", "New Post") { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;

            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> Reply(int id)
        {

            var member = await _memberService.GetById(User);
            if (_config.GetIntValue("STRFLOODCHECK") == 1 && !User.IsInRole("Administrator"))
            {
                var timeout = _config.GetIntValue("STRFLOODCHECKTIME", 30);
                if (member!.Lastpostdate.FromForumDateStr() > DateTime.UtcNow.AddSeconds(-timeout))
                {
                    TempData["Error"] = "floodcheck";
                    ViewBag.Error = _languageResource.GetString("FloodcheckErr", timeout);
                    return View("Error");
                }
            }
            var topic = await _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetWithPosts(topic!.ForumId);
            var model = new NewPostModel()
            {
                TopicId = id,
                ForumName = forum.Title,
                ForumId = topic.ForumId,
                CatId = forum.CategoryId,
                IsPost = false,
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name){ Parent = homePage,RouteValues = new{id=forum.Category!.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "lblReply") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public async Task<IActionResult> QuoteAsync(int id)
        {

            var member = await _memberService.GetById(User);
            var topic = await _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetWithPosts(topic!.ForumId);
            var model = new NewPostModel()
            {
                TopicId = id,
                ForumName = forum.Title,
                ForumId = topic.ForumId,
                CatId = forum.CategoryId,
                IsPost = false,
                Content = $"[quote]{topic.Content}[/quote=Originally posted by {topic.Member?.Name}]",
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name){ Parent = homePage,RouteValues = new{id=forum.Category!.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Reply with Quote") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {

            var member = await _memberService.GetById(User);
            var topic = await _postService.GetTopicWithRelated(id);
            var haspoll = _postService.HasPoll(id);
            var forum = _forumService.GetWithPosts(topic!.ForumId);
            var model = new NewPostModel()
            {
                Id = id,
                TopicId = id,
                ForumName = forum.Title,
                ForumId = topic.ForumId,
                Forums = _forumService.ForumList(),
                CatId = forum.CategoryId,
                Title = topic.Title,
                IsPost = true,
                Content = topic.Content,
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                AllowRating = topic.Forum.Rating == 1,
                AllowTopicRating = topic.AllowRating == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = topic.Created.FromForumDateStr(),
                IsAuthor = member!.Id == topic.MemberId
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name){ Parent = homePage,RouteValues = new{id=forum.Category!.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Edit", "Topic", "Edit Post") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;
            if (haspoll)
            {
                TempData["HasPoll"] = haspoll;
                TempData["Poll"] = _postService.GetPoll(id);
            };
            return View("Create", model);

        }
        [Authorize]
        public async Task<IActionResult> QuoteReply(int id)
        {

            var member = await _memberService.GetById(User);
            if (_config.GetIntValue("STRFLOODCHECK") == 1 && !User.IsInRole("Administrator"))
            {
                var timeout = _config.GetIntValue("STRFLOODCHECKTIME", 30);
                if (member!.Lastpostdate.FromForumDateStr() > DateTime.UtcNow.AddSeconds(-timeout))
                {
                    TempData["Error"] = "floodcheck";
                    ViewBag.Error = _languageResource.GetString("FloodcheckErr", timeout);
                    return View("Error");
                }
            }
            var reply = _postService.GetReply(id);
            
            var topic = await _postService.GetTopicWithRelated(reply!.PostId);
            var model = new NewPostModel()
            {
                Id = 0,
                TopicId = topic!.Id,
                ForumName = topic.Forum!.Title,
                ForumId = topic.ForumId,
                CatId = topic.CategoryId,
                IsPost = false,
                Content = $"[quote]{reply.Content}[/quote=Originally posted by {reply.Member!.Name}]<br/>",
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = DateTime.UtcNow,
                IsAuthor = User.Identity?.Name == member!.Name
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", topic.Category!.Name){ Parent = homePage,RouteValues = new{id=topic.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", topic.Forum.Title){ Parent = catPage,RouteValues = new{id=topic.Forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Reply with Quote") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public async Task<IActionResult> EditReply(int id)
        {

            var member = await _memberService.GetById(User);
            var reply = _postService.GetReply(id);
            var topic = await _postService.GetTopicWithRelated(reply!.PostId);
            var model = new NewPostModel()
            {
                Id = id,
                TopicId = topic!.Id,
                ForumName = topic.Forum!.Title,
                ForumId = topic.ForumId,
                CatId = topic.CategoryId,
                IsPost = false,
                Content = _bbcodeProcessor.CleanCode(reply.Content),
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = reply.Created.FromForumDateStr(),
                IsAuthor = User.Identity?.Name == member!.Name
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", topic.Category!.Name){ Parent = homePage,RouteValues = new{id=topic.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", topic.Forum.Title){ Parent = catPage,RouteValues = new{id=topic.Forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Edit Reply") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }

        [HttpPost]
        [Authorize]
        [Route("AddPost/")]
        public async Task<IActionResult> AddPost(NewPostModel model)
        {
            ModelState.Remove("ForumName");
            ModelState.Remove("AuthorName");
            ModelState.Remove("AuthorImageUrl");
            ModelState.Remove("Sticky");
            ModelState.Remove("Lock");
            ModelState.Remove("DoNotArchive");

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            var post = BuildPost(model, user!.MemberId);
            if (model.TopicId != 0)
            {
                post.Title = model.Title;
                post.IsSticky = (short)(model.Sticky ? 1 : 0);
                post.AllowRating = (short)(model.AllowTopicRating ? 1 : 0);
                post.Sig = (short)(model.UseSignature ? 1 : 0);
                post.Status = (short)(model.Lock ? 0 : post.Status);
                post.ArchiveFlag = model.DoNotArchive ? 1 : 0;
                post.Content = model.Content;
                post.LastEdit = DateTime.UtcNow.ToForumDateStr();
                post.LastEditby = user.MemberId;
                if (post.ForumId != model.ForumId)
                {
                    var forum = _snitzDbContext.Forums.AsNoTracking().OrderBy(f=>f.Id).First(f => f.Id == model.ForumId);
                    var author = _memberService.GetById(post.MemberId);
                    post.ForumId = model.ForumId;
                    post.CategoryId = forum.CategoryId;
                    await _postService.Update(post);
                    //We are moving the topic so need to update the ForumId and CategoryId for the replies
                    var replies = _snitzDbContext.Replies.Where(r => r.PostId == model.TopicId);
                    await replies
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(e => e.ForumId, forum.Id)
                            .SetProperty(e => e.CategoryId, forum.CategoryId));
                    await _forumService.UpdateLastPost(model.ForumId);
                    if (_config.GetIntValue("STRMOVENOTIFY") == 1)
                    {
                        await _mailSender.MoveNotify(author!,post);
                    }
                }
                else
                {
                    await _postService.Update(post);
                }
                
            }
            else
            {
                post.Created = DateTime.UtcNow.ToForumDateStr();
                var topicid = await _postService.Create(post);
                if (post.Status < 2) //not waiting to be moderated
                {
                    var sub = _config.GetIntValue("STRSUBSCRIPTION");
                    if (!(sub == 0 || sub == 4))
                    {
                        var forum = _forumService.GetWithPosts(post.ForumId);
                        switch (forum.Category!.Subscription)
                        {
                            case 1 :
                                BackgroundJob.Enqueue(() => _processSubscriptions.Topic(topicid));
                                break;
                        }

                        switch (forum.Subscription)
                        {
                            case 1:
                                BackgroundJob.Enqueue(() => _processSubscriptions.Topic(topicid));
                                break;
                        }
                    }
                    if (_config.GetIntValue("STRPMSTATUS") == 1)
                    {
                        MatchCollection matches = Regex.Matches(post.Content, @"(?:@""(?:[^""]+))|(?:@(?:[^\s^<]+))",RegexOptions.IgnoreCase);
                        foreach (Match match in matches)
                        {
                            var taggedmember = _memberService.GetByUsername(WebUtility.HtmlDecode(match.Value.Replace("@", "")));
                            if (taggedmember != null && taggedmember.Id != post.MemberId)
                            {
                                //user mentioned in post, send them a PM
                                PrivateMessage msg = new PrivateMessage
                                {
                                    To = taggedmember.Id,
                                    From = post.MemberId,
                                    Subject = _languageResource["MentionedInPostSubject"].Value,// "You were mentioned in a Post",
                                    Message = _languageResource["MentionedMessage",taggedmember.Name, _config.ForumUrl?.Trim() ?? "", post.Id,""].Value,                                    SentDate = DateTime.UtcNow.ToForumDateStr(),
                                    Read = 0,
                                    SaveSentMessage = 0
                                };
                                _pmService.Create(msg);
                            }
                        }
                    }
                }

            }

            return Json(new{url=Url.Action("Index", "Topic", new { id = post.Id }),id=post.Id});

        }
        
        [HttpPost]
        [Route("AddPoll/")]
        public IActionResult AddPoll(Poll poll)
        {
            try
            {
                var existingpoll = _snitzDbContext.Polls.Find(poll.Id);
                var polltoadd = new Poll()
                {
                    TopicId = poll.TopicId,
                    ForumId = poll.ForumId,
                    CatId = poll.CatId,
                    Question = poll.Question,
                    Whovotes = poll.Whovotes,
                    Lastvote = DateTime.UtcNow.ToForumDateStr()
                };
                if (existingpoll == null)
                {
                    _snitzDbContext.Polls.Add(polltoadd);

                }
                else
                {
                    existingpoll.Question = poll.Question;
                    existingpoll.Whovotes = poll.Whovotes;
                    _snitzDbContext.Polls.Update(existingpoll);
                }
                _snitzDbContext.SaveChanges();
                foreach (PollAnswer pollAnswer in poll.PollAnswers!)
                {
                    if (!string.IsNullOrWhiteSpace(pollAnswer.Label))
                    {
                        if (existingpoll == null)
                        {
                            pollAnswer.PollId = polltoadd.Id;
                            _snitzDbContext.PollAnswers.Add(pollAnswer);
                        }
                        else
                        {
                            var existinganswer = _snitzDbContext.PollAnswers.Find(pollAnswer.Id);
                            if (existinganswer != null)
                            {
                                existinganswer.Order = pollAnswer.Order;
                                existinganswer.Label = pollAnswer.Label;
                                _snitzDbContext.PollAnswers.Update(existinganswer);
                            }
                            else
                            {
                                pollAnswer.PollId = polltoadd.Id;
                                _snitzDbContext.PollAnswers.Add(pollAnswer);
                            }
                        }
                    }
                }
                _snitzDbContext.SaveChanges(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
               
            return Json(new{url=Url.Action("Index", "Topic", new { id = poll.TopicId }),id = poll.TopicId});
        }

        [HttpPost]
        [Authorize]
        [Route("AddReply/")]
        public async Task<IActionResult> AddReply(NewPostModel model)
        {
            var member = _memberService.GetByUsername(User.Identity!.Name!);
            if (_config.GetIntValue("STRFLOODCHECK") == 1 && !User.IsInRole("Administrator"))
            {
                var timeout = _config.GetIntValue("STRFLOODCHECKTIME", 30);
                if (member!.Lastpostdate.FromForumDateStr() > DateTime.UtcNow.AddSeconds(-timeout))
                {
                    
                    TempData["Error"] = "floodcheck";

                    return Json(new{url=Url.Action("Index", "Topic", new { id = model.TopicId }),id=model.TopicId});
                }
            }
            var reply = BuildReply(model, member!.Id);
            if (model.Id != 0)
            {
                reply!.Content = model.Content;
                reply.Sig = (short)(model.UseSignature ? 1 : 0);
                reply.LastEdited = DateTime.UtcNow.ToForumDateStr();
                reply.LastEditby = member.Id;
                await _postService.Update(reply);
            }
            else
            {
                var replyid = await _postService.Create(reply!);
                if (reply!.Status < 2) //not waiting to be moderated
                {
                    var forum = _forumService.GetWithPosts(reply.ForumId);
                    var sub = _config.GetIntValue("STRSUBSCRIPTION");
                    if (sub != 0)
                    {
                        switch (forum.Category?.Subscription)
                        {
                            case 1 :
                                BackgroundJob.Enqueue(() => _processSubscriptions.Reply(replyid));
                                break;
                        }
                        switch (forum.Subscription)
                        {
                            case 1:
                            case 2:
                                BackgroundJob.Enqueue(() => _processSubscriptions.Reply(replyid));
                                break;
                        }
                    }
                    if (_config.GetIntValue("STRPMSTATUS") == 1)
                    {
                        MatchCollection matches = Regex.Matches(reply.Content, @"(?:@""(?:[^""]+))|(?:@(?:[^\s^<]+))",RegexOptions.IgnoreCase);
                        foreach (Match match in matches)
                        {
                            var taggedmember = _memberService.GetByUsername(WebUtility.HtmlDecode(match.Value.Replace("@", "")));
                            if (taggedmember != null && taggedmember.Id != reply.MemberId)
                            {
                                //user mentioned in post, send them a PM
                                PrivateMessage msg = new PrivateMessage
                                {
                                    To = taggedmember.Id,
                                    From = reply.MemberId,
                                    Subject = _languageResource["MentionedInPostSubject"].Value,// "You were mentioned in a Post",
                                    Message = _languageResource["MentionedMessage",member.Name, _config.ForumUrl?.Trim() ?? "",reply.PostId, reply.Id].Value,
                                    SentDate = DateTime.UtcNow.ToForumDateStr(),
                                    Read = 0,SaveSentMessage = 0
                                };
                                _pmService.Create(msg);

                            }
                        }
                    }
                }

            }
            var topic = _postService.GetTopicForUpdate(reply.PostId);
            topic.Status = (short)(model.Lock ? 0 : 1);
            topic.IsSticky = (short)(model.Sticky ? 1 : 0);
            topic.ArchiveFlag = (short)(model.DoNotArchive ? 1 : 0);
            _postService?.Update(topic);

            // TODO: Implement User Rating Management
            return Json(new{url=Url.Action("Index", "Topic", new { id = reply.PostId }),id=reply.PostId});
            //return RedirectToAction("Index", "Topic", new { id = reply.PostId });
        }
        [HttpPost]
        [Authorize]
        [Route("Topic/DeleteReply/")]
        public async Task<IActionResult> DeleteReply(int id)
        {

            var member = _memberService.GetById(User).Result;
            var post = _postService.GetReply(id);
            //if this isn't the last post then can't delete it
            if ((post!.MemberId == member!.Id && post!.Topic!.LastPostReplyId != id) && !member.Roles.Contains("Administrator") && post.Status < 2)
            {
                ModelState.AddModelError("","Unable to delete this reply");
                return Json(new { result = false, error = "Unable to delete this reply" });

            }
            await _postService.DeleteReply(id);
            return Json(new { result = true, data = post.Topic!.Id });

        }
        [HttpPost]
        [Authorize]
        [Route("Topic/DeleteTopic/")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var member = _memberService.GetById(User).Result;
            var post = _postService.GetTopicAsync(id).Result;
            if (member != null && (member.Roles.Contains("Administrator") || post!.MemberId == member.Id))
            {
                await _postService.DeleteTopic(id);
                //Are there any Polls or events to remove?
                var topicpoll = _snitzDbContext.Polls.FirstOrDefault(p => p.TopicId == id);
                if (topicpoll != null)
                {
                    _snitzDbContext.PollAnswers.Where(a=>a.PollId == topicpoll.Id).ExecuteDelete();
                    _snitzDbContext.PollVotes.Where(a=>a.PollId == topicpoll.Id).ExecuteDelete();
                    _snitzDbContext.Polls.Remove(topicpoll);
                    _snitzDbContext.SaveChanges();
                }
                var topicevent = _eventsContext.EventItems.FirstOrDefault(e => e.TopicId == id);
                if (topicevent != null)
                {
                    _eventsContext.EventItems.Remove(topicevent);
                    _eventsContext.SaveChanges();
                }
                return Json(new { result = true, url = Url.Action("Index","Forum", new{id=post!.ForumId}) });

            }
            
            return Json(new { result = false, error = "Error Deleting Topic" });

        }
        [HttpPost]
        [Authorize]
        [Route("Topic/Answered/")]
        public async Task<IActionResult> Answered(int id)
        {
            var result = await _postService.Answer(id);
            return result ? Json(new { result = result, data = id }) : Json(new { result = result, error = "Unable to toggle Status" });

        }
        [HttpPost]
        [Authorize]
        [Route("Topic/LockTopic/")]
        public async Task<IActionResult> LockTopic(int id, int status)
        {

            var member = _memberService.GetById(User).Result;

            if (member != null && !member.Roles.Contains("Administrator"))
            {
                ModelState.AddModelError("","Unable to lock this reply");
                return Json(new { result = false, error = "Unable to lock Post" });

            }

            var result = await _postService.LockTopic(id, (short)status);
            return result ? Json(new { result, data = id }) : Json(new { result, error = "Unable to toggle Status" });
            
        }
        public IActionResult PasswordCheck(string pwd,string forumid,string? topicid)
        {
            var forum = _forumService.GetWithPosts(Convert.ToInt32(forumid));
            if (forum != null && forum.Password == pwd)
            {
                _httpcontext!.Session.SetString("Pforum_" + forumid, pwd);
                return Json(true);
            }

            return Json(false);
        }
        [Authorize]
        public ActionResult SendTo(int id, int archived)
        {
            EmailViewModel em = new EmailViewModel();
            //var archived = Request.Query["archived"] == "1";
            
            if (archived == 1)
            {
                var topic = _postService.GetArchivedTopic(id);
                Member? from = _memberService.Current();
                
                if (from != null)
                {
                    em.FromEmail = from.Email!;
                    em.FromName = from.Name;
                }
                else
                {
                    ViewBag.Sent = true;
                    ViewBag.Error = "Error loading data";
                    return View("Error");
                }
                em.ReturnUrl = topic!.Id.ToString();
                
                em.Subject = _languageResource["sendtoSubject", from.Name].Value;
                
                em.Message =
                    String.Format(
                        _languageResource["sendtoMessage"].Value,
                        _config.ForumTitle, _config.ForumUrl,
                        topic.Id, Request.Query["archived"], topic.Subject);
                ViewBag.TopicTitle = topic.Subject;
                
                ViewBag.Sent = false;
                return PartialView("popSendTo", em);
            }
            else
            {
                var topic = _postService.GetTopicAsync(id).Result;
                Member? from = _memberService.Current();
                
                if (from != null)
                {
                    em.FromEmail = from.Email!;
                    em.FromName = from.Name;
                }
                else
                {
                    ViewBag.Sent = true;
                    ViewBag.Error = "Error loading data";
                    return View("Error");
                }
                em.ReturnUrl = topic!.Id.ToString();
                
                em.Subject = _languageResource["sendtoSubject", from.Name].Value;
                
                em.Message =
                    String.Format(
                        _languageResource["sendtoMessage"].Value,
                        _config.ForumTitle, _config.ForumUrl,
                        topic.Id, Request.Query["archived"], topic.Title);
                ViewBag.TopicTitle = topic.Title;
                
                ViewBag.Sent = false;
                return PartialView("popSendTo", em);
            }

        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SendTo(EmailViewModel model)
        {
            _mailSender.SendToFreind( model);
            TempData["Success"] = "Email sent successfully";
            return RedirectToAction("Index", "Topic", new { id=model.ReturnUrl, pagenum = -1 });
        }
        public ActionResult Print(int id)
        {
            bool moderator;
            bool admin;
            string templateView = "";
            PostIndexModel model;

            var archived = Request.Query["archived"] == "1";
            if (archived)
            {
                var topic = _postService.GetArchivedTopic(id);
                if (topic != null)
                {
                    moderator = User.IsInRole("Forum_" + topic.ForumId);
                    admin = User.IsInRole("Administrator");

                }
                else
                {
                    ViewBag.Error = "No Topic Found with that ID";
                    return View("Error");
                }
                if (topic.ReplyCount > 0)
                {
                    topic = _postService.GetArchivedTopicWithRelated(id);
                }
                PagedList<ArchivedReply>? pagedReplies = PagedReplies(1, 100, "desc", topic!);
                model = new PostIndexModel()
                {
                    Id = topic!.Id,
                    Title = topic.Subject,
                    Author = topic.Member!,
                    AuthorId = topic.Member!.Id,
                    ShowSig = topic.Sig == 1,
                    Views = topic.ViewCount,
                    Status = topic.Status,
                    IsLocked = topic.Status == 0 || topic.Forum?.Status == 0,
                    IsSticky = topic.IsSticky == 1,
                    HasPoll = _postService.HasPoll(topic.Id),
                    //AuthorRating = post.User?.Rating ?? 0,
                    AuthorName = topic.Member?.Name ?? "Unknown",
                    Created = topic.Created.FromForumDateStr(),
                    Content = topic.Message,
                    Replies = pagedReplies != null ? BuildPostReplies(pagedReplies) : new HashSet<PostReplyModel>(),
                    ForumId = topic.Forum!.Id,
                    ForumName = topic.Forum.Title,
                    PageNum = 1,
                    PageCount = 10,
                    PageSize = 100,
                    SortDir = "asc",
                    Edited = topic.LastEdit?.FromForumDateStr(),
                    EditedBy = topic.LastEditby == null ? "" : _memberService.GetMemberName(topic.LastEditby.Value)
                };
            }
            else
            {
                var topic = _postService.GetTopicAsync(id).Result;
                if (topic != null)
                {
                    moderator = User.IsInRole("Forum_" + topic.ForumId);
                    admin = User.IsInRole("Administrator");

                }
                else
                {
                    ViewBag.Error = "No Topic Found with that ID";
                    return View("Error");
                }
                if (topic.ReplyCount > 0)
                {
                    topic = _postService.GetTopicWithRelated(id).Result;
                }
                PagedList<PostReply>? pagedReplies = PagedReplies(1, 100, "asc", topic!);
                model = new PostIndexModel()
                {
                    Id = topic!.Id,
                    Title = topic.Title,
                    Author = topic.Member!,
                    AuthorId = topic.Member!.Id,
                    ShowSig = topic.Sig == 1,
                    Views = topic.ViewCount,
                    IsLocked = topic.Status == 0 || topic.Forum?.Status == 0,
                    IsSticky = topic.IsSticky == 1,
                    HasPoll = _postService.HasPoll(topic.Id),
                    Answered = topic.Answered,
                    //AuthorRating = post.User?.Rating ?? 0,
                    AuthorName = topic.Member?.Name ?? "Unknown",
                    Created = topic.Created.FromForumDateStr(),
                    Content = topic.Content,
                    Replies = pagedReplies != null ? BuildPostReplies(pagedReplies) : new HashSet<PostReplyModel>(),
                    ForumId = topic.Forum!.Id,
                    ForumName = topic.Forum.Title,
                    PageNum = 1,
                    PageCount = 10,
                    PageSize = 100,
                    SortDir = "desc",
                    Edited = topic.LastEdit?.FromForumDateStr(),
                    EditedBy = topic.LastEditby == null ? "" : _memberService.GetMemberName(topic.LastEditby.Value)
                };
            }

            //if (topic.Forum.Type == Enumerators.ForumType.BlogPosts)
            //{
            //    templateView = "Blog/";
            //}


            ViewBag.IsForumModerator = moderator;
            ViewBag.IsAdministrator = admin;


            return View(templateView + "Print", model);
        }
        /// <summary>
        /// Process Topic Moderation
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        [ValidateAntiForgeryToken]
        [Route("Topic/ModeratePost/")]
        public async Task<ActionResult> ModeratePost(ApproveTopicViewModal vm)
        {

            if (ModelState.IsValid)
            {
                var topic = _postService.GetTopicForUpdate(vm.Id);
                var author = _memberService.GetById(topic.MemberId);
                var forum = _forumService.GetWithPosts(topic.ForumId);
                var subject = "";
                var message = "";
                
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                var topicLink = Url.Action("Index", "Topic", new { id = topic.Id, pagenum=-1}, Request.Scheme);
                switch (vm.PostStatus)
                {
                    case "Approve" :
                        topic.Status = 1;
                        _snitzDbContext.Update(topic);
                        await _snitzDbContext.SaveChangesAsync();
                        await _postService.UpdateLastPost(vm.Id,null);
                        //update Forum
                        forum = await _forumService.UpdateLastPost(topic.ForumId);
                        if (forum.CountMemberPosts == 1)
                        {
                            await _memberService.UpdatePostCount(topic.MemberId);
                        }
                        //Send email
                        subject = _config.ForumTitle + ": Topic Approved";
                        message = 
                            _mailSender.ParseTemplate("approvePost.html",_languageResource["tipApproveTopic"].Value,author?.Email!,author?.Name!, topicLink!, cultureInfo.Name,vm.ApprovalMessage);

                        var sub = (SubscriptionLevel)_config.GetIntValue("STRSUBSCRIPTION");
                        if (!(sub == SubscriptionLevel.None || sub == SubscriptionLevel.Topic))
                        {
                            switch ((ForumSubscription)forum.Subscription)
                            {
                                case ForumSubscription.ForumSubscription:
                                    BackgroundJob.Enqueue(() => _processSubscriptions.Topic(vm.Id));
                                    break;
                            }
                        }
                        break;
                    case "Reject":
                        
                        await _postService.DeleteTopic(topic.Id);
                        //Send email
                            subject = _config.ForumTitle + ": Topic rejected";
                            message = 
                            _mailSender.ParseTemplate("rejectPost.html",_languageResource["tipRejectTopic"].Value,author?.Email!,author?.Name!, "", cultureInfo.Name,vm.ApprovalMessage);

                        break;
                    case "Hold":
                        await _postService.SetStatus(vm.Id, Status.OnHold);
                        //Send email
                            subject = _config.ForumTitle + ": Topic placed on Hold";
                            message =                             
                                _mailSender.ParseTemplate("onholdPost.html",_languageResource["tipOnholdTopic"].Value,author?.Email!,author?.Name!, topicLink!, cultureInfo.Name,vm.ApprovalMessage);

                        break;
                }
                if (vm.EmailAuthor)
                {
                    _mailSender.ModerationEmail(author, subject, message, forum, topic);
                }
                
                return RedirectToAction("Index", "Forum", new { id=topic.ForumId});
            }

            return PartialView("popModerate",vm);
        }
        /// <summary>
        /// Open moderation Popup window
        /// </summary>
        /// <param name="id">Id of Unmoderated <see cref="Topic"/></param>
        /// <returns>PopUp Window</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public PartialViewResult Moderate(int id)
        {
            ApproveTopicViewModal vm = new ApproveTopicViewModal {Id = id};
            return PartialView("popModerate",vm);
        }

        [HttpGet]
        [Authorize]
        [Route("Topic/Subscribe/")]
        public IActionResult Subscribe(int id)
        {
            var topic = _postService.GetTopicAsync(id).Result;
            var member = _memberService.Current();
            _snitzDbContext.MemberSubscription.Add(new MemberSubscription()
            {
                MemberId = member!.Id,
                CategoryId = topic!.CategoryId,
                ForumId = topic.ForumId,
                PostId = id
            });
            _snitzDbContext.SaveChanges();
            return Content("OK");
        }
        [HttpGet]
        [Authorize]
        [Route("Topic/UnSubscribe/")]
        public IActionResult UnSubscribe(int id)
        {
            var member = _memberService.Current();
            _snitzDbContext.MemberSubscription.Where(s => s.MemberId == member!.Id && s.PostId == id).ExecuteDelete();
            return Content("OK");
        }

        [Authorize(Roles = "Administrator,Moderator")]
        [Route("Topic/Merge/")]
        public IActionResult Merge(int[]? selected)
        {
            try
            {
                var maintopicid = 0;
                if (selected != null)
                {
                    if (selected.Length < 2)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json("You must select at least two topics before merging");
                    }

                    try
                    {
                        maintopicid = _postService.CreateForMerge(selected);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }


                    var sub = (SubscriptionLevel)_config.GetIntValue("STRSUBSCRIPTION");
                    var topic = _postService.GetTopicAsync(maintopicid).Result;
                    if (topic != null)
                    {
                        if (sub == SubscriptionLevel.Topic || sub == SubscriptionLevel.Forum || sub == SubscriptionLevel.Category)
                        {
                            switch ((ForumSubscription)topic.Forum!.Subscription)
                            {
                                case ForumSubscription.ForumSubscription:
                                case ForumSubscription.TopicSubscription:
                                    BackgroundJob.Enqueue(() => _processSubscriptions.Topic(maintopicid));
                                    break;
                            }
                        }                        
                    }

                    string? redirectUrl = Url.Action("Index","Topic", new { id = maintopicid, pagenum = 1 });

                    return Json(new { data = redirectUrl });

                }
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("You must select at least two topics before merging");

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new{error = e.Message});
            }

        }

        [Authorize(Roles = "Administrator,Moderator")]
        [HttpGet]
        [Route("Topic/SplitTopic/")]
        public IActionResult SplitTopic(int id, int replyid)
        {
            SplitTopicViewModel vm = new SplitTopicViewModel();
            foreach (KeyValuePair<int, string> forum in _forumService.ForumList())
            {
                if(!vm.ForumList!.ContainsKey(forum.Key))
                    vm.ForumList.Add(forum.Key, forum.Value);
            }
            var topic = _postService.GetTopicWithRelated(id).Result;
            if (topic != null)
            {
                vm.Topic = topic;
                vm.Id = topic.Id;
                vm.Replies = topic?.Replies;
                ViewBag.ReplyId = replyid;
            }
            else
            {
                return View("Error");
            }
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", topic!.Category?.Name){ Parent = homePage,RouteValues = new{id=topic.Category?.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", topic.Forum?.Title){ Parent = catPage,RouteValues = new{id=topic.ForumId}};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        [ValidateAntiForgeryToken]
        [Route("Topic/SplitTopic/")]
        public IActionResult SplitTopic(SplitTopicViewModel vm)
        {
            var originaltopic = _postService.GetTopicWithRelated(vm.Id).Result;
            if (!ModelState.IsValid)
            {
                foreach (KeyValuePair<int, string> vmforum in _forumService.ForumList())
                {
                    if(!vm.ForumList!.ContainsKey(vmforum.Key))
                        vm.ForumList.Add(vmforum.Key, vmforum.Value);
                }
                if (originaltopic != null)
                {
                    vm.Topic = originaltopic;
                    vm.Replies = originaltopic?.Replies;
                }

                return View(vm);
            }
            string[] ids = Request.Form["check"]!;
            if (ids == null || !ids.Any())
            {
                //ModelState.AddModelError("Reply", _languageResource.GetString("TopicController_select_at_least_one_reply"));
                ViewBag.Error = _languageResource.GetString("TopicController_select_at_least_one_reply");
                return View("Error");
            }
            Post? topic = null;

            if (ids != null)
            {
                topic = _postService.SplitTopic(ids, vm.ForumId, vm.Subject);

                _postService.UpdateLastPost(topic!.Id,0);
                _forumService.UpdateLastPost(topic.ForumId);
                _postService.UpdateLastPost(originaltopic!.Id,0);
                //        EmailController.TopicSplitEmail(ControllerContext, topic);
            }

            if (topic != null) return RedirectToAction("Index", new { id = topic.Id, pagenum = 1 });
            ViewBag.Error = "Unknown problem";
            return View("Error");
        }
        [Route("Topic/SaveRating/")]
        public IActionResult SaveRating(IFormCollection form)
        {
            if (form.Keys.Contains("PostRating"))
            {
                var topic = _postService.GetTopicForUpdate(Convert.ToInt32(Request.Form["TopicId"]));
                if (Request.Form["PostRating"] != "0")
                {
                    topic.RatingTotal += (int) (decimal.Parse(Request.Form["PostRating"])*10);
                }

                topic.RatingTotalCount += 1;
                _snitzDbContext.Posts.Update(topic);

                TopicRating tr = new TopicRating
                {
                    RatingsBymemberId = Convert.ToInt32(Request.Form["MemberId"]),
                    RatingsTopicId = Convert.ToInt32(Request.Form["TopicId"])
                };
                _snitzDbContext.Add(tr);
                _snitzDbContext.SaveChanges();

                return Json(new {success = true, responseText = "Voted"});
            }
            return Json(new {success = false, responseText = "PostRating not found"});
            
        }
        /// <summary>
        /// Tracks Checked Topic list
        /// </summary>
        /// <param name="topicid">Id of checked <see cref="Topic"/></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator,Moderator")]
        [HttpPost]
        [Route("Topic/UpdateTopicList/")]
        public EmptyResult UpdateTopicList(int topicid)
        {
            if (HttpContext.Session.GetObject<List<int>>("TopicList") != null)
            {
                List<int>? selectedtopics = HttpContext!.Session.GetObject<List<int>>("TopicList");
                if(selectedtopics != null)
                {
                    if (!selectedtopics.Contains(topicid))
                    {
                        selectedtopics.Add(topicid);
                    }
                    else
                    {
                        selectedtopics.Remove(topicid);
                    }
                    HttpContext.Session.SetObject("TopicList", selectedtopics);
                }

            }
            else
            {
                List<int> topics = new List<int> {topicid};
                HttpContext.Session.SetObject("TopicList", topics);
            }

            return new EmptyResult();
        }
        /// <summary>
        /// Tracks Checked Topic list
        /// </summary>
        /// <param name="replyid">Id of checked <see cref="Topic"/></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator,Moderator")]
        [HttpPost]
        [Route("Topic/UpdateReplyList/")]
        public EmptyResult UpdateReplyList(int replyid)
        {
            if (HttpContext.Session.GetObject<List<int>>("ReplyList") != null)
            {
                List<int>? selectedtopics = HttpContext.Session.GetObject<List<int>>("ReplyList");
                if(selectedtopics != null)
                {
                    if (!selectedtopics.Contains(replyid))
                    {
                        selectedtopics.Add(replyid);
                    }
                    else
                    {
                        selectedtopics.Remove(replyid);
                    }
                    HttpContext.Session.SetObject("ReplyList", selectedtopics);
                }

            }
            else
            {
                List<int> topics = new List<int> {replyid};
                HttpContext.Session.SetObject("ReplyList", topics);
            }

            return new EmptyResult();
        }
        /// <summary>
        /// File upload handler for Topics
        /// </summary>
        /// <returns></returns>
        [Route("Topic/Upload/")]
        //[HttpPost]
        public IActionResult Upload(AlbumUploadViewModel model)
        {
            var uploadFolder = Combine(_config.ContentFolder, "Members");
            var currentMember = _memberService.Current();
            if (currentMember != null)
            {
                uploadFolder = Combine(uploadFolder, currentMember.Id.ToString());
            }
            else
            {
                return View("Error");
            }
            var path = $"{uploadFolder}".Replace("/","\\");
            uploadFolder = _config.RootFolder + "/" + uploadFolder;

            if (ModelState.IsValid)
            {
                var uniqueFileName = GetUniqueFileName(model.AlbumImage.FileName, out string timestamp);
                var uploads = Path.Combine(_environment.WebRootPath, path);
                var filePath = Path.Combine(uploads, uniqueFileName);
                var fStream = new FileStream(filePath, FileMode.Create);
                model.AlbumImage.CopyTo(fStream);
                fStream.Flush();
                return Json(new { result = true, data = Combine(uploadFolder,uniqueFileName),filesize= model.AlbumImage.Length/1024,type = Path.GetExtension(model.AlbumImage.FileName) });
                //return Json(uniqueFileName + "|" + model.Description);
            }

            return PartialView("popUpload",model);

        }

        [Route("Topic/UploadForm/")]
        public IActionResult UploadForm()
        {
            ViewBag.Title = "lblUpload";
            return PartialView("popUpload",new AlbumUploadViewModel());
        }

        [HttpPost]
        [Authorize]
        [Route("Topic/MakeSticky/")]
        public async Task<IActionResult> MakeSticky(int id, int state)
        {
            var result = await _postService.MakeSticky(id, (short)state);
            return result ? Json(new { result, data = id }) : Json(new { result, error = "Unable to toggle Status" });
        }

        private Post BuildPost(NewPostModel model, int memberid)
        {
            if (model.TopicId != 0)
            {
                return _postService.GetTopicForUpdate(model.TopicId);
            }

            var donotModerate = User.IsInRole("Administrator") || User.IsInRole("Forum_" + model.ForumId);
            var forum = _forumService.GetWithPosts(model.ForumId);
            return new Post()
            {
                Id = model.TopicId,
                Title = model.Title,
                Content = model.Content,
                Created = DateTime.UtcNow.ToForumDateStr(),
                MemberId = memberid,
                //Member = user,
                ForumId = model.ForumId,
                CategoryId = model.CatId,
                AllowRating= (short)(model.AllowRating ? 1 : 0),
                IsSticky = (short)(model.Sticky ? 1 : 0),
                ArchiveFlag = model.DoNotArchive ? 1 : 0,
                Status = (forum.Moderation == Moderation.AllPosts || forum.Moderation == Moderation.Topics) && !(donotModerate)
                ? (short)Status.UnModerated
                : (short)Status.Open,
                Sig = (short)(model.UseSignature ? 1 : 0),
            };

        }
        private PostReply? BuildReply(NewPostModel model, int memberid)
        {
            if (model.Id != 0)
            {
                return _postService.GetReplyForUdate(model.Id);
            }

            var donotModerate = User.IsInRole("Administrator") || User.IsInRole("Forum_" + model.ForumId);
            var forum = _forumService.GetWithPosts(model.ForumId);
            return new PostReply()
            {
                Id = model.Id,
                Content = model.Content,
                Created = DateTime.UtcNow.ToForumDateStr(),
                Status = (forum.Moderation == Moderation.AllPosts) && !(donotModerate)
                ? (short)Status.UnModerated
                : (short)Status.Open,
                MemberId = memberid,
                PostId = model.TopicId,
                ForumId = model.ForumId,
                CategoryId = model.CatId,
                Sig = (short)(model.UseSignature ? 1 : 0),
                Answer = model.Answer
            };

        }
        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<PostReply> replies)
        {
            return replies.Select(reply => new PostReplyModel()
            {
                Id = reply.Id,
                AuthorId = reply.Member!.Id,
                Author = reply.Member,
                AuthorName = reply.Member.Name,
                Answer = reply.Answer,
                //AuthorImageUrl = reply.User.ProfileImageUrl,
                Created = reply.Created.FromForumDateStr(),
                Content = reply.Content,
                AuthorPosts = reply.Member.Posts,
                AuthorRole = reply.Member.Level,
                Edited = reply.LastEdited?.FromForumDateStr(),
                Status = reply.Status,
                ShowSig = reply.Sig == 1,
                EditedBy = reply.LastEditby == null ? "" : _memberService.GetMemberName(reply.LastEditby.Value)
            });
        }
        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<ArchivedReply> replies)
        {
            return replies.Select(reply => new PostReplyModel()
            {
                Id = reply.Id,
                AuthorId = reply.Member!.Id,
                Author = reply.Member,
                AuthorName = reply.Member.Name,
                Answer = false,
                Created = reply.Created.FromForumDateStr(),
                Content = reply.Content,
                AuthorPosts = reply.Member.Posts,
                AuthorRole = reply.Member.Level,
                Edited = reply.LastEdited?.FromForumDateStr(),
                ShowSig = reply.Sig == 1,
                EditedBy = reply.LastEditby == null ? "" : _memberService.GetMemberName(reply.LastEditby.Value)
            });
        }
        private PagedList<PostReply>? PagedReplies(int page, int pagesize, string sortdir, Post post)
        {
            if(post.ReplyCount < 1 && post.UnmoderatedReplies < 1) return null;
            var pagedReplies = sortdir == "asc"
                ? new PagedList<PostReply>(post.Replies.OrderBy(r => r.Created), page, pagesize)
                : new PagedList<PostReply>(post.Replies.OrderByDescending(r => r.Created), page, pagesize);
            return pagedReplies;
        }
        private PagedList<ArchivedReply>? PagedReplies(int page, int pagesize, string sortdir, ArchivedPost post)
        {
            if(post.ReplyCount < 1) return null;
            if(!post.Replies.Any()) return null;

            var pagedReplies = sortdir == "asc"
                ? new PagedList<ArchivedReply>(post.Replies.OrderBy(r => r.Created), page, pagesize)
                : new PagedList<ArchivedReply>(post.Replies.OrderByDescending(r => r.Created), page, pagesize);
            return pagedReplies;
        }
        private string GetUniqueFileName(string fileName, out string timestamp)
        {
            fileName = Path.GetFileName(fileName);
            timestamp = DateTime.UtcNow.ToForumDateStr();
            return  timestamp
                    + "_"
                    + GetSafeFilename(fileName);
        }
        private static string GetSafeFilename(string filename)
        {

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

        }

        private bool CheckAuthorisation(ForumAuthType auth,bool signedin, bool ismoderator, bool isadministrator, ref bool passwordrequired)
        {
            bool notallowed = false;
            switch (auth)
            {
                case ForumAuthType.AllowedMembers:
                    if (signedin && (ismoderator || isadministrator))
                    {
                        break;
                    }
                    notallowed = true;
                    break;
                case ForumAuthType.PasswordProtected:
                    passwordrequired = true;
                    break;
                case ForumAuthType.AllowedMemberPassword:
                    if (signedin && (ismoderator || isadministrator))
                    {
                        passwordrequired = true;
                        break;
                    }
                    notallowed = true;
                    break;
                case ForumAuthType.Members:
                    if (signedin)
                        break;
                    notallowed = true;
                    break;
                case ForumAuthType.MembersHidden:
                    if (signedin)
                        break;
                    notallowed = true;
                    break;
                case ForumAuthType.AllowedMembersHidden:
                    if (signedin && (ismoderator || isadministrator))
                    {
                        break;
                    }
                    notallowed = true;
                    break;
                case ForumAuthType.MembersPassword:
                    if (signedin)
                    {
                        passwordrequired = true;
                        break;
                    }
                    notallowed = true;
                    break;
                default:
                    break;

            }

            return notallowed;
        }

    }
}

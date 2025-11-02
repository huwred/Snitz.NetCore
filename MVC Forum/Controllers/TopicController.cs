using BbCodeFormatter;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Post;
using Org.BouncyCastle.Asn1.X509;
using SmartBreadcrumbs.Nodes;
using Snitz.Events.Models;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

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
        private readonly ISnitzCookie _cookie;

        public TopicController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IPost postService, IForum forumService, UserManager<ForumUser> userManager,IWebHostEnvironment environment,
            IEmailSender mailSender, ISubscriptions processSubscriptions, ICodeProcessor bbcodeProcessor,
            IPrivateMessage pmService,EventContext eventsContext,ISnitzCookie cookie) : base(memberService, config, localizerFactory,dbContext, httpContextAccessor)
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
            _cookie = cookie;
        }

        [Route("Topic/{id}")]
        [Route("Topic/Index/{id}")]
        [Route("Topic/Posts/{id}")]
        public IActionResult Index(int id,int page = 1, int pagesize = 0, string sortdir="", int? replyid = null)
        {
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
            
            bool passwordrequired = false;
            bool notallowed = false;
            bool isadministrator = User.IsInRole("Administrator");
            bool signedin = false;
            ViewBag.RequireAuth = false;
            if (User.Identity is { IsAuthenticated: true })
            {
                signedin = true;
            }

            if(sortdir == "")
            {
                sortdir = _config.GetValueWithDefault("STRTOPICSORT","asc");
            }

            var post = _postService.GetTopicAsync(id).Result;

            if(post == null)
            {
                ViewBag.Error = "No Topic Found with that ID";
                return View("Error");
            }  
            if (!HttpContext.Session.Keys.Contains("TopicId_"+ id))
            {
                HttpContext.Session.SetInt32("TopicId_"+ id,1);
                _postService.UpdateViewCount(id, post.ViewCount + 1);
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
            bool ismoderator = User.IsInRole($"Forum_{post.ForumId}");
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

            if(_memberService.Current() != null)
            { 
                _cookie.UpdateTopicTrack(post.Id.ToString()); 
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
                Topic = post,
                Title = post.Title,
                Author = post.Member!,
                AuthorId = post.Member!.Id,
                ShowSig = post.Sig == 1,
                Views = post.ViewCount,
                Status = post.Status,
                IsLocked = post.Status == 0 || post.Forum?.Status == 0,
                IsSticky = post.IsSticky == 1,
                HasPoll = post.Ispoll == 1,
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
                Rating = post.GetTopicRating(),
                AccessDenied = notallowed,

            };
            if(_config.IsEnabled("STRSHOWTOPICNAV"))
            {
                var res = GetAdjacentTopics(post.Id,post.Forum!.Id);
                model.PreviousTopicId = (int?)res.PreviousId;
                model.NextTopicId = (int?)res.NextId;
            }            

            if(post.Forum.Type == (int)ForumType.BlogPosts)
            {
                return View("Blog/Index",model);
            }
            else
            {
                return View(model);
            }

        }

        /// <summary>
        /// Retrieves the IDs of the topics adjacent to the specified topic in the given forum.
        /// </summary>
        /// <remarks>The method assumes that topics in the forum are ordered in a predefined sequence. If
        /// the specified topic ID does not exist in the forum, the method will return <see langword="null"/> for both
        /// <c>PreviousId</c> and <c>NextId</c>.</remarks>
        /// <param name="id">The ID of the current topic.</param>
        /// <param name="forumId">The ID of the forum containing the topics.</param>
        /// <returns>An object containing the IDs of the adjacent topics: <list type="bullet"> <item>
        /// <description><c>PreviousId</c>: The ID of the topic immediately preceding the current topic, or <see
        /// langword="null"/> if the current topic is the first in the list.</description> </item> <item>
        /// <description><c>NextId</c>: The ID of the topic immediately following the current topic, or <see
        /// langword="null"/> if the current topic is the last in the list.</description> </item> </list></returns>
        private dynamic GetAdjacentTopics(int id, int forumId)
        {
            // Example list of record IDs
            var recordIds = _forumService.TopicIds(forumId).AsEnumerable();

            int index = recordIds.Select((value, idx) => new { value, idx })
                               .Where(x => x.value == id)
                               .Select(x => x.idx)
                               .FirstOrDefault();
            // Get previous and next elements
            int? previous = index > 0 ? recordIds.ElementAt(index - 1) : (int?)null;
            int? next = index < recordIds.Count() - 1 ? recordIds.ElementAt(index + 1) : (int?)null;

            return new { PreviousId = previous, NextId = next };
        }

        /// <summary>
        /// Displays the details of a blog post, including its replies, metadata, and related information.
        /// </summary>
        /// <remarks>This action retrieves the blog post and its associated replies, applies pagination
        /// and sorting, and checks user permissions for accessing the post. If the post requires authentication or a
        /// password, the appropriate flags are set in the view model.</remarks>
        /// <param name="id">The unique identifier of the blog post to display.</param>
        /// <param name="page">The page number of replies to display. Defaults to 1.</param>
        /// <param name="pagesize">The number of replies to display per page. Defaults to 0, which uses the session's page size or a default
        /// value.</param>
        /// <param name="sortdir">The sort direction for replies. Defaults to an application-configured value if not specified.</param>
        /// <param name="replyid">The identifier of a specific reply to highlight. If the reply does not exist in the topic, this parameter is
        /// ignored.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the blog post view with the specified details, or an error view
        /// if the post is not found or access is denied.</returns>
        [Route("Blog/{id}")]
        public IActionResult Index(string id,int page = 1, int pagesize = 0, string sortdir="", int? replyid = null)
        {
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
            
            bool passwordrequired = false;
            bool notallowed = false;
            bool isadministrator = User.IsInRole("Administrator");
            bool signedin = false;
            ViewBag.RequireAuth = false;
            if (User.Identity is { IsAuthenticated: true })
            {
                signedin = true;
            }

            if(sortdir == "")
            {
                sortdir = _config.GetValueWithDefault("STRTOPICSORT","asc");
            }

            var post = _postService.GetTopicAsync(id).Result;

            if(post == null)
            {
                ViewBag.Error = "No Topic Found with that ID";
                return View("Error");
            }  
            if (!HttpContext.Session.Keys.Contains("TopicId_"+ id))
            {
                HttpContext.Session.SetInt32("TopicId_"+ id,1);
                _postService.UpdateViewCount(post.Id, post.ViewCount + 1);
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
            bool ismoderator = User.IsInRole($"Forum_{post.ForumId}");
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
                post = _postService.GetTopicWithRelated(post.Id).Result;
            }

            if(_memberService.Current() != null)
            { 
                _cookie.UpdateTopicTrack(post.Id.ToString()); 
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
            var topicPage = new MvcBreadcrumbNode("", "Blog", post.Title) { Parent = forumPage };
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
                Topic = post,
                Title = post.Title,
                Author = post.Member!,
                AuthorId = post.Member!.Id,
                ShowSig = post.Sig == 1,
                Views = post.ViewCount,
                Status = post.Status,
                IsLocked = post.Status == 0 || post.Forum?.Status == 0,
                IsSticky = post.IsSticky == 1,
                HasPoll = post.Ispoll == 1,
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
                Rating = post.GetTopicRating(),
                AccessDenied = notallowed,
            };
            if(post.Forum.Type == (int)ForumType.BlogPosts)
            {
                return View("Blog/Index",model);
            }
            else
            {
                return View(model);
            }

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

                _postService.UpdateArchivedViewCount(post.ArchivedPostId,post!.ViewCount + 1);
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
            var forumPage = new MvcBreadcrumbNode("Archived", "Forum", post.Forum?.Title){ Parent = catPage,RouteValues = new{id=post.ForumId}};
            var topicPage = new MvcBreadcrumbNode("Archived", "Topic", post.Subject) { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;
            
            ViewData["Title"] = post.Subject;
            var totalCount = post.ArchivedReplies.Count();
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
                Id = post.ArchivedPostId,
                ArchivedTopic = post,
                Title = post.Subject,
                Author = post.Member!,
                AuthorId = post.Member!.Id,
                ShowSig = post.Sig == 1 && !string.IsNullOrWhiteSpace(post.Member?.Signature),
                Views = post.ViewCount,
                Status = post.Status,
                IsLocked = post.Status == 0 || post.Forum?.Status == 0,
                IsSticky = post.IsSticky == 1,
                HasPoll = post.Ispoll == 1,
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

        /// <summary>
        /// Displays the form for creating a new post in the specified forum.
        /// </summary>
        /// <remarks>This method enforces various access control rules, including flood control checks and
        /// forum-specific posting permissions. Users must have the appropriate role or authorization level to create a
        /// post in restricted forums.</remarks>
        /// <param name="id">The unique identifier of the forum where the post will be created.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the appropriate view for creating a new post. If the user does
        /// not have the required permissions or violates flood control rules, an error view is returned.</returns>
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
            var forumAuth = forum.Postauth ?? 0;
            if(forumAuth != (int)PostAuthType.Anyone)
            {
                var moderator = User.IsInRole("Forum_" + id);
                var admin = User.IsInRole("Administrator");
                ViewBag.Error = _languageResource.GetString($"lblPostAuthType_{(PostAuthType)forumAuth} in this Forum");

                if (!(admin || moderator))
                {
                    TempData["Error"] = "restricted";
                    return View("Error");
                }
                if(forum.Postauth == (int)PostAuthType.Admins && !admin)
                {
                    TempData["Error"] = "restricted";
                    return View("Error");
                }
            }
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
                IsLocked = false,
                IsSticky = false,
                DoNotArchive = false,
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name){ Parent = homePage,RouteValues = new{id=forum.CategoryId}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Create", "Topic", "New Post") { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;
            ViewData["AllowDraft"] = true;
            if (forum.Type == (int)ForumType.BugReports)
            {
                return View("Bug/Create", model);
            }
            return await Task.Run(() => View(model));
        }

        /// <summary>
        /// Prepares a new post model for replying to a topic with a quoted message and renders the post creation view.
        /// </summary>
        /// <remarks>This method retrieves the topic and its related data, including the forum and the
        /// current user, to populate the reply model. The reply includes a quoted version of the original topic
        /// content. Breadcrumb navigation data is also prepared for the view.</remarks>
        /// <param name="id">The unique identifier of the topic to reply to.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the post creation view with the pre-filled model for replying to
        /// the topic.</returns>
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
                IsLocked = topic.Status == 0,
                IsSticky = topic.IsSticky == 1,
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

        /// <summary>
        /// Displays the edit view for an existing topic, allowing the user to modify its content and settings.
        /// </summary>
        /// <remarks>This action requires the user to be authorized. The method retrieves the topic, its
        /// associated forum, and the current user's details  to populate the edit view model. The view model includes
        /// various properties such as the topic's title, content, status, and other  metadata. Breadcrumb navigation is
        /// also configured for the view.</remarks>
        /// <param name="id">The unique identifier of the topic to be edited.</param>
        /// <param name="archived">A value indicating whether the topic is archived. If <see langword="true"/>, the topic is marked as
        /// archived.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the edit view with the pre-populated topic data.</returns>
        [Authorize]
        public async Task<IActionResult> Edit(int id, bool archived)
        {

            var member = await _memberService.GetById(User);
            var topic = await _postService.GetTopicWithRelated(id);
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
                IsLocked = topic.Status == 0,
                IsSticky = topic.IsSticky == 1,
                SaveDraft = topic.Status == 99,
                AllowRating = topic.Forum.Rating == 1,
                AllowTopicRating = topic.AllowRating == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = topic.Created.FromForumDateStr(),
                IsAuthor = member!.Id == topic.MemberId,
                IsArchived = archived,
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name){ Parent = homePage,RouteValues = new{id=forum.Category!.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Edit", "Topic", "Edit Post") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;
            if (topic.Ispoll == 1)
            {
                TempData["HasPoll"] = true;
                TempData["Poll"] = _postService.GetPoll(id);
            };
            ViewData["AllowDraft"] = model.SaveDraft;
            return View("Create", model);

        }

        /// <summary>
        /// Creates a new post or topic in the forum based on the provided model.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated and authorized. It performs flood
        /// control checks  to prevent excessive posting within a short time frame, validates the provided model, and
        /// handles  both draft and published posts. If the post mentions other users, private messages may be sent to 
        /// notify them.</remarks>
        /// <param name="model">The <see cref="NewPostModel"/> containing the details of the post to be created.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the operation.  Returns a JSON object
        /// containing the URL and ID of the created post if successful,  or an error view if the operation fails due to
        /// validation or other constraints.</returns>
        [HttpPost]
        [Authorize]
        [Route("AddPost/")]
        public IActionResult AddPost(NewPostModel model)
        {
            ModelState.Remove("ForumName");
            ModelState.Remove("AuthorName");
            ModelState.Remove("AuthorImageUrl");
            ModelState.Remove("AuthorTitle");
            ModelState.Remove("Sticky");
            ModelState.Remove("Lock");
            ModelState.Remove("IsSticky");
            ModelState.Remove("IsLocked");
            ModelState.Remove("DoNotArchive");
            var wasdraft = false;
            //Check for flood control
            if (_config.GetIntValue("STRFLOODCHECK") == 1 && !User.IsInRole("Administrator"))
            {
                var member = _memberService.GetById(User).Result;
                var timeout = _config.GetIntValue("STRFLOODCHECKTIME", 30);
                if (member!.Lastpostdate.FromForumDateStr() > DateTime.UtcNow.AddSeconds(-timeout))
                {
                    TempData["Error"] = "floodcheck";
                    ViewBag.Error = _languageResource.GetString("FloodcheckErr", timeout);
                    return View("Error");

                }
            }
            //Check for required fields
            if (!ModelState.IsValid)
            {
                var errorList = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                //TempData["Error"] = string.Join(Environment.NewLine, errorList);
                //ViewBag.Error = string.Join(Environment.NewLine, errorList);
                return Content(string.Join(Environment.NewLine, errorList));
                //return View("Error");
            }
            var userId = _userManager.GetUserId(User);
            var user =  _userManager.FindByIdAsync(userId!).Result;

            var post = BuildPost(model, user!.MemberId);
            if(model.SaveDraft)
            {
                post.Status = 99; //draft
            }
            if (model.TopicId != 0)
            {
                var originaltopic = _postService.GetTopic(model.TopicId);
                if (originaltopic != null) {
                    wasdraft = originaltopic.Status == 99;
                }

                if(!model.SaveDraft && post.Status != 99)
                {
                    post.Status = (short)(model.IsLocked ? 0 : post.Status);
                }
                else if(model.SaveDraft)
                {
                    post.Status = 99;
                }
                else
                {
                    post.Status = (short)(model.IsLocked ? 0 : 1);
                    wasdraft = true;
                }
                post.LastEdit = DateTime.UtcNow.ToForumDateStr();
                post.LastEditby = user.MemberId;
                //We are moving the topic so need to update stuff
                if (post.ForumId != model.ForumId)
                {
                    var forum = _forumService.Get(model.ForumId);// _snitzDbContext.Forums.AsNoTracking().OrderBy(f=>f.Id).First(f => f.Id == model.ForumId);
                    
                    post.ForumId = model.ForumId;
                    post.CategoryId = forum.CategoryId;
                     _postService.Update(post);
                    //Update the ForumId and CategoryId for the replies
                    var replies = _snitzDbContext.Replies.Where(r => r.PostId == model.TopicId);
                    replies
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(e => e.ForumId, forum.Id)
                            .SetProperty(e => e.CategoryId, forum.CategoryId));
                     _forumService.UpdateLastPost(model.ForumId);
                    if (_config.GetIntValue("STRMOVENOTIFY") == 1)
                    {
                        var author = _memberService.GetById(post.MemberId);
                         _mailSender.MoveNotify(author!,post);
                    }
                }
                else
                {
                    if(post.Status != (int)Status.Draft && wasdraft)
                    {
                        post.Created = DateTime.UtcNow.ToForumDateStr();
                        _postService.Update(post);

                        _postService.UpdateLastPost(post.Id,null,wasdraft);
                    }
                    else
                    {
                        _postService.Update(post);
                    }
                }
            }
            else
            {
                post.Created = DateTime.UtcNow.ToForumDateStr();

                var topicid =  _postService.Create(post).Result;
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
                    //if we are sending PMs to users mentioned in the post
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
                                    From = post.Member,
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
        
        /// <summary>
        /// Adds or updates a poll and its associated answers in the database.
        /// </summary>
        /// <remarks>If a poll with the specified ID already exists, it is updated with the new details.
        /// Otherwise, a new poll is created. The method also ensures that the associated topic is marked as a poll
        /// topic.</remarks>
        /// <param name="poll">The <see cref="Poll"/> object containing the poll details, including the question, voting rules, and
        /// answers.</param>
        /// <returns>A JSON result containing the URL of the topic associated with the poll and the topic ID.</returns>
        [HttpPost]
        [Authorize]
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
                var topic = _snitzDbContext.Posts.Find(poll.TopicId);
                if (topic != null){
                    topic.Ispoll = 1;
                    _snitzDbContext.Posts.Update(topic);
                    _snitzDbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
               
            return Json(new{url=Url.Action("Index", "Topic", new { id = poll.TopicId }),id = poll.TopicId});
        }

        /// <summary>
        /// Deletes a topic from the forum, either archived or active, based on the specified parameters.
        /// </summary>
        /// <remarks>Only users with the "Administrator" role or the owner of the topic are authorized to
        /// delete the topic.  If the topic is archived, it must exist in the archived topics collection to be deleted. 
        /// For active topics, any associated polls or events are also removed during the deletion process.</remarks>
        /// <param name="id">The unique identifier of the topic to delete.</param>
        /// <param name="archived">A value indicating whether the topic to delete is archived.  If <see langword="true"/>, the method attempts
        /// to delete an archived topic; otherwise, it deletes an active topic.</param>
        /// <returns>A JSON result indicating the success or failure of the operation.  If successful, the result includes a URL
        /// to the forum index page.  If unsuccessful, the result includes an error message.</returns>
        [HttpPost]
        [Authorize]
        [Route("Topic/Delete/")]
        public async Task<IActionResult> Delete(int id, bool archived)
        {
            var member = _memberService.GetById(User).Result;
            if (archived)
            {
                var archivedpost = _postService.GetArchivedTopic(id);
                if (archivedpost == null)
                {
                    ModelState.AddModelError("","Unable to delete this topic");
                    return Json(new { result = false, error = "Unable to delete this topic" });
                }
                if (member != null && (member.Roles.Contains("Administrator") || archivedpost.MemberId == member.Id))
                {
                    await _postService.DeleteArchivedTopic(id);
                    return Json(new { result = true, url = Url.Action("Index", "Forum", new { id = archivedpost.ForumId }) });
                }
                ModelState.AddModelError("","Unable to delete this topic");
                return Json(new { result = false, error = "Unable to delete this topic" });
            }
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

        /// <summary>
        /// Toggles the "answered" status of a topic with the specified identifier.
        /// </summary>
        /// <remarks>This action requires the caller to be authorized. The endpoint is accessible via a
        /// POST request to "Topic/Answered/".</remarks>
        /// <param name="id">The unique identifier of the topic whose status is to be toggled.</param>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object with the operation result.  If successful, the JSON
        /// object includes the topic ID and the result status.  Otherwise, it includes an error message.</returns>
        [HttpPost]
        [Authorize]
        [Route("Topic/Answered/")]
        public async Task<IActionResult> Answered(int id)
        {
            var result = await _postService.Answer(id);
            return result ? Json(new { result = result, data = id }) : Json(new { result = result, error = "Unable to toggle Status" });

        }

        /// <summary>
        /// Locks or unlocks a topic based on the specified status.
        /// </summary>
        /// <remarks>This action requires the user to be authorized and have the "Administrator" role.  If
        /// the user does not have the required role, the operation will fail with an error message.</remarks>
        /// <param name="id">The unique identifier of the topic to be locked or unlocked.</param>
        /// <param name="status">The status indicating whether to lock or unlock the topic.  A value of <see langword="1"/> locks the topic,
        /// and <see langword="0"/> unlocks it.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the operation.  If successful, the response includes
        /// the topic ID and the operation result.  Otherwise, it includes an error message.</returns>
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

        /// <summary>
        /// Verifies whether the provided password matches the password for the specified forum.
        /// </summary>
        /// <remarks>If the password is valid, it is stored in the session under the key
        /// "Pforum_{forumid}".</remarks>
        /// <param name="pwd">The password to validate against the forum's password.</param>
        /// <param name="forumid">The unique identifier of the forum to check.</param>
        /// <param name="topicid">The optional identifier of the topic within the forum. This parameter is not used in the current
        /// implementation.</param>
        /// <returns>A JSON result containing <see langword="true"/> if the password matches the forum's password; otherwise,
        /// <see langword="false"/>.</returns>
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

        /// <summary>
        /// Prepares and sends an email containing information about a topic or archived topic.
        /// </summary>
        /// <remarks>This action requires the user to be authorized. The email content is dynamically
        /// generated  based on the topic or archived topic details, including the sender's information and the topic
        /// title.</remarks>
        /// <param name="id">The unique identifier of the topic or archived topic to be sent.</param>
        /// <param name="archived">A value indicating whether the topic is archived.  Pass <see langword="1"/> for archived topics; otherwise,
        /// pass <see langword="0"/>.</param>
        /// <returns>A partial view containing the email preparation form if the operation is successful,  or an error view if
        /// the data cannot be loaded.</returns>
        [Authorize]
        public IActionResult SendTo(int id, int archived)
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
                em.ReturnUrl = topic!.ArchivedPostId.ToString();
                
                em.Subject = _languageResource["sendtoSubject", from.Name].Value;
                
                em.Message =
                    String.Format(
                        _languageResource["sendtoMessage"].Value,
                        _config.ForumTitle, _config.ForumUrl,
                        topic.ArchivedPostId, Request.Query["archived"], topic.Subject);
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
        
        /// <summary>
        /// Sends an email to a specified recipient using the provided email model.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and includes anti-forgery token
        /// validation. Upon successful email delivery, a success message is stored in <see cref="TempData"/>.</remarks>
        /// <param name="model">The <see cref="EmailViewModel"/> containing the email details, including the recipient's address, subject,
        /// and message.</param>
        /// <returns>A redirect to the "Index" action of the "Topic" controller, with the specified return URL and page number.</returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SendTo(EmailViewModel model)
        {
            _mailSender.SendToFriend( model);
            TempData["Success"] = "Email sent successfully";
            return RedirectToAction("Index", "Topic", new { id=model.ReturnUrl, pagenum = -1 });
        }

        /// <summary>
        /// Generates a printable view of a forum topic based on the specified topic ID.
        /// </summary>
        /// <remarks>This method determines whether the topic is archived or active and retrieves the
        /// appropriate data  for rendering. It also sets the moderator and administrator roles in the view data for use
        /// in the view.</remarks>
        /// <param name="id">The unique identifier of the topic to be printed.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the printable view of the topic.  If the topic is not found, an
        /// error view is returned.</returns>
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
                    Id = topic!.ArchivedPostId,
                    Title = topic.Subject,
                    Author = topic.Member!,
                    AuthorId = topic.Member!.Id,
                    ShowSig = topic.Sig == 1 && !string.IsNullOrWhiteSpace(topic.Member?.Signature),
                    Views = topic.ViewCount,
                    Status = topic.Status,
                    IsLocked = topic.Status == 0 || topic.Forum?.Status == 0,
                    IsSticky = topic.IsSticky == 1,
                    HasPoll = topic.Ispoll == 1,
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
                    Topic = topic,
                    Title = topic.Title,
                    Author = topic.Member!,
                    AuthorId = topic.Member!.Id,
                    ShowSig = topic.Sig == 1 && !string.IsNullOrWhiteSpace(topic.Member?.Signature),
                    Views = topic.ViewCount,
                    IsLocked = topic.Status == 0 || topic.Forum?.Status == 0,
                    IsSticky = topic.IsSticky == 1,
                    HasPoll = topic.Ispoll == 1,
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
        /// Moderates a forum topic based on the specified approval status.
        /// </summary>
        /// <remarks>This method allows moderators or administrators to approve, reject, or place a topic
        /// on hold.  Depending on the selected status, the topic's state is updated, and an optional email notification
        /// is sent to the topic's author. The method validates the input model before processing.</remarks>
        /// <param name="vm">The view model containing the topic ID, the desired post status, an optional approval message,  and a flag
        /// indicating whether to send an email notification to the topic author.</param>
        /// <returns>An asynchronous operation that returns an <see cref="ActionResult"/>.  If the operation is successful, a
        /// JSON result indicating success is returned; otherwise, a partial view is rendered.</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        [ValidateAntiForgeryToken]
        [Route("Topic/ModeratePost/")]
        public async Task<ActionResult> ModeratePost(ApproveTopicViewModal vm)
        {

            if (ModelState.IsValid)
            {
                var topic = _postService.GetTopicAsync(vm.Id).Result;
                var author = _memberService.GetById(topic.MemberId);
                var forum = _forumService.Get(topic.ForumId);
                var subject = "";
                var message = "";
                
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                var topicLink = Url.Action("Index", "Topic", new { id = topic.Id, pagenum=-1}, Request.Scheme);
                switch (vm.PostStatus)
                {
                    case "Approve" :
                        await _postService.SetStatus(vm.Id, Status.Open);
                        await _postService.UpdateLastPost(vm.Id,-1);
                        //update Forum
                        forum = _forumService.UpdateLastPost(topic.ForumId);
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
                    case "OnHold":
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
                return Json(new { result = true });
                //return RedirectToAction("Index", "Forum", new { id=topic.ForumId});
            }

            return PartialView("popModerate",vm);
        }

        /// <summary>
        /// Renders a partial view for moderating a topic.
        /// </summary>
        /// <remarks>This action is restricted to users with the "Administrator" or "Moderator"
        /// roles.</remarks>
        /// <param name="id">The unique identifier of the topic to be moderated.</param>
        /// <returns>A <see cref="PartialViewResult"/> that renders the "popModerate" partial view with the topic's moderation
        /// details.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public PartialViewResult Moderate(int id)
        {
            var topic = _postService.GetTopic(id);
            ApproveTopicViewModal vm = new ApproveTopicViewModal {Id = id, PostStatus = ((Status)topic.Status).ToString()};
            return PartialView("popModerate",vm);
        }

        /// <summary>
        /// Subscribes the current user to a specified topic.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated. The subscription links the current
        /// user to the specified topic,  as well as its associated category and forum.</remarks>
        /// <param name="id">The unique identifier of the topic to subscribe to.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns "OK" if the subscription is
        /// successful.</returns>
        [HttpGet]
        [Authorize]
        [Route("Topic/Subscribe/")]
        public IActionResult Subscribe(int id)
        {
            var topic = _postService.GetTopicAsync(id).Result;
            var member = _memberService.Current();
            _snitzDbContext.MemberSubscriptions.Add(new MemberSubscription()
            {
                MemberId = member!.Id,
                CategoryId = topic!.CategoryId,
                ForumId = topic.ForumId,
                PostId = id
            });
            _snitzDbContext.SaveChanges();
            return Content("OK");
        }

        /// <summary>
        /// Unsubscribes the current user from a specific topic.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated. The method removes the
        /// subscription for the current user  to the specified topic, identified by its unique ID.</remarks>
        /// <param name="id">The unique identifier of the topic to unsubscribe from.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns "OK" if the operation is
        /// successful.</returns>
        [HttpGet]
        [Authorize]
        [Route("Topic/UnSubscribe/")]
        public IActionResult UnSubscribe(int id)
        {
            var member = _memberService.Current();
            _snitzDbContext.MemberSubscriptions.Where(s => s.MemberId == member!.Id && s.PostId == id).ExecuteDelete();
            return Content("OK");
        }

        /// <summary>
        /// Merges multiple topics into a single topic.
        /// </summary>
        /// <remarks>This action is restricted to users with the "Administrator" or "Moderator" roles. 
        /// The method performs additional processing for subscriptions based on the application's
        /// configuration.</remarks>
        /// <param name="selected">An array of topic IDs to be merged. Must contain at least two topic IDs.  If null or fewer than two IDs are
        /// provided, the method returns a bad request response.</param>
        /// <returns>A JSON result containing the URL of the newly merged topic if the operation is successful.  Returns a bad
        /// request response with an error message if the operation fails.</returns>
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

        /// <summary>
        /// Displays a view for splitting a topic into a new topic, allowing the user to select replies to move.
        /// </summary>
        /// <remarks>This action is restricted to users with the "Administrator" or "Moderator" roles. The
        /// view model includes the topic details, a list of replies, and a list of available forums  to which the new
        /// topic can be moved. Breadcrumb navigation is also set up for the view.</remarks>
        /// <param name="id">The unique identifier of the topic to be split.</param>
        /// <param name="replyid">The unique identifier of the reply to highlight in the view.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the view for splitting the topic.  Returns the "Error" view if
        /// the specified topic is not found.</returns>
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

        /// <summary>
        /// Splits an existing topic into a new topic based on the selected replies.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and have either the "Administrator"
        /// or "Moderator" role.  It validates the anti-forgery token and ensures that at least one reply is selected
        /// for the split operation.</remarks>
        /// <param name="vm">The view model containing the details of the topic to be split, including the selected replies, target
        /// forum, and new subject.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the operation.  Returns the updated view if the
        /// model state is invalid, an error view if no replies are selected,  or redirects to the new topic's index
        /// page upon successful split.</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        [ValidateAntiForgeryToken]
        [Route("Topic/SplitTopic/")]
        public async Task<IActionResult> SplitTopicAsync(SplitTopicViewModel vm)
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

                await _postService.UpdateLastPost(topic!.Id,0);
                _forumService.UpdateLastPost(topic.ForumId);
                await _postService.UpdateLastPost(originaltopic!.Id, 0);
                //        EmailController.TopicSplitEmail(ControllerContext, topic);
            }

            if (topic != null) return RedirectToAction("Index", new { id = topic.Id, pagenum = 1 });
            ViewBag.Error = "Unknown problem";
            return View("Error");
        }

        /// <summary>
        /// Saves a rating for a topic based on the submitted form data.
        /// </summary>
        /// <remarks>This method updates the topic's total rating and rating count, and records the rating
        /// in the database.  The "PostRating" value must be a valid numeric value, and the "TopicId" and "MemberId"
        /// must correspond to existing entities.</remarks>
        /// <param name="form">The form collection containing the rating data. Must include the keys "PostRating", "TopicId", and
        /// "MemberId".</param>
        /// <returns>A JSON result indicating the success or failure of the operation.  Returns a success message if the rating
        /// is saved successfully, or an error message if the required "PostRating" key is not found.</returns>
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
        /// Updates the list of selected topic IDs stored in the current user's session.
        /// </summary>
        /// <remarks>If the session already contains a list of topic IDs, the method adds the specified
        /// <paramref name="topicid"/>          to the list if it is not already present. If the <paramref
        /// name="topicid"/> is already in the list, it is removed.          If no topic list exists in the session, a
        /// new list is created containing the specified <paramref name="topicid"/>.</remarks>
        /// <param name="topicid">The ID of the topic to add to or remove from the session-based topic list.</param>
        /// <returns>An empty result indicating that the operation completed successfully.</returns>
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
        /// Updates the reply list stored in the current session by adding or removing the specified reply ID.
        /// </summary>
        /// <remarks>If the reply list does not exist in the session, a new list is created and the
        /// specified reply ID is added to it. If the reply list already exists, the method adds the reply ID to the
        /// list if it is not already present;  otherwise, it removes the reply ID from the list.</remarks>
        /// <param name="replyid">The ID of the reply to add to or remove from the reply list.</param>
        /// <returns>An empty result.</returns>
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
        /// Handles the upload of an album image for the current member.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated and authorized. The uploaded file
        /// is saved to a member-specific directory  within the configured content folder. The method ensures that the
        /// file name is unique and returns the file's URL, size, and type  in the JSON response upon successful
        /// upload.</remarks>
        /// <param name="model">The <see cref="UploadViewModel"/> containing the album image file and associated metadata.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the upload operation.  Returns a JSON response
        /// with the file details if the upload is successful, a partial view if the model state is invalid,  or an
        /// error view if the current member is not authenticated.</returns>
        [Route("Topic/Upload/")]
        [Authorize]
        public IActionResult Upload(UploadViewModel model)
        {
            var AllowedTypes=_config.GetValue("STRFILETYPES").Split(',').ToList();

            if (!AllowedTypes.Contains(Path.GetExtension(model.AlbumImage.FileName)))
            {
                ModelState.AddModelError("AlbumImage", "Invalid file type");
                return PartialView("popUpload",model);
            }

            var uploadFolder = StringExtensions.UrlCombine(_config.ContentFolder, "Members");
            var currentMember = _memberService.Current();
            if (currentMember != null)
            {
                uploadFolder = StringExtensions.UrlCombine(uploadFolder, currentMember.Id.ToString());
            }
            else
            {
                return View("Error");
            }
            //var path = Path.Combine(_config.ContentFolder, "Members");// $"{uploadFolder}".Replace("/","\\");
            uploadFolder = _config.RootFolder + "/" + uploadFolder;

            if (ModelState.IsValid)
            {
                var uniqueFileName = GetUniqueFileName(model.AlbumImage.FileName, out string timestamp);
                var uploads = Path.Combine(_environment.WebRootPath, _config.ContentFolder, "Members", currentMember.Id.ToString());
                if(!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var filePath = Path.Combine(uploads, uniqueFileName);
                var fStream = new FileStream(filePath, FileMode.Create);
                model.AlbumImage.CopyTo(fStream);
                fStream.Flush();
                return Json(new { result = true, data = StringExtensions.UrlCombine(uploadFolder,uniqueFileName),filesize= model.AlbumImage.Length/1024,type = Path.GetExtension(model.AlbumImage.FileName) });
                //return Json(uniqueFileName + "|" + model.Description);
            }

            return PartialView("popUpload",model);

        }

        /// <summary>
        /// Renders a partial view for uploading a form.
        /// </summary>
        /// <remarks>This action method returns a partial view named <c>popUpload</c> with an <see
        /// cref="UploadViewModel"/>  preconfigured for uploading files. The view model specifies the controller,
        /// action, and allowed file types  for the upload operation.</remarks>
        /// <returns>A <see cref="PartialViewResult"/> containing the <c>popUpload</c> partial view and an  <see
        /// cref="UploadViewModel"/> instance.</returns>
        [Route("Topic/UploadForm/")]
        [Authorize]
        public IActionResult UploadForm()
        {
            ViewBag.Title = "lblUpload";
            return PartialView("popUpload",new UploadViewModel(){Controller="Topic",Action="Upload",AllowedTypes=_config.GetValue("STRFILETYPES")});
        }

        /// <summary>
        /// Toggles the sticky status of a topic.
        /// </summary>
        /// <remarks>This action requires the user to be authorized. Ensure the provided topic ID exists
        /// and the user has the necessary permissions to modify the topic.</remarks>
        /// <param name="id">The unique identifier of the topic to update.</param>
        /// <param name="state">The desired sticky state of the topic. Use <see langword="1"/> to make the topic sticky, or <see
        /// langword="0"/> to remove the sticky status.</param>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object with the operation result. If successful, the JSON
        /// object includes the topic ID. If unsuccessful, it includes an error message.</returns>
        [HttpPost]
        [Authorize]
        [Route("Topic/MakeSticky/")]
        public async Task<IActionResult> MakeSticky(int id, int state)
        {
            var result = await _postService.MakeSticky(id, (short)state);
            return result ? Json(new { result, data = id }) : Json(new { result, error = "Unable to toggle Status" });
        }

        /// <summary>
        /// Renders a preview of the specified topic using the "TopicPreview" view component.
        /// </summary>
        /// <param name="id">The unique identifier of the topic to preview.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the "TopicPreview" view component with the specified topic ID.</returns>
        public IActionResult TopicPreview(int id)
        {
            return ViewComponent("TopicPreview", new { id });
        }

        /// <summary>
        /// Renders a partial view displaying a paginated list of blog posts for a specified forum.
        /// </summary>
        /// <remarks>The method retrieves the forum and its associated posts using the provided forum ID. 
        /// If the forum does not exist, an error message is added to the <see cref="ViewBag"/> and  the "Error" partial
        /// view is returned. Otherwise, the blog posts are paginated with a  fixed page size of 50, and the resulting
        /// data is passed to the "_BlogList" partial view.</remarks>
        /// <param name="id">The unique identifier of the forum whose blog posts are to be displayed.</param>
        /// <returns>A <see cref="PartialViewResult"/> containing the "_BlogList" partial view populated with a  <see
        /// cref="ForumViewModel"/> that includes the forum details and its associated blog posts. If the forum is not
        /// found, returns the "Error" partial view with an error message.</returns>
        public PartialViewResult BlogList(int id)
        {

            var forum = _forumService.GetWithPosts(id);

            if (forum == null)
            {
                ViewBag.Error = "Forum not found";
                return PartialView("Error");
            }
            PagedList<Post> result = new PagedList<Post>(forum.Posts!,1,50); //forum.Posts(50, 1, User, WebSecurity.CurrentUserId, 120);

            ForumViewModel vm = new()
            {
                Forum = forum, 
                Topics = result.ToList()
            };
            vm.Id = forum.Id;
            vm.Forum = forum;
            vm.PageSize = 50;
            vm.StickyTopics = null;
            vm.TotalRecords = result.TotalItemCount;
            int pagecount = Convert.ToInt32(result.PageCount);
            vm.PageCount = pagecount;
            vm.Page = 1;
            return PartialView("_BlogList", vm);
        }

        /// <summary>
        /// Constructs a new <see cref="Post"/> instance based on the provided model and member ID.
        /// </summary>
        /// <remarks>The method determines the forum and category for the post based on the provided topic
        /// ID, if any. If the topic ID is not specified, the values from the model are used. The moderation status of
        /// the post is set based on the forum's moderation settings and the user's roles.</remarks>
        /// <param name="model">The data model containing the details of the post to be created, including title, content, and other
        /// attributes.</param>
        /// <param name="memberid">The unique identifier of the member creating the post.</param>
        /// <returns>A new <see cref="Post"/> object initialized with the specified data. The post's properties, such as forum,
        /// category, and status, are determined based on the input model and the current user's roles and permissions.</returns>
        private Post BuildPost(NewPostModel model, int memberid)
        {
            Post? originaltopic = null;
            if (model.TopicId != 0)
            {
                originaltopic = _postService.GetTopic(model.TopicId);
            }

            var donotModerate = User.IsInRole("Administrator") || User.IsInRole("Forum_" + model.ForumId);
            var forum = _forumService.Get(model.ForumId);

            return new Post()
            {
                Id = model.TopicId,
                Title = model.Title,
                Content = model.Content,
                Created = DateTime.UtcNow.ToForumDateStr(),
                MemberId = memberid,
                ForumId = originaltopic != null ? originaltopic.ForumId : model.ForumId,
                CategoryId = originaltopic != null ? originaltopic.CategoryId : model.CatId,
                AllowRating = (short)(model.AllowTopicRating ? 1 : 0),
                
                IsSticky = (short)(model.IsSticky ? 1 : 0),
                ArchiveFlag = model.DoNotArchive ? 1 : 0,
                Status = (forum.Moderation == Moderation.AllPosts || forum.Moderation == Moderation.Topics) && !(donotModerate)
                ? (short)Status.UnModerated
                : (model.IsLocked ? (short)Status.Closed : (short)Status.Open),
                Sig = (short)(model.UseSignature ? 1 : 0),
            };

        }

        /// <summary>
        /// Builds a collection of <see cref="PostReplyModel"/> objects from a collection of <see cref="PostReply"/>
        /// entities.
        /// </summary>
        /// <remarks>This method maps the properties of each <see cref="PostReply"/> entity to a
        /// corresponding <see cref="PostReplyModel"/> object. The transformation includes details about the author,
        /// content, creation date, and other metadata.</remarks>
        /// <param name="replies">The collection of <see cref="PostReply"/> entities to transform into models. Cannot be null.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PostReplyModel"/> objects, each representing a transformed
        /// reply.</returns>
        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<PostReply> replies)
        {
            return replies.Select(reply => new PostReplyModel()
            {
                Id = reply.Id,
                Reply = reply,

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
                ShowSig = reply.Sig == 1 && !string.IsNullOrWhiteSpace(reply.Member?.Signature),
                EditedBy = reply.LastEditby == null ? "" : _memberService.GetMemberName(reply.LastEditby.Value)
            });
        }

        /// <summary>
        /// Builds a collection of <see cref="PostReplyModel"/> objects from a collection of archived replies.
        /// </summary>
        /// <remarks>This method maps each <see cref="ArchivedReply"/> to a <see cref="PostReplyModel"/>
        /// by extracting relevant properties such as the reply content, author details, creation date, and other
        /// metadata. The method ensures that the resulting models are enriched with additional information, such as the
        /// author's role and post count, and handles optional fields like the last edited date and signature
        /// visibility.</remarks>
        /// <param name="replies">The collection of archived replies to transform into <see cref="PostReplyModel"/> objects.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PostReplyModel"/> objects, each representing a reply with its
        /// associated metadata and author details.</returns>
        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<ArchivedReply> replies)
        {
            return replies.Select(reply => new PostReplyModel()
            {
                Id = reply.Id,
                ArchivedReply = reply,
                AuthorId = reply.Member!.Id,
                Author = reply.Member,
                AuthorName = reply.Member.Name,
                Answer = false,
                Created = reply.Created.FromForumDateStr(),
                Content = reply.Content,
                AuthorPosts = reply.Member.Posts,
                AuthorRole = reply.Member.Level,
                Edited = reply.LastEdited?.FromForumDateStr(),
                ShowSig = reply.Sig == 1 && !string.IsNullOrWhiteSpace(reply.Member.Signature),
                EditedBy = reply.LastEditby == null ? "" : _memberService.GetMemberName(reply.LastEditby.Value)
            });
        }

        /// <summary>
        /// Retrieves a paginated list of replies for the specified post, sorted by creation date.
        /// </summary>
        /// <remarks>The replies are sorted by their creation date in the specified order. If the post has
        /// no replies and no unmoderated replies, the method returns <see langword="null"/>.</remarks>
        /// <param name="page">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pagesize">The number of replies per page. Must be greater than or equal to 1.</param>
        /// <param name="sortdir">The sort direction for the replies. Use "asc" for ascending or "desc" for descending order.</param>
        /// <param name="post">The post for which replies are retrieved. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="PagedList{T}"/> containing the replies for the specified post, or <see langword="null"/> if the
        /// post has no replies or unmoderated replies.</returns>
        private PagedList<PostReply>? PagedReplies(int page, int pagesize, string sortdir, Post post)
        {
            if(post.ReplyCount < 1 && post.UnmoderatedReplies < 1) return null;
            var pagedReplies = sortdir == "asc"
                ? new PagedList<PostReply>(post.Replies.OrderBy(r => r.Created), page, pagesize)
                : new PagedList<PostReply>(post.Replies.OrderByDescending(r => r.Created), page, pagesize);
            return pagedReplies;
        }

        /// <summary>
        /// Retrieves a paginated list of archived replies for the specified post, sorted in the specified direction.
        /// </summary>
        /// <param name="page">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pagesize">The number of replies per page. Must be greater than or equal to 1.</param>
        /// <param name="sortdir">The sort direction for the replies. Use "asc" for ascending or "desc" for descending.</param>
        /// <param name="post">The archived post for which replies are to be retrieved. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="PagedList{T}"/> containing the archived replies for the specified page and page size,  sorted
        /// in the specified direction. Returns <see langword="null"/> if the post has no replies or no archived
        /// replies.</returns>
        private PagedList<ArchivedReply>? PagedReplies(int page, int pagesize, string sortdir, ArchivedPost post)
        {
            if(post.ReplyCount < 1) return null;
            if(!post.ArchivedReplies.Any()) return null;

            var pagedReplies = sortdir == "asc"
                ? new PagedList<ArchivedReply>(post.ArchivedReplies.OrderBy(r => r.Created), page, pagesize)
                : new PagedList<ArchivedReply>(post.ArchivedReplies.OrderByDescending(r => r.Created), page, pagesize);
            return pagedReplies;
        }

        /// <summary>
        /// Generates a unique file name by appending a timestamp to the provided file name.
        /// </summary>
        /// <remarks>The method ensures that the returned file name is safe for use in file systems by
        /// sanitizing the original file name.</remarks>
        /// <param name="fileName">The original file name, which may include an extension. Only the file name portion is used.</param>
        /// <param name="timestamp">When this method returns, contains the UTC timestamp used in the generated file name, formatted as a string.</param>
        /// <returns>A string representing the unique file name, consisting of the timestamp followed by an underscore and a
        /// sanitized version of the original file name.</returns>
        private string GetUniqueFileName(string fileName, out string timestamp)
        {
            fileName = Path.GetFileName(fileName);
            timestamp = DateTime.UtcNow.ToForumDateStr();
            return  timestamp
                    + "_"
                    + GetSafeFilename(fileName);
        }

        /// <summary>
        /// Converts a filename into a safe format by replacing invalid characters with underscores.
        /// </summary>
        /// <param name="filename">The original filename to be sanitized. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A sanitized version of the filename with invalid characters replaced by underscores.</returns>
        private static string GetSafeFilename(string filename)
        {

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

        }

        /// <summary>
        /// Determines whether access is restricted based on the specified forum authorization type and user roles.
        /// </summary>
        /// <remarks>This method evaluates the user's access permissions based on the provided
        /// authorization type and roles. It also determines whether a password is required for access in certain cases.
        /// The caller should check the return value to determine if access is restricted and use the <paramref
        /// name="passwordrequired"/> parameter to handle password-protected scenarios.</remarks>
        /// <param name="auth">The type of forum authorization to evaluate.</param>
        /// <param name="signedin">A value indicating whether the user is signed in.</param>
        /// <param name="ismoderator">A value indicating whether the user has moderator privileges.</param>
        /// <param name="isadministrator">A value indicating whether the user has administrator privileges.</param>
        /// <param name="passwordrequired">A reference parameter that is set to <see langword="true"/> if a password is required for access; otherwise,
        /// <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if access is not allowed based on the specified authorization type and user roles;
        /// otherwise, <see langword="false"/>.</returns>
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

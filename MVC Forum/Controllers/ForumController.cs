using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Forum;
using MVCForum.ViewModels.Post;
using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using X.PagedList;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class ForumController : SnitzBaseController
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly ISnitzCookie _cookie;
        private readonly SignInManager<ForumUser> _signInManager;
        private readonly HttpContext? _httpcontext;
        private readonly IGroups _groupservice;

        public ForumController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IForum forumService,IPost postService,ISnitzCookie snitzCookie,SignInManager<ForumUser> SignInManager,IGroups groups) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _forumService = forumService;
            _postService = postService;
            _cookie = snitzCookie;
            _signInManager = SignInManager;
            _httpcontext = httpContextAccessor.HttpContext;
            _groupservice = groups;
        }
        
        [Route("Forum/{id:int}")]
        [Route("Forum/Index/{id:int}")]
        public IActionResult Index(int id,int? defaultdays, int page = 1, string orderby = "lpd",string sortdir="des", int pagesize = 0)
        {
            ViewBag.RequireAuth = false;
            if (_httpcontext!.Session.GetInt32("ForumPageSize") != null && pagesize == 0)
            {
                pagesize = _httpcontext!.Session.GetInt32("ForumPageSize")!.Value;
            }
            else if (pagesize == 0)
            {
                pagesize = 10;
            }
            _httpcontext.Session.SetInt32("ForumPageSize",pagesize);
            Forum? forum = null;
            try
            {
                forum = _forumService.GetWithPosts(id);
            }
            catch (Exception)
            {
                return NotFound();
            }
            

            bool signedin = false;
            if (User.Identity is { IsAuthenticated: true })
            {
                signedin = true;
            }
            bool passwordrequired = false;
            bool notallowed = false;
            bool ismoderator = User.IsInRole($"Forum_{forum.Id}");
            bool isadministrator = User.IsInRole("Administrator");

            notallowed = CheckAuthorisation(forum.Privateforums, signedin, ismoderator, isadministrator, ref passwordrequired);
            if (!isadministrator && passwordrequired)
            {
                var auth = _httpcontext.Session.GetString("Pforum_" + id) == null ? "" : _httpcontext.Session.GetString("Pforum_" + id);
                if (auth != forum.Password)
                {
                    ViewBag.RequireAuth = true;
                }
            }
            var forumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name) { Parent = forumPage,RouteValues = new {id=forum.Category?.Id}};
            var topicPage = new MvcBreadcrumbNode("Index", "Post", forum.Title) { Parent = catPage };
            ViewData["BreadcrumbNode"] = topicPage;

            ViewData["Title"] = forum.Title;
            bool showsticky = _config.GetIntValue("STRSTICKYTOPIC") == 1;
            
            List<PostListingModel>? stickies = null;
            IEnumerable<Post>? forumPosts = null;

            if (!showsticky)
            {
                forumPosts = forum?.Posts;
            }
            else
            {
                stickies = new PagedList<Post>(forum!.Posts!.Where(p=>p.IsSticky == 1).OrderByDescending(p=>p.LastPostDate), page, pagesize)
                .Select(p => new PostListingModel()
                {
                    Id = p.Id,
                    AuthorId = p.MemberId,
                    AuthorName = p.Member?.Name ?? "Unknown",
                    //AuthorRating = p.User?.Rating ?? 0,
                    Title = p.Title,
                    Created = p.Created.FromForumDateStr(),
                    RepliesCount = p.ReplyCount,
                    ViewCount = p.ViewCount,
                    UnmoderatedReplies = p.UnmoderatedReplies,
                    IsSticky = p.IsSticky == 1 && showsticky,
                    Status = p.Status,
                    Message = p.Content,
                    LastPostDate = !string.IsNullOrEmpty(p.LastPostDate) ? p.LastPostDate?.FromForumDateStr() : null,
                    LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                    LatestReply = p.LastPostReplyId,
                    Forum = BuildForumListing(p.ForumId,p.Forum!),
                    Answered = p.Answered,
                    HasPoll = _postService.HasPoll(p.Id),
                    AllowRating = p.AllowRating,
                    ForumAllowRating = p.Forum.Rating,
                    Rating = p.GetTopicRating()
                }).ToList();
                forumPosts = forum?.Posts?.Where(p => p.IsSticky != 1);
            }
            if (!(isadministrator || ismoderator))
            {
                var curuser = _memberService.Current()?.Id;
                forumPosts = forumPosts?.Where(t => t.Status < 2 || t.MemberId == curuser);
            }
            if (defaultdays != null)
            {
                HttpContext.Session.SetInt32($"Forum_{forum!.Id}", defaultdays.Value);
            }
            else
            {
                defaultdays = HttpContext.Session.GetInt32($"Forum_{forum!.Id}");
            }

            if (defaultdays != null && defaultdays.Value > 0)
            {
                forumPosts = forumPosts?.Where(f => f.LastPostDate?.FromForumDateStr() > DateTime.UtcNow.AddDays(defaultdays.Value * -1) ||
                                                    (f.LastPostDate == null && f.Created?.FromForumDateStr() > DateTime.UtcNow.AddDays(defaultdays.Value * -1)));
            }
            else if (defaultdays != null)
            {
                switch (defaultdays.Value)
                {
                    case -1 : //AllOpen 
                        forumPosts = forumPosts?.Where(f => f.Status == 1);
                        break;
                    //case -99 : //Archived
                    //    //TODO: Archived Topics                      
                    //    break;
                    case -999: //NoReplies
                        forumPosts = forumPosts?.Where(f => f is { Status: 1, ReplyCount: 0 });
                        break;
                    case -9999: //Draft
                        forumPosts = forumPosts?.Where(f => f.Status == 99 ); //TODO: current user check
                        break;
                    case -88: //Hot
                        if (_config.GetIntValue("STRHOTTOPIC") == 1)
                        {
                            var hottopicount = _config.GetIntValue("INTHOTTOPICNUM",25);
                            forumPosts = forumPosts?.Where(f => f.ReplyCount > hottopicount);
                        }
                        break;
                }
            }
            else
            {
                switch (forum.Defaultdays)
                {
                    case -1 : //AllOpen 
                        forumPosts = forumPosts?.Where(f => f.Status == 1);
                        break;
                    //case -99 : //Archived
                    //    //TODO: Archived Topics                      
                    //    break;
                    case -999: //NoReplies
                        forumPosts = forumPosts?.Where(f => f is { Status: 0, ReplyCount: 0 });
                        break;
                    case -9999: //Draft
                        forumPosts = forumPosts?.Where(f => f.Status == 99 ); //TODO: current user check
                        break;
                    case -88: //Hot
                        if (_config.GetIntValue("STRHOTTOPIC") == 1)
                        {
                            var hottopicount = _config.GetIntValue("INTHOTTOPICNUM",25);
                            forumPosts = forumPosts?.Where(f => f.ReplyCount > hottopicount);
                        }
                        break;
                    default:
                        if(forum.Defaultdays > 0)
                            forumPosts = forumPosts?.Where(f => f.LastPostDate?.FromForumDateStr() > DateTime.UtcNow.AddDays(forum.Defaultdays * -1) ||
                                                                (f.LastPostDate == null && f.Created?.FromForumDateStr() > DateTime.UtcNow.AddDays(forum.Defaultdays * -1)));
                        break;
                }                
            }

            if (forum != null)
            {
                forumPosts = orderby switch
                {
                    "a" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.Member?.Name)
                        : forumPosts?.OrderBy(p => p.Member?.Name),
                    "v" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.ViewCount)
                        : forumPosts?.OrderBy(p => p.ViewCount),
                    "r" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.ReplyCount)
                        : forumPosts?.OrderBy(p => p.ReplyCount),
                    "lpa" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.LastPostAuthor?.Name)
                        : forumPosts?.OrderBy(p => p.LastPostAuthor?.Name),
                    "pd" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.Created)
                        : forumPosts?.OrderBy(p => p.Created),
                    _ => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.LastPostDate)
                        : forumPosts?.OrderBy(p => p.LastPostDate)
                };

                PagedList<Post> pagedTopics = new PagedList<Post>(forumPosts!, page, pagesize);

                var postlistings = pagedTopics.Select(p => new PostListingModel()
                {
                    Id = p.Id,
                    AuthorId = p.MemberId,
                    AuthorName = p.Member?.Name ?? "Unknown",
                    //AuthorRating = p.User?.Rating ?? 0,
                    Title = p.Title,
                    Created = p.Created.FromForumDateStr(),
                    RepliesCount = p.ReplyCount,
                    ViewCount = p.ViewCount,
                    UnmoderatedReplies = p.UnmoderatedReplies,
                    IsSticky = p.IsSticky == 1 && showsticky,
                    Status = p.Status,
                    Message = p.Content,
                    LastPostDate = !string.IsNullOrEmpty(p.LastPostDate) ? p.LastPostDate?.FromForumDateStr() : null,
                    LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                    LatestReply = p.LastPostReplyId,
                    Forum = BuildForumListing(p),
                    Answered = p.Answered,
                    HasPoll = _postService.HasPoll(p.Id),
                    AllowRating = p.AllowRating,
                    ForumAllowRating = p.Forum.Rating,
                    Rating = p.GetTopicRating()
                });

                var model = new ForumTopicModel()
                {
                    AccessDenied = notallowed,
                    PasswordRequired = passwordrequired,
                    StickyPosts = stickies,
                    Posts = postlistings,
                    Forum = BuildForumListing(forum.Id,forum,defaultdays,orderby,sortdir),
                    PageCount = pagedTopics.PageCount,
                    PageNum = pagedTopics.PageNumber,
                    PageSize = pagesize,
                    
                };

                return View("Index",model);
            }
            return View("Index");
        }

        public IActionResult Archived(int id,int? defaultdays, int page = 1, string orderby = "lpd",string sortdir="des", int pagesize = 0)
        {
            Forum forum ;
            try
            {
                forum = _forumService.Get(id);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
            ViewBag.RequireAuth = false;
            if (_httpcontext!.Session.GetInt32("ForumPageSize") != null && pagesize == 0)
            {
                pagesize = _httpcontext!.Session.GetInt32("ForumPageSize")!.Value;
            }
            else if (pagesize == 0)
            {
                pagesize = 10;
            }
            _httpcontext.Session.SetInt32("ForumPageSize",pagesize);
            

            bool signedin = false;
            if (User.Identity is { IsAuthenticated: true })
            {
                signedin = true;
            }
            bool passwordrequired = false;
            bool notallowed = false;
            bool ismoderator = User.IsInRole($"Forum_{forum.Id}");
            bool isadministrator = User.IsInRole("Administrator");

            notallowed = CheckAuthorisation(forum.Privateforums, signedin, ismoderator, isadministrator, ref passwordrequired);
            if (!isadministrator && passwordrequired)
            {
                var auth = _httpcontext.Session.GetString("Pforum_" + id) == null ? "" : _httpcontext.Session.GetString("Pforum_" + id);
                if (auth != forum.Password)
                {
                    ViewBag.RequireAuth = true;
                }
            }
            var forumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name) { Parent = forumPage,RouteValues = new {id=forum.Category?.Id}};
            var topicPage = new MvcBreadcrumbNode("Index", "Post", forum.Title) { Parent = catPage };
            ViewData["BreadcrumbNode"] = topicPage;

            ViewData["Title"] = forum.Title;
            bool showsticky = _config.GetIntValue("STRSTICKYTOPIC") == 1;
            
            IEnumerable<ArchivedPost>? forumPosts = null;

            forumPosts = _forumService.ArchivedPosts(id);

            if (!(isadministrator || ismoderator))
            {
                var curuser = _memberService.Current()?.Id;
                forumPosts = forumPosts!.Where(t => t.Status < 2 || t.MemberId == curuser);
            }
            if (defaultdays != null)
            {
                HttpContext.Session.SetInt32($"Forum_{forum.Id}", defaultdays.Value);
            }
            else
            {
                defaultdays = HttpContext.Session.GetInt32($"Forum_{forum.Id}");
            }

            if (defaultdays != null && defaultdays.Value > 0)
            {
                forumPosts = forumPosts?.Where(f => f.LastPostDate?.FromForumDateStr() > DateTime.UtcNow.AddDays(defaultdays.Value * -1) ||
                                                    (f.LastPostDate == null && f.Created?.FromForumDateStr() > DateTime.UtcNow.AddDays(defaultdays.Value * -1)));
            }
            else if (defaultdays != null)
            {
                switch (defaultdays.Value)
                {
                    case -1 : //AllOpen 
                        forumPosts = forumPosts?.Where(f => f.Status == 1);
                        break;
                    //case -99 : //Archived
                    //    //TODO: Archived Topics                      
                    //    break;
                    case -999: //NoReplies
                        forumPosts = forumPosts?.Where(f => f is { Status: 1, ReplyCount: 0 });
                        break;
                    case -9999: //Draft
                        forumPosts = forumPosts?.Where(f => f.Status == 99 ); //TODO: current user check
                        break;
                    case -88: //Hot
                        if (_config.GetIntValue("STRHOTTOPIC") == 1)
                        {
                            var hottopicount = _config.GetIntValue("INTHOTTOPICNUM",25);
                            forumPosts = forumPosts?.Where(f => f.ReplyCount > hottopicount);
                        }
                        break;
                }
            }
            else
            {
                switch (forum.Defaultdays)
                {
                    case -1 : //AllOpen 
                        forumPosts = forumPosts?.Where(f => f.Status == 1);
                        break;
                    //case -99 : //Archived
                    //    //TODO: Archived Topics                      
                    //    break;
                    case -999: //NoReplies
                        forumPosts = forumPosts?.Where(f => f is { Status: 0, ReplyCount: 0 });
                        break;
                    case -9999: //Draft
                        forumPosts = forumPosts?.Where(f => f.Status == 99 ); //TODO: current user check
                        break;
                    case -88: //Hot
                        if (_config.GetIntValue("STRHOTTOPIC") == 1)
                        {
                            var hottopicount = _config.GetIntValue("INTHOTTOPICNUM",25);
                            forumPosts = forumPosts?.Where(f => f.ReplyCount > hottopicount);
                        }
                        break;
                    default:
                        if(forum.Defaultdays > 0)
                            forumPosts = forumPosts?.Where(f => f.LastPostDate?.FromForumDateStr() > DateTime.UtcNow.AddDays(forum.Defaultdays * -1) ||
                                                                (f.LastPostDate == null && f.Created?.FromForumDateStr() > DateTime.UtcNow.AddDays(forum.Defaultdays * -1)));
                        break;
                }                
            }

            if (forum != null)
            {
                forumPosts = orderby switch
                {
                    "a" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.Member?.Name)
                        : forumPosts?.OrderBy(p => p.Member?.Name),
                    "v" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.ViewCount)
                        : forumPosts?.OrderBy(p => p.ViewCount),
                    "r" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.ReplyCount)
                        : forumPosts?.OrderBy(p => p.ReplyCount),
                    "lpa" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.LastPostAuthor?.Name)
                        : forumPosts?.OrderBy(p => p.LastPostAuthor?.Name),
                    "pd" => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.Created)
                        : forumPosts?.OrderBy(p => p.Created),
                    _ => sortdir == "des"
                        ? forumPosts?.OrderByDescending(p => p.LastPostDate)
                        : forumPosts?.OrderBy(p => p.LastPostDate)
                };

                PagedList<ArchivedPost> pagedTopics = new PagedList<ArchivedPost>(forumPosts!, page, pagesize);

                var postlistings = pagedTopics.Select(p => new PostListingModel()
                {
                    Id = p.Id,
                    AuthorId = p.MemberId,
                    AuthorName = p.Member?.Name ?? "Unknown",
                    //AuthorRating = p.User?.Rating ?? 0,
                    Title = p.Subject,
                    Created = p.Created.FromForumDateStr(),
                    RepliesCount = p.ReplyCount,
                    ViewCount = p.ViewCount,
                    UnmoderatedReplies = 0,
                    IsSticky = false,
                    Status = 0,
                    Message = p.Message,
                    LastPostDate = !string.IsNullOrEmpty(p.LastPostDate) ? p.LastPostDate?.FromForumDateStr() : null,
                    LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                    LatestReply = p.LastPostReplyId,
                    Forum = BuildForumListing(p),
                    Answered = false,
                    HasPoll = _postService.HasPoll(p.Id),
                    AllowRating = p.AllowRating,
                    ForumAllowRating = p.Forum.Rating,

                });

                var model = new ForumTopicModel()
                {
                    AccessDenied = notallowed,
                    PasswordRequired = passwordrequired,
                    StickyPosts = new List<PostListingModel>(),
                    Posts = postlistings,
                    Forum = BuildForumListing(forum.Id,forum,defaultdays,orderby,sortdir),
                    PageCount = pagedTopics.PageCount,
                    PageNum = pagedTopics.PageNumber,
                    PageSize = pagesize,
                    Archives = true
                };

                return View("Index",model);
            }
            return this.NotFound();
        }



        [Route("Topic/Active")]
        public IActionResult Active(int page = 1, int pagesize = 0,ActiveRefresh? Refresh = null,ActiveSince? Since = null, int groupId=0)
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

            if (HttpContext.Session.GetInt32("ActivePageSize") != null && pagesize == 0)
            {
                pagesize = HttpContext!.Session.GetInt32("ActivePageSize")!.Value;
            }
            else if (pagesize == 0)
            {
                pagesize = 20;
            }
            HttpContext.Session.SetInt32("ActivePageSize",pagesize);

            var homePage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
            var topicPage = new MvcBreadcrumbNode("Active", "Forum", "Active") { Parent = homePage };
            ViewData["BreadcrumbNode"] = topicPage;
            if (Since == null)
            {
                var sincecookie = _cookie.GetTopicSince();
                if (sincecookie != null)
                {
                    Since = (ActiveSince)(Convert.ToInt32(sincecookie));
                }
            }
            else
            {
                _cookie.SetTopicSince(((int)Since).ToString());
            }
            if (Refresh != null)
            {
                _cookie.SetActiveRefresh(((int)Refresh).ToString());
            }
            var lastvisit = DateTime.UtcNow.AddDays(-7);
            var member = _memberService.GetByUsername(User.Identity?.Name!);
            if (member != null)
            {
                lastvisit = member.LastLogin.FromForumDateStr();
            }

            if (Since.HasValue && Since != ActiveSince.LastVisit)
            {
                lastvisit = Since switch
                {
                    ActiveSince.Last12Hours => DateTime.UtcNow.AddHours(-12),
                    ActiveSince.Last2Hours => DateTime.UtcNow.AddHours(-2),
                    ActiveSince.LastHour => DateTime.UtcNow.AddHours(-1),
                    ActiveSince.Last6Hours => DateTime.UtcNow.AddHours(-6),
                    ActiveSince.LastFifteen => DateTime.UtcNow.AddMinutes(-15),
                    ActiveSince.LastThirty => DateTime.UtcNow.AddMinutes(-30),
                    ActiveSince.LastDay => DateTime.UtcNow.AddDays(-1),
                    ActiveSince.Last2Days => DateTime.UtcNow.AddDays(-2),
                    ActiveSince.LastWeek => DateTime.UtcNow.AddDays(-7),
                    ActiveSince.Last2Weeks => DateTime.UtcNow.AddDays(-14),
                    ActiveSince.LastMonth => DateTime.UtcNow.AddMonths(-1),
                    ActiveSince.Last2Months => DateTime.UtcNow.AddMonths(-2),
                    _ => lastvisit
                };
                
            }
            //TODO: filter by group ??

            var posts = _postService.GetAllTopicsAndRelated()
                .Where(f => f.LastPostDate?.FromForumDateStr() > lastvisit)
                .OrderByDescending(t=>t.LastPostDate).AsEnumerable();
            if(groupId > 1)
            {
                var catfilter = _groupservice.GetGroups(groupId).Select(g=>g.CategoryId).ToList();
                posts = posts
                .Where(f =>  catfilter.Contains(f.CategoryId))
                .OrderByDescending(t=>t.LastPostDate).AsEnumerable();
            }
            if (!User.IsInRole("Administrator")) //TODO: Is the member a moderator?
            {
                posts = posts.Where(p => p.Status < 2 || p.MemberId == member?.Id);
            }
            PagedList<Post> latestPosts = new(posts, page, pagesize);
                
            var activeposts = latestPosts.Select(p => new PostListingModel()
            {
                Id = p.Id,
                AuthorId = p.MemberId,
                AuthorName = p.Member?.Name ?? "Unknown",
                //AuthorRating = p.User?.Rating ?? 0,
                Title = p.Title,
                Created = p.Created.FromForumDateStr(),
                RepliesCount = p.ReplyCount,
                ViewCount = p.ViewCount,
                UnmoderatedReplies = p.UnmoderatedReplies,
                IsSticky = p.IsSticky == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !string.IsNullOrEmpty(p.LastPostDate) ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = p.LastPostAuthorId != null ? p.LastPostAuthor!.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered,
                AllowRating = p.AllowRating,
                ForumAllowRating = p.Forum.Rating,
                Rating = p.GetTopicRating()
                //HasPoll = _postService.HasPoll(p.Id),
            });
            if (Refresh == null)
            {
                var refreshcookie = _cookie.GetActiveRefresh();
                if (refreshcookie != null)
                {
                    Refresh = (ActiveRefresh)(Convert.ToInt32(refreshcookie));
                } 
            }


            var model = new ActiveTopicModel()
            {
                Posts = activeposts,
                PageCount = latestPosts.PageCount,
                PageNum = latestPosts.PageNumber,
                Refresh = Refresh ?? ActiveRefresh.None,
                Since = member == null || Since == null ? ActiveSince.LastVisit : Since.Value,
                PageSize = pagesize
            };

            ViewBag.RefreshSeconds = (int)model.Refresh * 1000 * 60;
            return View(model);
        }
        
        [Breadcrumb(FromAction = "Index", FromController = typeof(CategoryController),Title = "Create Forum")]
        [Authorize(Roles="Administrator")]
        [HttpGet]
        public IActionResult Create(int id)
        {
             var catlist = _forumService.CategoryList();
            if (id > 0)
            {
                var category = catlist.OrderBy(f=>f.Key).FirstOrDefault(f=>f.Key == id);
                var forumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
                var catPage = new MvcBreadcrumbNode("", "Category", category.Value) { Parent = forumPage,RouteValues = new {id=id}};
                ViewData["BreadcrumbNode"] = catPage;
            }
            else
            {
                var forumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
                var catPage = new MvcBreadcrumbNode("", "Category", _languageResource.GetString("tipNewForum")) { Parent = forumPage};
                ViewData["BreadcrumbNode"] = catPage;
            }
                


            var model = new NewForumModel { CategoryList = catlist,Category = id,ForumId = 0};
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles="Administrator")]
        public async Task<IActionResult> Create(NewForumModel model)
        {
            ModelState.Remove("CategoryList");
            if (ModelState.IsValid)
            {
                try
                {
                    Forum newForum = new()
                    {
                        Id = model.ForumId,
                        Title = model.Subject, 
                        Description = model.Description, 
                        CategoryId = model.Category,
                        Type = (short)model.Type,
                        Privateforums = model.AuthType,
                        Status = (short)(model.Status),
                        Order = model.Order,
                        Defaultdays = (int)model.DefaultView,
                        CountMemberPosts = (short)(model.IncrementMemberPosts ? 1 : 0),
                        Moderation = model.Moderation,
                        Subscription = (int)model.Subscription,
                        Password = model.NewPassword,
                        Rating = (short) model.AllowTopicRating
                    };
                    if (model.ForumId != 0)
                    {
                        newForum.Id = model.Id;
                        await _forumService.Update(newForum);
                        return RedirectToAction("Index","Category",new{id = model.Category});
                    }
                    else
                    {
                        await _forumService.Create(newForum);
                        return RedirectToAction("Index","Category",new{id = model.Category});
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    ModelState.AddModelError("Save Forum Error",e.Message);
                }



            }
            else
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
            }
            return View(model);

            
        }

        [Authorize(Roles="Administrator")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var forum = _forumService.GetWithPosts(id);
            var catlist = _forumService.CategoryList();
            var category = catlist.OrderBy(f=>f.Key).FirstOrDefault(f=>f.Key == forum.CategoryId);
            var allforumPage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", category.Value) { Parent = allforumPage,RouteValues = new {id=forum.CategoryId}};
            var forumPage = new MvcBreadcrumbNode("Index", "Post", forum.Title) { Parent = catPage };

            ViewData["BreadcrumbNode"] = forumPage;

            var model = new NewForumModel
            {
                CategoryList = catlist!,
                Category = forum.CategoryId,
                Id = id,
                DefaultView = (DefaultDays)forum.Defaultdays,
                Description = forum.Description,
                IncrementMemberPosts = forum.CountMemberPosts == 1,
                AuthType = (ForumAuthType)forum.Privateforums,
                Order = forum.Order,
                Subject = forum.Title,
                Type = (ForumType)forum.Type,
                Status = forum.Status,
                Moderation = forum.Moderation,
                Subscription = (ForumSubscription)forum.Subscription,
                ForumId = id,
                NewPassword = forum.Password,
                AllowedMembers = _forumService.AllowedUsers(id),
                AllowTopicRating = forum.Rating
            };
            return View("Create",model);
        }

        [Breadcrumb(FromAction = "Index",FromController = typeof(CategoryController), Title = "Delete Forum")]
        [Authorize(Roles="Administrator")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var catid = _forumService.GetWithPosts(id).CategoryId;
            await _forumService.Delete(id);
            return Json(new { redirectToUrl = Url.Action("Index", "Category",new{id = catid}) });
        }

        [HttpGet]
        [Authorize]
        [Route("Forum/Subscribe/")]
        public IActionResult Subscribe(int id)
        {
            var forum = _forumService.GetWithPosts(id);
            var member = _memberService.Current();
            _snitzDbContext.MemberSubscription.Add(new MemberSubscription()
            {
                MemberId = member!.Id,
                CategoryId = forum.CategoryId,
                ForumId = forum.Id,
                PostId = 0
            });
            _snitzDbContext.SaveChanges();
            return Content("OK");
        }

        [HttpGet]
        [Authorize]
        [Route("Forum/UnSubscribe/")]
        public IActionResult UnSubscribe(int id)
        {
            var member = _memberService.Current();
            _snitzDbContext.MemberSubscription.Where(s => s.MemberId == member!.Id && s.ForumId == id && s.PostId == 0)
                .ExecuteDelete();
            return Content("OK");
        }

        public IActionResult Search(string? searchFor, int pagesize=10,int page=1,int catid = 0,int forumid=0)
        {
            var homePage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
            var topicPage = new MvcBreadcrumbNode("Search", "Forum", "ViewData.Title") { Parent = homePage };
            ViewData["BreadcrumbNode"] = topicPage;
            ViewData["Title"] = "Search";
            if (HttpContext.Request.Cookies.ContainsKey("search-pagesize") && _config.GetValueWithDefault("STRFORUMPAGESIZES", _config.DefaultPageSize.ToString())!.Split(',').Count() > 1)
            {
                var pagesizeCookie = HttpContext.Request.Cookies["search-pagesize"];
                if (pagesizeCookie != null)
                    pagesize = Convert.ToInt32(pagesizeCookie);
            }
            if (searchFor != null)
            {
                HttpContext.Session.SetString("searchFor",searchFor);
                ViewData["Title"] = "SearchFor : " + searchFor;
            }
            var posts = _postService.GetFilteredPost(searchFor!,out int totalcount,pagesize,page,catid,forumid).Select(p => new PostListingModel()
            {
                Id = p.Id,
                AuthorId = p.MemberId,
                AuthorName = p.Member?.Name ?? "Unknown",
                //AuthorRating = p.User?.Rating ?? 0,
                Title = p.Title,
                Created = p.Created.FromForumDateStr(),
                RepliesCount = p.ReplyCount,
                ViewCount = p.ViewCount,
                UnmoderatedReplies = p.UnmoderatedReplies,
                IsSticky = p.IsSticky == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !string.IsNullOrEmpty(p.LastPostDate) ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered,
                HasPoll = _postService.HasPoll(p.Id),
                AllowRating = p.AllowRating,
                ForumAllowRating = p.Forum.Rating,
                Rating = p.GetTopicRating()
            });

            var pageCount = (int)Math.Ceiling((double)totalcount / pagesize);
            var topicModel = new ForumTopicModel()
            {
                Posts = posts,
                PageCount = pageCount,
                PageNum = page
            };
            var model = new ForumSearchModel()
            {
                Results = topicModel,
                SearchFor = SearchFor.ExactPhrase,
                SearchArchives = false,
                SearchMessage = false,
                Categories = new SelectList(_forumService.CategoryList(), "Key", "Value"),
                Forums = new SelectList(_forumService.ForumList(false), "Key", "Value"),
            };
            return View("../Search/Index",model);
        }

        //[HttpPost]
        public IActionResult SearchResult(ForumSearchModel model,int pagesize=10,int page=1)
        {
            var homePage = new MvcBreadcrumbNode("", "AllForums", "ttlForums");
            var searchPage = new MvcBreadcrumbNode("Search", "Forum", "Search") { Parent = homePage };
            var topicPage = new MvcBreadcrumbNode("Search", "Forum", "ViewData.Title") { Parent = searchPage };
            ViewData["BreadcrumbNode"] = topicPage;
            ViewData["Title"] = "Search Results";
            var searchmodel = new ForumSearch()
            {
                SinceDate = (SearchDate)model.SinceDate,
                SearchArchives = model.SearchArchives,
                SearchMessage = model.SearchMessage,
                SearchCategory = model.SearchCategory,
                SearchFor = model.SearchFor,
                SearchForums = model.SearchForums,
                UserName = model.UserName,
                Terms = model.Terms,

            };
            if(searchmodel.UserName != null)
            {
                ViewData["Title"] = $"{searchmodel.UserName}'s Posts";
                if (searchmodel.SearchMessage)
                {
                    ViewData["Title"] = $"{searchmodel.UserName}'s Topics";
                }
                if (searchmodel.SinceDate == SearchDate.Since14Days)
                {
                    ViewData["Title"] = $"{searchmodel.UserName}'s Recent Posts";
                }
            }
            var posts = _postService.Find(searchmodel,out int totalcount,pagesize,page).Select(p => new PostListingModel()
            {
                Id = p.Id,
                AuthorId = p.MemberId,
                AuthorName = p.Member?.Name ?? "Unknown",
                //AuthorRating = p.User?.Rating ?? 0,
                Title = p.Title,
                Created = p.Created.FromForumDateStr(),
                RepliesCount = p.ReplyCount,
                ViewCount = p.ViewCount,

                IsSticky = p.IsSticky == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !string.IsNullOrEmpty(p.LastPostDate) ? p.LastPostDate?.FromForumDateStr() : p.Created.FromForumDateStr(),
                LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered,
                HasPoll = _postService.HasPoll(p.Id),
                AllowRating = p.AllowRating,
                ForumAllowRating = p.Forum.Rating,
                Rating = p.GetTopicRating()
            }).OrderByDescending(p=>p.LastPostDate);

            var pageCount = (int)Math.Ceiling((double)totalcount / pagesize);
            var topicModel = new ForumTopicModel()
            {
                Posts = posts,
                PageCount = pageCount,
                PageNum = page
            };
            model.Results = topicModel;
            return View("../Search/Index",model);
        }
        public IActionResult MyView(int pagenum=1, MyTopicsSince activesince = MyTopicsSince.Last12Months)
        {
            var forumsubs = _memberService.ForumSubscriptions().ToList();
            var result = _forumService.FetchMyForumTopicsPaged(5, pagenum, forumsubs);

            MyTopicsViewModel vm = new MyTopicsViewModel
            {
                Topics = result,
                AllTopics = _forumService.FetchAllMyForumTopics(forumsubs),
                ActiveSince = activesince
            };

            return View(vm);
        }
        [HttpGet]
        public IActionResult MyViewNext(int nextpage, string refresh = "NO" )
        {
            var forumsubs = _memberService.ForumSubscriptions().ToList();
            var result = _forumService.FetchMyForumTopicsPaged(5, nextpage, forumsubs);
            MyTopicsViewModel vm = new MyTopicsViewModel
            {
                Topics = result,
            };
            return ViewComponent("MyView", new { template = "Posts", model = vm });

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

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EmptyForum(int id )
        {
            await _forumService.EmptyForum(id);
            var forum = _forumService.GetWithPosts(id);
            return Json(new { redirectToUrl = Url.Action("Index", "Category",new{id=forum.CategoryId}) });

        }

        public IActionResult RemoveAllowed(IFormCollection form)
        {
            try
            {
                var member = Convert.ToInt32(form["AllowedMembers"]);
                var forum = Convert.ToInt32(form["ForumId"]);
                var allowedMember = _snitzDbContext.ForumAllowedMembers.SingleOrDefault(a=>a.MemberId == member && a.ForumId == forum);
                if (allowedMember != null)
                {
                    _snitzDbContext.Remove(allowedMember);
                    _snitzDbContext.SaveChanges();
                    return Json(new{id = form["AllowedMembers"]});
                }
                return Json(new{error="Record not found"});
            }
            catch (Exception e)
            {
                return Json(new{error = e.Message});
            }

        }
        public IActionResult AddAllowed(IFormCollection form)
        {
            try
            {
                var member = _memberService.GetByUsername(form["NewMember"]!);
                if (member != null)
                {
                    var allowed = new ForumAllowedMember()
                    {
                        MemberId = member.Id,
                        ForumId = Convert.ToInt32(form["ForumId"])
                    };
                    try
                    {
                        _snitzDbContext.ForumAllowedMembers.Add(allowed);
                        _snitzDbContext.SaveChanges();
                        return Json(new{id=member.Id,name=member.Name});
                    }
                    catch (Exception e)
                    {
                        return Json(new{error=e.Message});
                    }
                }
                return Json(new{error="Member not found"});
            }
            catch (Exception e)
            {
                return Json(new{error=e.Message});
            }


            
        }

        private ForumListingModel BuildForumListing(Post post)
        {
            var forum = post.Forum!;

            return BuildForumListing(post.ForumId,forum);
        }

        //[OutputCache(Duration = 30,VaryByQueryKeys = new []{"forumid"})]
        private ForumListingModel BuildForumListing(int forumid, Forum forum, int? defaultdays = null, string orderby = "lpd", string sortdir = "des")
        {
            return new ForumListingModel()
            {
                Id = forum.Id,
                Title = forum.Title,
                Description = forum.Description,
                AccessType = forum.Privateforums,
                ForumType = (ForumType)forum.Type,
                Url = forum.Url,
                OrderBy = orderby,
                SortDir = sortdir,
                DefaultView = (defaultdays == null) ? (DefaultDays)forum.Defaultdays : (DefaultDays)defaultdays.Value,
                CategoryId = forum.CategoryId,
                Status = forum.Status,
                Topics = forum.TopicCount,
                Posts = forum.ReplyCount,
                ForumModeration = forum.Moderation,
                ForumSubscription = (ForumSubscription)forum.Subscription,
                Polls = forum.Polls,
                ArchivedCount = forum.ArchivedTopics,
                AllowTopicRating = forum.Rating == 1
                //ImageUrl = forum.ImageUrl

            };
        }
        private ForumListingModel BuildForumListing(int? defaultdays, string orderby, string sortdir)
        {
            return new ForumListingModel()
            {
                OrderBy = orderby,
                SortDir = sortdir,
                DefaultView = (defaultdays == null || defaultdays<0) ? DefaultDays.Last30Days : (DefaultDays)defaultdays.Value,
            };
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
                ArchivedCount = forum.ArchivedTopics,
                AllowTopicRating = forum.Rating == 1
                //ImageUrl = forum.ImageUrl
            };
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

        private ForumListingModel BuildForumListing(ArchivedPost p)
        {
            var forum = p.Forum!;

            return BuildForumListing(p.ForumId,forum);
        }
    }
}
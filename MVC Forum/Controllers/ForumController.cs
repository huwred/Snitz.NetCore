using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using SmartBreadcrumbs.Attributes;
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
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels.Forum;
using MVCForum.ViewModels.Post;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Extensions;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class ForumController : SnitzController
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly ISnitzCookie _cookie;
        private readonly SignInManager<ForumUser> _signInManager;

        public ForumController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IForum forumService,IPost postService,ISnitzCookie snitzCookie,SignInManager<ForumUser> SignInManager) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _forumService = forumService;
            _postService = postService;
            _cookie = snitzCookie;
            _signInManager = SignInManager;
            
        }
        
        //[Breadcrumb("Forums",FromAction = "Index",FromController = typeof(CategoryController))]
        [Route("Forum/{id:int}")]
        [Route("Forum/Index/{id:int}")]
        public IActionResult Index(int id,int? defaultdays, int page = 1, string orderby = "lpd",string sortdir="des", int pagesize = 10)
        {
            bool passwordrequired = false;
            bool notallowed = false;
            if (User.Identity is { IsAuthenticated: true })
            {
                _memberService.SetLastHere(User);
            }
            var forum = _forumService.GetById(id);
            switch (forum.Privateforums)
            {
                case ForumAuthType.AllowedMembers:
                    if (_signInManager.IsSignedIn(User) && (User.IsInRole("Forum_" + forum.Id) || User.IsInRole("Administrator")))
                    {
                        break;
                    }
                    notallowed = true;
                    break;
                case ForumAuthType.PasswordProtected:
                    passwordrequired = true;
                    break;
                case ForumAuthType.AllowedMemberPassword:
                    if (_signInManager.IsSignedIn(User) && (User.IsInRole("Forum_" + forum.Id) || User.IsInRole("Administrator")))
                    {
                        passwordrequired = true;
                        break;
                    }
                    notallowed = true;
                    break;
                case ForumAuthType.Members:
                    if (_signInManager.IsSignedIn(User))
                        break;
                    notallowed = true;
                    break;
                case ForumAuthType.MembersHidden:
                    if (_signInManager.IsSignedIn(User))
                        break;
                    notallowed = true;
                    break;
                case ForumAuthType.AllowedMembersHidden:
                    if (_signInManager.IsSignedIn(User) && (User.IsInRole("Forum_" + forum.Id) || User.IsInRole("Administrator")))
                    {
                        break;
                    }
                    notallowed = true;
                    break;
                case ForumAuthType.MembersPassword:
                    if (_signInManager.IsSignedIn(User))
                    {
                        passwordrequired = true;
                        break;
                    }
                    notallowed = true;
                    break;
                default:
                    break;

            }
            var forumPage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name) { Parent = forumPage,RouteValues = new {id=forum.Category?.Id}};
            var topicPage = new MvcBreadcrumbNode("Index", "Post", forum.Title) { Parent = catPage };
            ViewData["BreadcrumbNode"] = topicPage;

            ViewData["Title"] = forum.Title;
            PagedList<Post> stickyTopics = new PagedList<Post>(forum?.Posts?.Where(p=>p.IsSticky == 1).OrderByDescending(p=>p.LastPostDate), page, pagesize);
            var stickylistings = stickyTopics.Select(p => new PostListingModel()
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
                IsSticky = p.IsSticky == 1 && _config.GetIntValue("STRSTICKYTOPIC") == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered
            });
            
            IEnumerable<Post>? forumPosts = forum?.Posts?.Where(p => p.IsSticky != 1);

            if (_config.GetIntValue("STRSTICKYTOPIC") != 1)
            {
                stickylistings = null;
                forumPosts = forum?.Posts;
            }
            if (!(User.IsInRole("Administrator") || User.IsInRole("Forum_" + forum.Id)))
            {
                var curuser = _memberService.Current()?.Id;
                forumPosts = forumPosts.Where(t => t.Status < 2 || t.MemberId == curuser);
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
                    //    //TODO: Arcvied Topics                      
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
                    //    //TODO: Arcvied Topics                      
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

            PagedList<Post> pagedTopics = new PagedList<Post>(forumPosts, page, pagesize);

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
                IsSticky = p.IsSticky == 1 && _config.GetIntValue("STRSTICKYTOPIC") == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered
            });

            if (forum != null)
            {
                var model = new ForumTopicModel()
                {
                    AccessDenied = notallowed,
                    PasswordRequired = passwordrequired,
                    StickyPosts = stickylistings,
                    Posts = postlistings,
                    Forum = BuildForumListing(forum,defaultdays,orderby,sortdir),
                    PageCount = pagedTopics.PageCount,
                    PageNum = pagedTopics.PageNumber,
                    PageSize = pagesize,
                    
                };

                return View("Index",model);
            }
            return View("Index");
        }

        public IActionResult Active(int page = 1, int pagesize = 20,ActiveRefresh? Refresh = null,ActiveSince? Since = null)
        {
            if (User.Identity is { IsAuthenticated: true })
            {
                _memberService.SetLastHere(User);
            }
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
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
                lastvisit = member.Lastheredate.FromForumDateStr();
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
            var posts = _postService.GetAllTopicsAndRelated()
                .Where(f => f.IsSticky != 1 && f.LastPostDate?.FromForumDateStr() > lastvisit)
                .OrderByDescending(t=>t.LastPostDate).AsEnumerable();
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
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered
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
        public IActionResult Create(int id)
        {
            var model = new NewForumModel { CategoryList = _forumService.CategoryList()!,Category = id,ForumId = 0};
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
                        Status = (short)(model.Status ? 1 : 0),
                        Order = model.Order,
                        Defaultdays = (int)model.DefaultView,
                        CountMemberPosts = (short)(model.IncrementMemberPosts ? 1 : 0),
                        Moderation = model.Moderation,
                        Subscription = (int)model.Subscription
                    };
                    if (model.ForumId != 0)
                    {
                        newForum.Id = model.Id;
                        await _forumService.Update(newForum);
                    }
                    else
                    {
                        await _forumService.Create(newForum);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                
                
            }
            return RedirectToAction("Index","Category",new{id = model.Category});
        }

        [Breadcrumb(FromAction = "Index",FromController = typeof(CategoryController), Title = "Edit Forum")]
        [Authorize(Roles="Administrator")]
        public IActionResult Edit(int id)
        {
            var forum = _forumService.GetById(id);
            var model = new NewForumModel
            {
                CategoryList = _forumService.CategoryList()!,
                Category = forum.CategoryId,
                Id = id,
                DefaultView = (DefaultDays)forum.Defaultdays,
                Description = forum.Description,
                IncrementMemberPosts = forum.CountMemberPosts == 1,
                AuthType = (ForumAuthType)forum.Type,
                Order = forum.Order,
                Subject = forum.Title,
                Type = (ForumType)forum.Type,
                Status = forum.Status == 1,
                Moderation = forum.Moderation,
                Subscription = (ForumSubscription)forum.Subscription,
                ForumId = id
            };
            return View("Create",model);
        }

        [Breadcrumb(FromAction = "Index",FromController = typeof(CategoryController), Title = "Delete Forum")]
        [Authorize(Roles="Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            string referer = Request.Headers["Referer"].ToString();
            var catid = _forumService.GetById(id).CategoryId;
            await _forumService.Delete(id);
            return Json(new { redirectToUrl = referer ?? Url.Action("Index", "Category",new{id = catid}) });
        }

        public IActionResult Search(string? searchFor, int pagesize=10,int page=1)
        {
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var topicPage = new MvcBreadcrumbNode("Search", "Forum", "ViewData.Title") { Parent = homePage };
            ViewData["BreadcrumbNode"] = topicPage;
            ViewData["Title"] = "Search";
            if (HttpContext.Request.Cookies.ContainsKey("search-pagesize") && _config.GetValue("STRFORUMPAGESIZES", _config.DefaultPageSize.ToString())!.Split(',').Count() > 1)
            {
                var pagesizeCookie = HttpContext.Request.Cookies["search-pagesize"];
                if (pagesizeCookie != null)
                    pagesize = Convert.ToInt32(pagesizeCookie);
            }
            if (searchFor == null)
            {
                var prev = HttpContext.Session.GetString("searchFor");
                if (prev != null)
                {
                    searchFor = prev;
                    ViewData["Title"] = "SearchFor : " + searchFor;
                }
            }
            else
            {
                HttpContext.Session.SetString("searchFor",searchFor);
                ViewData["Title"] = "SearchFor : " + searchFor;
            }
            var posts = _postService.GetFilteredPost(searchFor!,out int totalcount,pagesize,page).Select(p => new PostListingModel()
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
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered
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
                Forums = new SelectList(_forumService.ForumList(), "Key", "Value"),
            };
            return View("../Search/Index",model);
        }

        [HttpPost]
        public IActionResult SearchResult(ForumSearchModel model,int pagesize=10,int page=1)
        {
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var searchPage = new MvcBreadcrumbNode("Search", "Forum", "Search") { Parent = homePage };
            var topicPage = new MvcBreadcrumbNode("Search", "Forum", "ViewData.Title") { Parent = searchPage };
            ViewData["BreadcrumbNode"] = topicPage;
            ViewData["Title"] = "Search Results";
            var searchmodel = new ForumSearch()
            {
                SinceDate = model.SinceDate,
                SearchArchives = model.SearchArchives,
                SearchMessage = model.SearchMessage,
                SearchCategory = model.SearchCategory,
                SearchFor = model.SearchFor,
                SearchForums = model.SearchForums,
                UserName = model.UserName,
                Terms = model.Terms,

            };
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
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : p.Created.FromForumDateStr(),
                LastPostAuthorName = p.LastPostAuthorId != null ? _memberService.GetById(p.LastPostAuthorId!.Value)?.Name : "",
                LatestReply = p.LastPostReplyId,
                Forum = BuildForumListing(p),
                Answered = p.Answered
            }).OrderBy(p=>p.LastPostDate);

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

        private ForumListingModel BuildForumListing(Post post)
        {
            var forum = post.Forum!;

            return BuildForumListing(forum);
        }
        private ForumListingModel BuildForumListing(Forum forum, int? defaultdays = null, string orderby = "lpd", string sortdir = "des")
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
                ForumSubscription = (ForumSubscription)forum.Subscription
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
                ForumModeration = forum.Moderation
                //ImageUrl = forum.ImageUrl
            };
        }

        public async Task<IActionResult> EmptyForum(int id )
        {
            await _forumService.EmptyForum(id);
            var forum = _forumService.GetById(id);
            return Json(new { redirectToUrl = Url.Action("Index", "Category",new{id=forum.CategoryId}) });

        }

    }
}
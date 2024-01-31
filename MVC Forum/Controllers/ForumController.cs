using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using MVCForum.Models.Forum;
using MVCForum.Models.Post;
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
using MVCForum.Extensions;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class ForumController : SnitzController
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly ISnitzCookie _cookie;

        public ForumController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,
            IForum forumService,IPost postService,ISnitzCookie snitzCookie) : base(memberService, config, localizerFactory)
        {
            _forumService = forumService;
            _postService = postService;
            _cookie = snitzCookie;
        }
        
        //[Breadcrumb("Forums",FromAction = "Index",FromController = typeof(CategoryController))]
        [Route("Forum/{id:int}")]
        [Route("Forum/Index/{id:int}")]
        public IActionResult Index(int id, int page = 1, int defaultdays=30,string orderby = "lpd",string sortdir="des", int pagesize = 10)
        {
            var forum = _forumService.GetById(id);
            var forumPage = new MvcBreadcrumbNode("", "Category", "Forums");
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
                IsSticky = p.IsSticky == 1 && _config.GetIntValue("STRSTICKYTOPIC") == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = _memberService.GetById(p.LastPostAuthorId!.Value)?.Name,
                Forum = BuildForumListing(p)
            });
            
            IEnumerable<Post>? forumPosts = forum?.Posts?.Where(p => p.IsSticky != 1);
            if (_config.GetIntValue("STRSTICKYTOPIC") != 1)
            {
                stickylistings = null;
                forumPosts = forum?.Posts;
            }
            
            if (defaultdays > 0)
            {
                forumPosts = forumPosts?.Where(f => f.LastPostDate?.FromForumDateStr() > DateTime.UtcNow.AddDays(defaultdays * -1) ||
                                                    (f.LastPostDate == null && f.Created?.FromForumDateStr() > DateTime.UtcNow.AddDays(defaultdays * -1)));
            }
            else
            {
                switch (defaultdays)
                {
                    case -1 : //AllOpen 
                        forumPosts = forumPosts?.Where(f => f.Status == 0);
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
                IsSticky = p.IsSticky == 1 && _config.GetIntValue("STRSTICKYTOPIC") == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = _memberService.GetById(p.LastPostAuthorId!.Value)?.Name,
                Forum = BuildForumListing(p)
            });

            if (forum != null)
            {
                var model = new ForumTopicModel()
                {
                    StickyPosts = stickylistings,
                    Posts = postlistings,
                    Forum = BuildForumListing(forum,defaultdays,orderby,sortdir),
                    PageCount = pagedTopics.PageCount,
                    PageNum = pagedTopics.PageNumber
                };

                return View("Index",model);
            }
            return View("Index");
        }

        public IActionResult Active(int page = 1, int pagesize = 20,ActiveRefresh? Refresh = null,ActiveSince? Since = null)
        {
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
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
            var posts = _postService.GetAllTopicsAndRelated().Where(f => f.IsSticky != 1 && f.LastPostDate?.FromForumDateStr() > lastvisit).OrderByDescending(t=>t.LastPostDate);

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
                IsSticky = p.IsSticky == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = _memberService.GetById(p.LastPostAuthorId!.Value)?.Name,
                Forum = BuildForumListing(p)
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
                Since = member == null || Since == null ? ActiveSince.LastVisit : Since.Value
            };

            ViewBag.RefreshSeconds = (int)model.Refresh * 1000 * 60;
            return View(model);
        }

        
        [Breadcrumb(FromAction = "Index", FromController = typeof(CategoryController),Title = "Create Forum")]
        [Authorize(Roles="Admin")]
        public IActionResult Create(int id)
        {
            var model = new NewForumModel { CategoryList = _forumService.CategoryList()!,Category = id};
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles="Admin")]
        public IActionResult Create(NewForumModel model)
        {
            ModelState.Remove("CategoryList");
            if (ModelState.IsValid)
            {
                Forum newForum = new()
                {
                    Title = model.Subject, 
                    Description = model.Description, 
                    CategoryId = model.Category,
                    Type = (short)model.Type,
                    Privateforums = model.AuthType,
                    Order = model.Order,
                    Defaultdays = (int)model.DefaultView,
                    CountMemberPosts = (short)(model.IncrementMemberPosts ? 1 : 0)
                };
                if (model.Id != 0)
                {
                    newForum.Id = model.Id;
                    _forumService.Update(newForum);
                }
                else
                {
                    _forumService.Create(newForum);
                }
                
                
            }
            return RedirectToAction("Index","Category",new{id = model.Category});
        }

        [Breadcrumb(FromAction = "Index",FromController = typeof(CategoryController), Title = "Edit Forum")]
        [Authorize(Roles="Admin")]
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
                Type = (ForumType)forum.Type
            };
            return View("Create",model);
        }

        [Breadcrumb(FromAction = "Index",FromController = typeof(CategoryController), Title = "Delete Forum")]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var catid = _forumService.GetById(id).CategoryId;
            await _forumService.Delete(id);
            return Json(new { redirectToUrl = Url.Action("Index", "Category",new{id = catid}) });
            //return RedirectToAction("Index", "Category",new{id = forum.CategoryId});
        }

        public IActionResult Search(string? searchFor, int pagesize=10,int page=1)
        {
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var topicPage = new MvcBreadcrumbNode("Search", "Forum", "ViewData.Title") { Parent = homePage };
            ViewData["BreadcrumbNode"] = topicPage;
            ViewData["Title"] = "Search";

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
                IsSticky = p.IsSticky == 1,
                Status = p.Status,
                Message = p.Content,
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = _memberService.GetById(p.LastPostAuthorId!.Value)?.Name,
                Forum = BuildForumListing(p)
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
            var homePage = new MvcBreadcrumbNode("", "Category", "Forums");
            var topicPage = new MvcBreadcrumbNode("Search", "Forum", "ViewData.Title") { Parent = homePage };
            ViewData["BreadcrumbNode"] = topicPage;
            ViewData["Title"] = "Search";
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
                LastPostDate = !p.LastPostDate.IsNullOrEmpty() ? p.LastPostDate?.FromForumDateStr() : null,
                LastPostAuthorName = _memberService.GetById(p.LastPostAuthorId!.Value)?.Name,
                Forum = BuildForumListing(p)
            });

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
                CategoryId = forum.CategoryId
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
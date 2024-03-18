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
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Resources;
using MVCForum.ViewModels;
using Microsoft.Extensions.Hosting;
using System.Drawing.Printing;
using Microsoft.EntityFrameworkCore;

namespace MVCForum.Controllers
{
    [CustomAuthorize]
    public class TopicController : SnitzController
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        private readonly UserManager<ForumUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _mailSender;

        public TopicController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IPost postService, IForum forumService, UserManager<ForumUser> userManager,IWebHostEnvironment environment,
            IEmailSender mailSender) : base(memberService, config, localizerFactory,dbContext, httpContextAccessor)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _environment = environment;
            _mailSender = mailSender;
        }

        [Route("{id:int}")]
        [Route("Topic/{id}")]
        [Route("Topic/Index/{id}")]
        public IActionResult Index(int id,int page = 1, int pagesize = 20, string sortdir="desc", int? replyid = null)
        {
            if (User.Identity is { IsAuthenticated: true })
            {
                _memberService.SetLastHere(User);
            }
            var post = _postService.GetTopic(id);
            if (post.ReplyCount > 0)
            {
                post = _postService.GetTopicWithRelated(id);
            }
            if (!HttpContext.Session.Keys.Contains("TopicId_"+ id))
            {
                HttpContext.Session.SetInt32("TopicId_"+ id,1);
                post.ViewCount += 1;
                _postService.UpdateViewCount(post.Id);
            }


            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", post.Category?.Name){ Parent = homePage,RouteValues = new{id=post.Category?.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", post.Forum?.Title){ Parent = catPage,RouteValues = new{id=post.ForumId}};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", post.Title) { Parent = forumPage };
            ViewData["BreadcrumbNode"] = topicPage;
            
            ViewData["Title"] = post.Title;
            var totalCount = post.Replies?.Count();
            var pageCount = 1;
            if (totalCount > 0)
            {
                pageCount = (int)Math.Ceiling((double)totalCount! / pagesize);
            }

            PagedList<PostReply>? pagedReplies = PagedReplies(page, pagesize, sortdir, post);
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
                Title = post.Title,
                Author = post.Member!,
                AuthorId = post.Member!.Id,
                ShowSig = post.Sig == 1,
                Views = post.ViewCount,
                Status = post.Status,
                IsLocked = post.Status == 0 || post.Forum?.Status == 0,
                IsSticky = post.IsSticky == 1,
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
                EditedBy = post.LastEditby == null ? "" : _memberService.GetMemberName(post.LastEditby.Value)
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Create(int id)
        {
            var member = await _memberService.GetById(User);
            var forum = _forumService.GetById(id);
            var model = new NewPostModel()
            {
                Id = 0,
                CatId = forum.CategoryId,
                ForumName = forum.Title,
                ForumId = id,
                IsPost = true,
                AuthorName = User.Identity?.Name!,
                UseSignature = member!.SigDefault == 1,
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
            var topic = _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetById(topic.ForumId);
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
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Create Reply") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public async Task<IActionResult> QuoteAsync(int id)
        {

            var member = await _memberService.GetById(User);
            var topic = _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetById(topic.ForumId);
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
                Lock = topic.Status == 1,
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
            var topic = _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetById(topic.ForumId);
            var model = new NewPostModel()
            {
                Id = id,
                TopicId = id,
                ForumName = forum.Title,
                ForumId = topic.ForumId,
                CatId = forum.CategoryId,
                Title = topic.Title,
                IsPost = true,
                Content = topic.Content,
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 1,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = topic.Created.FromForumDateStr(),
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", forum.Category?.Name){ Parent = homePage,RouteValues = new{id=forum.Category!.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", forum.Title){ Parent = catPage,RouteValues = new{id=forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Edit", "Topic", "Edit Post") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("Create", model);

        }
        [Authorize]
        public async Task<IActionResult> QuoteReply(int id)
        {

            var member = await _memberService.GetById(User);
            var reply = _postService.GetReply(id);
            
            var topic = _postService.GetTopicWithRelated(reply.PostId);
            var model = new NewPostModel()
            {
                Id = 0,
                TopicId = topic.Id,
                ForumName = topic.Forum!.Title,
                ForumId = topic.ForumId,
                CatId = topic.CategoryId,
                IsPost = false,
                Content = $"[quote]{reply.Content}[/quote=Originally posted by {reply.Member!.Name}]<br/>",
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 1,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = DateTime.UtcNow
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
            var topic = _postService.GetTopicWithRelated(reply.PostId);
            var model = new NewPostModel()
            {
                Id = id,
                TopicId = topic.Id,
                ForumName = topic.Forum!.Title,
                ForumId = topic.ForumId,
                CatId = topic.CategoryId,
                IsPost = false,
                Content = reply.Content,
                AuthorName = member!.Name,
                UseSignature = member.SigDefault == 1,
                Lock = topic.Status == 0,
                Sticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = reply.Created.FromForumDateStr(),
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
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            var post = BuildPost(model, user!.MemberId);
            if (model.TopicId != 0)
            {
                post.Title = model.Title;
                post.IsSticky = (short)(model.Sticky ? 1 : 0);
                post.Sig = (short)(model.UseSignature ? 1 : 0);
                post.Status = (short)(model.Lock ? 0 : 1);
                post.ArchiveFlag = model.DoNotArchive ? 1 : 0;
                post.Content = model.Content;
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
        [Route("AddReply/")]
        public async Task<IActionResult> AddReply(NewPostModel model)
        {
            //var userId = _userManager.GetUserId(User);
            //var user = await _userManager.FindByIdAsync(userId!);
            var member = _memberService.GetByUsername(User.Identity.Name);
            var reply = BuildReply(model, member.Id);
            if (model.Id != 0)
            {
                reply.Content = model.Content;
                reply.Sig = (short)(model.UseSignature ? 1 : 0);
                reply.LastEdited = DateTime.UtcNow.ToForumDateStr();
                reply.LastEditby = member.Id;
                await _postService.Update(reply);
            }
            else
            {
                await _postService.Create(reply);
            }
            var topic = _postService.GetTopicForUpdate(reply.PostId);
            topic.Status = (short)(model.Lock ? 0 : 1);
            topic.IsSticky = (short)(model.Sticky ? 1 : 0);
            topic.ArchiveFlag = (short)(model.DoNotArchive ? 1 : 0);
            _postService?.Update(topic);
            
            // TODO: Implement User Rating Management
            return RedirectToAction("Index", "Topic", new { id = reply.PostId });
        }
        [HttpPost]
        [Authorize]
        [Route("Topic/DeleteReply/")]
        public async Task<IActionResult> DeleteReply(int id)
        {

            var member = _memberService.GetById(User).Result;
            var post = _postService.GetReply(id);
            //if this isn't the last post then can't delete it
            if ((post.MemberId == member!.Id && post!.Topic!.LastPostReplyId != id) && !member.Roles.Contains("Administrator") && post.Status < 2)
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
            var post = _postService.GetTopic(id);
            if (member != null && (member.Roles.Contains("Administrator") || post.MemberId == member.Id))
            {
                await _postService.DeleteTopic(id);
                return Json(new { result = true, url = Url.Action("Index","Forum", new{id=post.ForumId}) });

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
            return result ? Json(new { result = result, data = id }) : Json(new { result = result, error = "Unable to toggle Status" });
            
        }

        [Authorize]
        public ActionResult SendTo(int id, int archived)
        {
            EmailViewModel em = new EmailViewModel();
            //var archived = Request.Query["archived"] == "1";
            
            if (archived == 1)
            {
                var topic = _postService.GetArchivedTopic(id);
                Member from = _memberService.Current();
                
                if (from != null)
                {
                    em.FromEmail = from.Email;
                    em.FromName = from.Name;
                }
                else
                {
                    ViewBag.Sent = true;
                    ViewBag.Error = "Error loading data";
                    return View("Error");
                }
                em.ReturnUrl = topic.Id.ToString();
                
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
                var topic = _postService.GetTopic(id);
                Member from = _memberService.Current();
                
                if (from != null)
                {
                    em.FromEmail = from.Email;
                    em.FromName = from.Name;
                }
                else
                {
                    ViewBag.Sent = true;
                    ViewBag.Error = "Error loading data";
                    return View("Error");
                }
                em.ReturnUrl = topic.Id.ToString();
                
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

            ViewBag.Error = _languageResource["InvalidID"]; 
            return PartialView("_Error");
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SendTo(EmailViewModel model)
        {
            
            _mailSender.SendToFreind( model);


            TempData["Success"] = "Email sent successfully";
            return RedirectToAction("Index", "Topic", new { id=model.ReturnUrl, pagenum = -1 });
            //return PartialView("popSendTo",model);
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
                PagedList<ArchivedReply>? pagedReplies = PagedReplies(1, 100, "desc", topic);
                model = new PostIndexModel()
                {
                    Id = topic.Id,
                    Title = topic.Subject,
                    Author = topic.Member!,
                    AuthorId = topic.Member!.Id,
                    ShowSig = topic.Sig == 1,
                    Views = topic.ViewCount,
                    Status = topic.Status,
                    IsLocked = topic.Status == 0 || topic.Forum?.Status == 0,
                    IsSticky = topic.IsSticky == 1,
                    //AuthorRating = post.User?.Rating ?? 0,
                    AuthorName = topic.Member?.Name ?? "Unknown",
                    Created = topic.Date.FromForumDateStr(),
                    Content = topic.Message,
                    Replies = pagedReplies != null ? BuildPostReplies(pagedReplies) : null,
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
            else
            {
                var topic = _postService.GetTopic(id);
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
                    topic = _postService.GetTopicWithRelated(id);
                }
                PagedList<PostReply>? pagedReplies = PagedReplies(1, 100, "desc", topic);
                model = new PostIndexModel()
                {
                    Id = topic.Id,
                    Title = topic.Title,
                    Author = topic.Member!,
                    AuthorId = topic.Member!.Id,
                    ShowSig = topic.Sig == 1,
                    Views = topic.ViewCount,
                    IsLocked = topic.Status == 0 || topic.Forum?.Status == 0,
                    IsSticky = topic.IsSticky == 1,
                    Answered = topic.Answered,
                    //AuthorRating = post.User?.Rating ?? 0,
                    AuthorName = topic.Member?.Name ?? "Unknown",
                    Created = topic.Created.FromForumDateStr(),
                    Content = topic.Content,
                    Replies = pagedReplies != null ? BuildPostReplies(pagedReplies) : null,
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
                var topic = _postService.GetTopic(vm.Id);
                var author = _memberService.GetById(topic.MemberId);
                var forum = _forumService.GetById(topic.ForumId);
                var subject = "";
                var message = "";
                

                switch (vm.PostStatus)
                {
                    case "Approve" :
                        await _postService.SetStatus(vm.Id, Status.Open);
                        await _postService.UpdateLastPost(vm.Id,null);
                        //update Forum
                        forum = await _forumService.UpdateLastPost(topic.ForumId);
                        if (forum.CountMemberPosts == 1)
                        {
                            await _memberService.UpdatePostCount(topic.MemberId);
                        }
                        //Send email
                            subject = _config.ForumTitle + ": Post Approved";
                            message = "Has been approved. You can view it at " + Environment.NewLine +
                                      _config.ForumUrl + "Topic/Posts/" + topic.Id + "?pagenum=-1" +
                                            Environment.NewLine +
                                            vm.ApprovalMessage;
                        var sub = (SubscriptionLevel)_config.GetIntValue("STRSUBCRIPTION");
                        if (!(sub == SubscriptionLevel.None || sub == SubscriptionLevel.Topic))
                        {
                            switch ((Subscription)forum.Subscription)
                            {
                                case Subscription.ForumSubscription:
                                    //TODO: BackgroundJob.Enqueue(() => ProcessSubscriptions.Topic(vm.Id));
                                    break;
                            }
                        }
                        break;
                    case "Reject":
                        
                        _postService.DeleteTopic(topic.Id);
                        //Send email
                            subject = _config.ForumTitle + ": Post rejected";
                            message = "Has been rejected. " + Environment.NewLine +
                                            vm.ApprovalMessage;
                        break;
                    case "Hold":
                        await _postService.SetStatus(vm.Id, Status.OnHold);
                        //Send email
                            subject = _config.ForumTitle + ": Post placed on Hold";
                            message = "Has been placed on Hold. " + Environment.NewLine +
                                            vm.ApprovalMessage;
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
        private Post BuildPost(NewPostModel model, int memberid)
        {
            if (model.TopicId != 0)
            {
                return _postService.GetTopic(model.TopicId);
            }

            var donotModerate = User.IsInRole("Administrator") || User.IsInRole("Forum_" + model.ForumId);
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
                CategoryId = model.CatId,
                //Forum = forum,
                IsSticky = (short)(model.Sticky ? 1 : 0),
                ArchiveFlag = model.DoNotArchive ? 1 : 0,
                Status = (forum.Moderation == Moderation.AllPosts || forum.Moderation == Moderation.Topics) && !(donotModerate)
                ? (short)Status.UnModerated
                : (short)Status.Open,
                Sig = (short)(model.UseSignature ? 1 : 0),
            };

        }
        private PostReply BuildReply(NewPostModel model, int memberid)
        {
            if (model.Id != 0)
            {
                return _postService.GetReplyForUdate(model.Id);
            }

            var donotModerate = User.IsInRole("Administrator") || User.IsInRole("Forum_" + model.ForumId);
            var forum = _forumService.GetById(model.ForumId);
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
                EditedBy = reply.LastEditby == null ? "" : _memberService.GetMemberName(reply.LastEditby.Value)
            });
        }
        private PagedList<PostReply>? PagedReplies(int page, int pagesize, string sortdir, Post post)
        {
            if(post.ReplyCount < 1) return null;
            var pagedReplies = sortdir == "asc"
                ? new PagedList<PostReply>(post?.Replies?.OrderBy(r => r.Created), page, pagesize)
                : new PagedList<PostReply>(post?.Replies?.OrderByDescending(r => r.Created), page, pagesize);
            return pagedReplies;
        }
        private PagedList<ArchivedReply>? PagedReplies(int page, int pagesize, string sortdir, ArchivedTopic post)
        {
            if(post.ReplyCount < 1) return null;
            var pagedReplies = sortdir == "asc"
                ? new PagedList<ArchivedReply>(post?.Replies?.OrderBy(r => r.Created), page, pagesize)
                : new PagedList<ArchivedReply>(post?.Replies?.OrderByDescending(r => r.Created), page, pagesize);
            return pagedReplies;
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
    }
}

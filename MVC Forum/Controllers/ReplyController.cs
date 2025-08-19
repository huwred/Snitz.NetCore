using BbCodeFormatter;
using BbCodeFormatter.Processors;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels.Post;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MVCForum.Controllers
{
    public class ReplyController : SnitzBaseController
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        private readonly UserManager<ForumUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _mailSender;
        private readonly ISubscriptions _processSubscriptions;
        private readonly IPrivateMessage _pmService;
        private readonly ICodeProcessor _bbcodeProcessor;

        public ReplyController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IPost postService, IForum forumService, UserManager<ForumUser> userManager,IWebHostEnvironment environment,
            IEmailSender mailSender, ISubscriptions processSubscriptions,IPrivateMessage pmService,ICodeProcessor bbcodeProcessor) : base(memberService, config, localizerFactory,dbContext, httpContextAccessor)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _environment = environment;
            _mailSender = mailSender;
            _processSubscriptions = processSubscriptions;
            _pmService = pmService;
            _bbcodeProcessor = bbcodeProcessor;
        }

        /// <summary>
        /// Displays the form for creating a new post in a specified topic.
        /// </summary>
        /// <remarks>This method enforces several conditions before allowing post creation: 
        /// <list type="bullet"> <item>Flood control: If flood control is enabled, users must wait a specified amount of time
        /// between posts.</item> <item>Authorization: Users must have appropriate permissions to reply in the topic's
        /// forum.</item> </list> The returned view includes a model pre-populated with relevant topic and forum
        /// details.</remarks>
        /// <param name="id">The unique identifier of the topic where the post will be created.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the post creation view if the user has the necessary
        /// permissions. If the user is restricted or violates flood control rules, an error view is returned.</returns>
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
            var topic = await _postService.GetTopicWithRelated(id);
            
            var forum = _forumService.GetWithPosts(topic!.ForumId);
            if(forum.Replyauth != (int)PostAuthType.Anyone)
            {
                var moderator = User.IsInRole("Forum_" + id);
                var admin = User.IsInRole("Administrator");
                ViewBag.Error = _languageResource.GetString($"lblReplyAuthType_{(PostAuthType)forum.Replyauth}");

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
                TopicId = id,
                ForumName = forum.Title,
                ForumId = topic.ForumId,
                CatId = forum.CategoryId,
                IsPost = false,
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
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "lblReply") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("../Topic/Create", model);

        }

        /// <summary>
        /// Adds a reply to a topic in the forum.
        /// </summary>
        /// <remarks>This method handles the creation or update of a reply to a forum topic. It performs
        /// flood-check validation to prevent rapid consecutive posts, updates the topic's metadata, and triggers
        /// additional processes such as subscription notifications and private messages for mentioned users.</remarks>
        /// <param name="model">The <see cref="NewPostModel"/> containing the details of the reply, including the topic ID, content, and
        /// optional flags for locking, stickiness, and archiving.</param>
        /// <returns>A JSON result containing the URL of the updated topic and the ID of the reply.</returns>
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
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new{err=_languageResource.GetString("FloodcheckErr", timeout),url=Url.Action("Index", "Topic", new { id = model.TopicId }),id=model.TopicId});
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
                    var forum = _forumService.Get(reply.ForumId);
                    var sub = _config.GetIntValue("STRSUBSCRIPTION");
                    //if we are sending out subscriptions
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
                    //if we are sending PMs to users mentioned in the post
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
            int moderated = 0;
            //update topic stuff
            if (reply.Status == (short)Status.UnModerated)
            {
                moderated = 1;
            }

            var topic = _postService.GetTopicForUpdate(reply.PostId);
            topic.UnmoderatedReplies += moderated;
            topic.LastPostReplyId = reply.Id;
            topic.LastPostDate = reply.Created;
            topic.LastPostAuthorId = reply.MemberId;
            topic.ReplyCount += (1 - moderated);
            topic.Status = (short)(model.IsLocked ? 0 : 1);
            topic.IsSticky = (short)(model.IsSticky ? 1 : 0);
            topic.ArchiveFlag = (short)(model.DoNotArchive ? 1 : 0);
            await _snitzDbContext.SaveChangesAsync();

            await _postService.UpdateReplyTopic(topic);


            // TODO: Implement User Rating Management
            return Json(new{url=Url.Action("Index", "Topic", new { id = reply.PostId,replyid=reply.Id}) });
            //return RedirectToAction("Index", "Topic", new { id = reply.PostId });
        }

        /// <summary>
        /// Displays the edit view for a reply in a forum topic.
        /// </summary>
        /// <remarks>This method retrieves the reply, its associated topic, and related forum details to
        /// populate the edit view. The view model includes information such as the reply content, author details, topic
        /// status, and forum metadata. The method also sets up breadcrumb navigation for the view.</remarks>
        /// <param name="id">The unique identifier of the reply to be edited.</param>
        /// <param name="archived">A value indicating whether the reply is archived.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the edit view for the reply.</returns>
        [Authorize]
        public async Task<IActionResult> Edit(int id, bool archived)
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
                IsLocked = topic.Status == 0,
                IsSticky = topic.IsSticky == 1,
                DoNotArchive = topic.ArchiveFlag == 1,
                Created = reply.Created.FromForumDateStr(),
                IsAuthor = User.Identity?.Name == member!.Name,
                IsArchived = archived
            };
            var homePage = new MvcBreadcrumbNode("", "Category", "ttlForums");
            var catPage = new MvcBreadcrumbNode("", "Category", topic.Category!.Name){ Parent = homePage,RouteValues = new{id=topic.Category.Id}};
            var forumPage = new MvcBreadcrumbNode("Index", "Forum", topic.Forum.Title){ Parent = catPage,RouteValues = new{id=topic.Forum.Id }};
            var topicPage = new MvcBreadcrumbNode("Index", "Topic", topic.Title) { Parent = forumPage,RouteValues = new{id=topic.Id} };
            var replyPage = new MvcBreadcrumbNode("Quote", "Topic", "Edit Reply") { Parent = topicPage };
            ViewData["BreadcrumbNode"] = replyPage;

            return View("../Topic/Create", model);

        }

        /// <summary>
        /// Deletes a reply from a topic or an archived reply based on the specified parameters.
        /// </summary>
        /// <remarks>The reply can only be deleted if the user has sufficient permissions and the reply
        /// meets certain conditions: - The reply must not be the last reply in the topic unless the user is an
        /// administrator. - The reply must have a status less than 2. - If <paramref name="archived"/> is <see
        /// langword="true"/>, the reply must exist in the archived replies.</remarks>
        /// <param name="id">The unique identifier of the reply to delete.</param>
        /// <param name="archived">A value indicating whether the reply to delete is archived.  <see langword="true"/> if the reply is
        /// archived; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the delete operation.  If successful, the result
        /// includes the topic ID associated with the deleted reply. If the operation fails, the result includes an
        /// error message.</returns>
        [HttpPost]
        [Authorize]
        [Route("Reply/Delete/")]
        public async Task<IActionResult> Delete(int id, bool archived)
        {

            var member = _memberService.GetById(User).Result;
            var post = _postService.GetReply(id);
            //if this isn't the last post then can't delete it
            if ((post!.MemberId == member!.Id && post!.Topic!.LastPostReplyId != id) && !member.Roles.Contains("Administrator") && post.Status < 2)
            {
                ModelState.AddModelError("","Unable to delete this reply");
                return Json(new { result = false, error = "Unable to delete this reply" });

            }
            if(archived)
            {
                var archivedpost = _postService.GetArchivedReply(id);
                if (archivedpost == null)
                {
                    ModelState.AddModelError("","Unable to delete this reply");
                    return Json(new { result = false, error = "Unable to delete this reply" });
                }
                await _postService.DeleteArchivedReply(id);
                return Json(new { result = true, data = archivedpost.Topic!.Id });
            }
            await _postService.DeleteReply(id);
            return Json(new { result = true, data = post.Topic!.Id });

        }

        /// <summary>
        /// Displays a form for creating a new post in response to an existing post, including a quoted reply.
        /// </summary>
        /// <remarks>This method retrieves the original post and its associated topic, forum, and category
        /// information. It checks for flood control restrictions based on the user's last post time and applies them if
        /// enabled. The method constructs a model pre-filled with the quoted content and other relevant details for the
        /// new post. Breadcrumb navigation data is also set to guide the user through the forum hierarchy.</remarks>
        /// <param name="id">The identifier of the post to quote.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the post creation view with the pre-filled model. If flood
        /// control restrictions are violated, an error view is returned.</returns>
        [Authorize]
        public async Task<IActionResult> Quote(int id)
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
                IsLocked = topic.Status == 0,
                IsSticky = topic.IsSticky == 1,
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

            return View("../Topic/Create", model);

        }

        /// <summary>
        /// Opens a Modal dialog to mederate the specified topic.
        /// </summary>
        /// <remarks>This action is restricted to users with the roles "Administrator" or
        /// "Moderator."</remarks>
        /// <param name="id">The unique identifier of the topic to be moderated.</param>
        /// <returns>A <see cref="PartialViewResult"/> that renders the Modal dialog for the topic.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public PartialViewResult Moderate(int id)
        {
            ApproveTopicViewModal vm = new ApproveTopicViewModal {Id = id};
            return PartialView("popModerate",vm);
        }

        /// <summary>
        /// Moderates a forum post by approving, rejecting, or placing it on hold based on the provided status.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized as an
        /// Administrator or Moderator. It validates the provided moderation details and performs the appropriate action
        /// on the post: approving, rejecting, or placing it on hold. If the <paramref name="vm.EmailAuthor"/> property
        /// is set to <see langword="true"/>, an email notification is sent to the post's author.</remarks>
        /// <param name="vm">The view model containing the moderation details, including the post ID, moderation status, optional
        /// approval message, and whether to notify the author via email.</param>
        /// <returns>An <see cref="ActionResult"/> that redirects to the topic page if moderation is successful, or renders a
        /// partial view with the provided view model if the model state is invalid.</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        [ValidateAntiForgeryToken]
        [Route("Reply/ModeratePost/")]
        public async Task<ActionResult> ModeratePost(ApproveTopicViewModal vm)
        {

            if (ModelState.IsValid)
            {
                var reply = _postService.GetReply(vm.Id);
                var author = _memberService.GetById(reply.MemberId);
                var forum = _forumService.GetWithPosts(reply.ForumId);
                var subject = "";
                var message = "";
                
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                var topicLink = Url.Action("Index", "Topic", new { id = reply.PostId, pagenum=-1}, Request.Scheme);
                switch (vm.PostStatus)
                {
                    case "Approve" :
                        await _postService.SetReplyStatus(vm.Id, Status.Open);
                        await _postService.UpdateLastPost(reply.PostId, -1);
                        //update Forum
                        forum = await _forumService.UpdateLastPost(reply.ForumId);
                        if (forum.CountMemberPosts == 1)
                        {
                            await _memberService.UpdatePostCount(reply.MemberId);
                        }
                        //Send email
                            subject = _config.ForumTitle + ": Reply Approved";
                            message = 
                            _mailSender.ParseTemplate("approvePost.html",_languageResource["tipApproveReply"].Value,author.Email,author.Name, topicLink, cultureInfo.Name,vm.ApprovalMessage);

                        var sub = (SubscriptionLevel)_config.GetIntValue("STRSUBSCRIPTION");
                        if (!(sub == SubscriptionLevel.None || sub == SubscriptionLevel.Topic))
                        {
                            switch ((ForumSubscription)forum.Subscription)
                            {
                                case ForumSubscription.ForumSubscription:
                                case ForumSubscription.TopicSubscription:
                                    BackgroundJob.Enqueue(() => _processSubscriptions.Reply(vm.Id));
                                    break;
                            }
                        }
                        break;
                    case "Reject":
                        await _postService.DeleteReply(reply.Id);
                        await _postService.UpdateLastPost(reply.PostId, -1);
                        //Send email
                            subject = _config.ForumTitle + ": Reply rejected";
                            message = 
                            _mailSender.ParseTemplate("rejectPost.html",_languageResource["tipRejectReply"].Value,author.Email,author.Name, "", cultureInfo.Name,vm.ApprovalMessage);

                        break;
                    case "Hold":
                        await _postService.SetReplyStatus(vm.Id, Status.OnHold);
                        //Send email

                            subject = _config.ForumTitle + ": Reply placed on Hold";
                            message = 
                            _mailSender.ParseTemplate("onholdPost.html",_languageResource["tipOnHoldReply"].Value,author.Email,author.Name, topicLink, cultureInfo.Name,vm.ApprovalMessage);

                        break;
                }
                if (vm.EmailAuthor)
                {
                    _mailSender.ModerationEmail(author, subject, message, forum, reply);
                }
                
                return RedirectToAction("Index", "Topic", new { id=reply.PostId});
            }

            return PartialView("popModerate",vm);
        }

        /// <summary>
        /// Builds a reply to a post based on the provided model and member ID.
        /// </summary>
        /// <remarks>If the <paramref name="model"/> specifies an existing post ID, the method retrieves
        /// the reply for update using the post service. Otherwise, it creates a new reply based on the provided model
        /// and member ID, applying moderation rules based on the forum's settings and the user's roles.</remarks>
        /// <param name="model">The model containing the details of the new post, including its content, forum ID, and other metadata.</param>
        /// <param name="memberid">The ID of the member creating the reply.</param>
        /// <returns>A <see cref="PostReply"/> object representing the reply to the post, or <see langword="null"/> if the reply
        /// is being updated and the corresponding reply cannot be found.</returns>
        private PostReply? BuildReply(NewPostModel model, int memberid)
        {
            if (model.Id != 0)
            {
                return _postService.GetReplyForUdate(model.Id);
            }

            var donotModerate = User.IsInRole("Administrator") || User.IsInRole("Forum_" + model.ForumId);
            var forum = _forumService.Get(model.ForumId);
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
                CategoryId = forum.CategoryId,
                Sig = (short)(model.UseSignature ? 1 : 0),
                Answer = model.Answer
            };

        }

    }
}

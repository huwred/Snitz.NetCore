using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCForum.ViewModels.Post;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using NuGet.Common;
using System.Globalization;
using System.Threading;
using Hangfire;

namespace MVCForum.Controllers
{
    public class ReplyController : SnitzController
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        private readonly UserManager<ForumUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _mailSender;
        private readonly ISubscriptions _processSubscriptions;

        public ReplyController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IPost postService, IForum forumService, UserManager<ForumUser> userManager,IWebHostEnvironment environment,
            IEmailSender mailSender, ISubscriptions processSubscriptions) : base(memberService, config, localizerFactory,dbContext, httpContextAccessor)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _environment = environment;
            _mailSender = mailSender;
            _processSubscriptions = processSubscriptions;
        }
        /// <summary>
        /// Open moderation Popup window
        /// </summary>
        /// <param name="id">Id of Unmoderated <see cref="PostReply"/></param>
        /// <returns>PopUp Window</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public PartialViewResult Moderate(int id)
        {
            ApproveTopicViewModal vm = new ApproveTopicViewModal {Id = id};
            return PartialView("popModerate",vm);
        }
        /// <summary>
        /// Process Reply Moderation
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
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
                var forum = _forumService.GetById(reply.ForumId);
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
                            _mailSender.ParseTemplate("approvePost.html",_languageResource["tipApproveReply"].Value,author.Email,author.Name, topicLink, cultureInfo,vm.ApprovalMessage);

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
                            _mailSender.ParseTemplate("rejectPost.html",_languageResource["tipRejectReply"].Value,author.Email,author.Name, "", cultureInfo,vm.ApprovalMessage);

                        break;
                    case "Hold":
                        await _postService.SetReplyStatus(vm.Id, Status.OnHold);
                        //Send email

                            subject = _config.ForumTitle + ": Reply placed on Hold";
                            message = 
                            _mailSender.ParseTemplate("onholdPost.html",_languageResource["tipOnHoldReply"].Value,author.Email,author.Name, topicLink, cultureInfo,vm.ApprovalMessage);

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
    }
}

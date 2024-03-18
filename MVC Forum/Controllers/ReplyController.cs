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

namespace MVCForum.Controllers
{
    public class ReplyController : SnitzController
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        private readonly UserManager<ForumUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _mailSender;

        public ReplyController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IPost postService, IForum forumService, UserManager<ForumUser> userManager,IWebHostEnvironment environment,
            IEmailSender mailSender) : base(memberService, config, localizerFactory,dbContext, httpContextAccessor)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _environment = environment;
            _mailSender = mailSender;
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
                            subject = _config.ForumTitle + ": Post Approved";
                            message = "Has been approved. You can view it at " + Environment.NewLine +
                                      _config.ForumUrl + "Topic/Posts/" + reply.PostId + "?pagenum=-1" +
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
                        
                        _postService.DeleteReply(reply.Id);
                        //Send email
                            subject = _config.ForumTitle + ": Post rejected";
                            message = "Has been rejected. " + Environment.NewLine +
                                            vm.ApprovalMessage;
                        break;
                    case "Hold":
                        await _postService.SetReplyStatus(vm.Id, Status.OnHold);
                        //Send email
                            subject = _config.ForumTitle + ": Post placed on Hold";
                            message = "Has been placed on Hold. " + Environment.NewLine +
                                            vm.ApprovalMessage;
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

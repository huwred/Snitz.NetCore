using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels.Post;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using SnitzCore.Data.Extensions;

namespace MVCForum.Controllers
{
    public class PollsController : SnitzController
    {
        private readonly ISnitzCookie _snitzCookie;
        public PollsController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory, SnitzDbContext dbContext, IHttpContextAccessor httpContextAccessor,
            ISnitzCookie snitzCookie) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _snitzCookie = snitzCookie;
        }        
        [HttpPost]
        public IActionResult Vote(FormCollection form)
        {
            Member? curUser = _memberService.Current();
            var poll = _snitzDbContext.Polls.Find(Convert.ToInt32(form["PollId"]));
            if (poll != null)
            {
                poll.Lastvote = DateTime.UtcNow.ToForumDateStr();
                _snitzDbContext.SaveChanges();
                var answer = _snitzDbContext.PollAnswers.Find(Convert.ToInt32(form["voteid"]));
                if (poll.Whovotes == "everyone")
                {
                    _snitzDbContext.PollVotes.Add(new PollVote()
                    {
                        MemberId = curUser?.Id ?? 0,
                        PollId = poll.Id,
                        ForumId = poll.ForumId,
                        CategoryId = poll.CatId,
                        PostId = poll.TopicId,
                        GuestVote = (curUser == null) ? 1 : 0
                    });
                    if (answer != null) answer.Count += 1;
                    _snitzDbContext.SaveChanges();
                    _snitzCookie.PollVote(poll.Id);
                }
                else
                {
                    _snitzDbContext.PollVotes.Add(new PollVote()
                    {
                        MemberId = curUser?.Id ?? 0,
                        PollId = poll.Id,
                        ForumId = poll.ForumId,
                        CategoryId = poll.CatId,
                        PostId = poll.TopicId,
                        GuestVote = 0
                    });
                    if (answer != null) answer.Count += 1;
                    _snitzDbContext.SaveChanges();
                    _snitzCookie.PollVote(poll.Id);
                }
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }        
        //
        // GET: /Polls/Delete/5
        /// <summary>
        /// Remove a Poll
        /// </summary>
        /// <param name="id">Id of Poll</param>
        /// <param name="returnUrl">Url of calling page</param>
        /// <returns></returns>        
        [Authorize(Roles = "Administrator,Moderator")]
        public IActionResult Delete(int id, string returnUrl)
        {
            var poll = _snitzDbContext.Polls.Find(id);
            if (poll != null)
            {
                _snitzDbContext.Polls.Remove(poll);
                _snitzDbContext.SaveChanges();
            }
            if (_config.GetIntValue("INTFEATUREDPOLLID") == id)
            {
                _config.SetValue("INTFEATUREDPOLLID", "0");
            }

            return Redirect(Request.Headers["Referer"].ToString());

        }

        [Authorize(Roles = "Administrator,Moderator")]
        public IActionResult Lock(int id, int pollid)
        {

            var topic = _snitzDbContext.Posts.Find(id);
            if (topic != null)
            {
                topic.Status = 0;
                _snitzDbContext.SaveChanges();
            }

            if (_config.GetIntValue("INTFEATUREDPOLLID") == pollid)
            {
                _config.SetValue("INTFEATUREDPOLLID", "0");
            }

            return Redirect(Request.Headers["Referer"].ToString());

        }
        [Authorize(Roles = "Administrator")]
        public IActionResult MakeFeaturedPoll(int id)
        {
            var routinfo = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
            _config.SetValue("INTFEATUREDPOLLID", id.ToString());

            return Redirect(Request.Headers["Referer"].ToString());            
        }

    }
}

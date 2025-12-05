using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using SnitzCore.Data.Extensions;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MVCForum.Controllers
{
    public class PollsController : SnitzBaseController
    {
        private readonly ISnitzCookie _snitzCookie;
        public PollsController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory, SnitzDbContext dbContext, IHttpContextAccessor httpContextAccessor,
            ISnitzCookie snitzCookie) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _snitzCookie = snitzCookie;
        }

        public IActionResult Active()
        {

            var polls = _snitzDbContext.Polls.AsNoTracking().Include(p=>p.Topic).AsQueryable();
            if (!User.IsInRole("Administrator"))
            {
                polls = polls.Where(p => p.Topic.Status == 1);
            }

            return View(polls.ToList());
        }
        [HttpPost]
        public IActionResult Vote(IFormCollection form)
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
                    if (answer != null)
                    {
                        answer.Count += 1;
                        _snitzDbContext.PollAnswers.Update(answer);
                        _snitzDbContext.SaveChanges();
                    }
                    
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
                    if (answer != null)
                    {
                        answer.Count += 1;
                        _snitzDbContext.PollAnswers.Update(answer);
                        _snitzDbContext.SaveChanges();
                    }

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
                var topicid = poll.TopicId;
                var topic = _snitzDbContext.Posts.Find(topicid);
                if(topic != null)
                { 
                    topic.Ispoll = 0;
                    _snitzDbContext.Update(topic);
                }
                _snitzDbContext.Polls.Remove(poll);

                _snitzDbContext.SaveChanges();
                _snitzDbContext.PollAnswers.Where(a=>a.PollId == id).ExecuteDelete();
                _snitzDbContext.PollVotes.Where(a=>a.PollId == id).ExecuteDelete();
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
                if(topic.Status == 1)
                {
                    topic.Status = 0;
                    if (_config.GetIntValue("INTFEATUREDPOLLID") == pollid)
                    {
                        _config.SetValue("INTFEATUREDPOLLID", "0");
                    }                
                }else
                    { topic.Status = 1; }

                _snitzDbContext.Update(topic);
                _snitzDbContext.SaveChanges();
            }

            return Redirect(Request.Headers["Referer"].ToString());

        }
        [Authorize(Roles = "Administrator")]
        public IActionResult MakeFeaturedPoll(int id)
        {
            _config.SetValue("INTFEATUREDPOLLID", id.ToString());
            return Redirect(Request.Headers["Referer"].ToString());            
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult SaveForum(IFormCollection form)
        {
            try
            {
                var forum = _snitzDbContext.Forums.Find(Convert.ToInt32(form["ForumId"]));
                if (forum != null)
                {
                    forum.Polls = Convert.ToInt32(form["PollsAuth"]);
                    _snitzDbContext.Forums.Update(forum);
                    _snitzDbContext.SaveChanges();
                    return Content("Poll confg updated");
                }
                return Content("Problem updating");
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }
    }
}

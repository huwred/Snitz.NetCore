using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SnitzCore.Service
{
    public class ProcessSubscriptions : ISubscriptions
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly IEmailSender _emailsender;

        public ProcessSubscriptions( SnitzDbContext dbContext, 
            ISnitzConfig config,IEmailSender emailsender)
        {
            _dbContext = dbContext;
            _config = config;
            _emailsender = emailsender;
        }
        /// <summary>
        /// Process subscriptions for a new topic
        /// </summary>
        /// <param name="topicid"></param>
        public void Topic(int topicid)
        {
            var topic = _dbContext.Posts.Include(r=>r.Member).SingleOrDefault(r=>r.Id == topicid);
            if (topic==null || topic.Id == 0)
            {
                return;
            }

            foreach (var a in _dbContext.MemberSubscription.Include(s=>s.Member)
                         .Include(s=>s.Forum)
                         .Include(s=>s.Category).Where(s=>s.PostId == topicid || s.ForumId == topic.ForumId))
            {

                dynamic email = new TopicSubscriptionEmail();
                email.To = a.Member.Email;
                email.UserName = a.Member.Name;
                email.Subject = _config.ForumTitle;
                email.ForumTitle = _config.ForumTitle;
                email.Unsubscribe = string.Format("{0}Topic/UnSubscribe/{1}?forumid={2}&catid={3}&userid={4}", _config.ForumUrl, a.PostId, a.ForumId, a.CategoryId, a.MemberId);
                email.Topiclink = string.Format("{0}Topic/Posts/{1}?pagenum=-1", _config.ForumUrl, topic.Id);
                email.Author = topic.Member.Name;

                if (a.ForumId > 0 && a.Forum.Subscription == (int)ForumSubscription.ForumSubscription)
                {
                    email.PostedIn = "Forum";
                    email.PostedInName = a.Forum.Title;
                }
                else if (a.Category.Subscription == (int)CategorySubscription.CategorySubscription)
                {
                    email.PostedIn = "Category";
                    email.PostedInName = a.Category.Name;
                }
                try
                {
                    _emailsender.SendEmailAsync(new EmailMessage(new List<string>() { email.To }, email.Subject,
                        "Test message"));
                    //email.Send();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Process subscriptions for new reply
        /// </summary>
        /// <param name="replyid"></param>
        public void Reply(int replyid)
        {

            var reply = _dbContext.Replies.Include(r=>r.Member).SingleOrDefault(r=>r.Id == replyid);
            if (reply == null || reply.Id == 0)
            {
                return;
            }

            foreach (var a in _dbContext.MemberSubscription.Include(s=>s.Member)
                         .Include(s=>s.Post)
                         .Include(s=>s.Forum)
                         .Include(s=>s.Category).Where(s=>s.PostId == reply.PostId || s.ForumId == reply.ForumId))
            {
                try
                {
                    var email = new SubscriptionEmail();
                    email.To = a.Member.Email;
                    email.UserName = a.Member.Name;
                    email.Subject = _config.ForumTitle ;
                    email.Unsubscribe = string.Format("{0}Topic/UnSubscribe/{1}?forumid={2}&catid={3}&userid={4}", _config.ForumUrl, a.PostId, a.ForumId, a.CategoryId, a.MemberId);
                    email.Topiclink = string.Format("{0}Topic/Posts/{1}?pagenum=-1#{2}", _config.ForumUrl, reply.PostId, reply.Id);
                    email.ForumTitle = _config.ForumTitle; 
                    email.Author = reply.Member.Name;

                    if (a.PostId > 0)
                    {
                        email.PostedIn = "Topic";
                        email.PostedInName = a.Post.Title;
                    }
                    else if (a.ForumId > 0 && a.Forum.Subscription == (int)ForumSubscription.ForumSubscription)
                    {
                        email.PostedIn = "Forum";
                        email.PostedInName = a.Forum.Title;
                    }
                    else if (a.Category.Subscription == (int)CategorySubscription.CategorySubscription)
                    {
                        email.PostedIn = "Category";
                        email.PostedInName = a.Category.Name;
                    }

                    _emailsender.SendEmailAsync(new EmailMessage(new List<string>() { email.To }, email.Subject,
                        "Test message"));

                    //email.Send();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }


}

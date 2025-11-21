using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace SnitzCore.Service
{
    public class ProcessSubscriptions : ISubscriptions
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly IEmailSender _emailsender;

        public ProcessSubscriptions( SnitzDbContext dbContext, ISnitzConfig config,IEmailSender emailsender)
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

            var subs = _dbContext.MemberSubscriptions.Include(s => s.Member)
                .Include(s => s.Forum)
                .Include(s => s.Category)
                .Where(s => s.CategoryId == topic.CategoryId || s.ForumId == topic.ForumId)
                .Where(s => s.PostId == 0)
                .Where(s => s.MemberId != topic.MemberId);

            foreach (var a in subs)
            {

                dynamic email = new TopicSubscriptionEmail();
                email.To = a.Member?.Email;
                email.UserName = a.Member?.Name;
                email.Subject = _config.ForumTitle;

                email.Unsubscribe = string.Format("{0}Topic/UnSubscribe/{1}?forumid={2}&catid={3}&userid={4}", _config.ForumUrl, a.PostId, a.ForumId, a.CategoryId, a.MemberId);
                email.Topiclink = string.Format("{0}Topic/Index/{1}?pagenum=-1", _config.ForumUrl, topic.Id);
                email.Author = topic.Member!.Name;

                try
                {
                    if (a.ForumId == topic.ForumId && a.MemberId != topic.MemberId && a.Forum!.Subscription == (int)ForumSubscription.ForumSubscription)
                    {
                        email.PostedIn = "Forum";
                        email.PostedInName = a.Forum.Title;
                        var message = _emailsender.ParseSubscriptionTemplate("topicsubscription.html"
                            , email.PostedIn, email.PostedInName, email.Author, email.UserName, email.Topiclink,
                            email.Unsubscribe, CultureInfo.CurrentUICulture.Name);

                        _emailsender.SendEmailAsync(new EmailMessage(new List<string>() { email.To }, email.Subject,
                            message));
                    }
                    else if (a.CategoryId == topic.CategoryId && a.MemberId != topic.MemberId && a.Category!.Subscription == (int)CategorySubscription.CategorySubscription)
                    {
                        email.PostedIn = "Category";
                        email.PostedInName = a.Category.Name;
                        var message = _emailsender.ParseSubscriptionTemplate("topicsubscription.html"
                            , email.PostedIn, email.PostedInName, email.Author, email.UserName, email.Topiclink,
                            email.Unsubscribe, CultureInfo.CurrentUICulture.Name);

                        _emailsender.SendEmailAsync(new EmailMessage(new List<string>() { email.To }, email.Subject,
                            message));
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
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

            var subs = _dbContext.MemberSubscriptions.Include(s => s.Member)
                .Include(s => s.Post)
                .Include(s => s.Forum)
                .Include(s => s.Category)
                .Where(s => s.PostId == reply.PostId || s.ForumId == reply.ForumId || s.CategoryId == reply.CategoryId)
                .Where(s => s.MemberId != reply.MemberId);

            foreach (var a in subs)
            {
                try
                {
                    var email = new SubscriptionEmail
                    {
                        To = a.Member!.Email!,
                        UserName = a.Member.Name,
                        Subject = _config.ForumTitle!,
                        Unsubscribe = string.Format("{0}Topic/UnSubscribe/{1}?forumid={2}&catid={3}&userid={4}", _config.ForumUrl, a.PostId, a.ForumId, a.CategoryId, a.MemberId),
                        Topiclink = string.Format("{0}Topic/Index/{1}?pagenum=-1#{2}", _config.ForumUrl, reply.PostId, reply.Id),

                        Author = reply.Member!.Name
                    };

                    if (a.PostId == reply.PostId && a.MemberId != reply.MemberId)
                    {
                        email.PostedIn = "Topic";
                        email.PostedInName = a.Post!.Title;
                        var message = _emailsender.ParseSubscriptionTemplate("replysubscription.html"
                            , email.PostedIn, email.PostedInName, email.Author, email.UserName, email.Topiclink,
                            email.Unsubscribe, CultureInfo.CurrentUICulture.Name);

                        _emailsender.SendEmailAsync(new EmailMessage(new List<string>() { email.To! }, email.Subject!,
                            message));

                    }
                    else if (a.ForumId == reply.ForumId && a.MemberId != reply.MemberId && a.Forum!.Subscription == (int)ForumSubscription.ForumSubscription)
                    {
                        email.PostedIn = "Forum";
                        email.PostedInName = a.Forum.Title;
                        var message = _emailsender.ParseSubscriptionTemplate("replysubscription.html"
                            , email.PostedIn, email.PostedInName, email.Author, email.UserName, email.Topiclink,
                            email.Unsubscribe, CultureInfo.CurrentUICulture.Name);

                        _emailsender.SendEmailAsync(new EmailMessage(new List<string>() { email.To! }, email.Subject!,
                            message));

                    }
                    else if (a.CategoryId == reply.CategoryId && a.MemberId != reply.MemberId && a.Category!.Subscription == (int)CategorySubscription.CategorySubscription)
                    {
                        email.PostedIn = "Category";
                        email.PostedInName = a.Category.Name;
                        var message = _emailsender.ParseSubscriptionTemplate("replysubscription.html"
                            , email.PostedIn, email.PostedInName!, email.Author, email.UserName, email.Topiclink,
                            email.Unsubscribe, CultureInfo.CurrentUICulture.Name);
                        if(email != null)
                        {
                            _emailsender.SendEmailAsync(new EmailMessage(new List<string>() { email.To! }, email.Subject!,
                                message));
                        }


                    }


                    //email.Send();
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                }
            }
        }
    }

}

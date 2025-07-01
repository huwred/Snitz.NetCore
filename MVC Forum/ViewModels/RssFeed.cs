using BbCodeFormatter;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;


namespace SnitzCore.Data.Models
{
    public class RssFeed
    {
        private readonly string STitle;
        private readonly string SSiteUrl;
        private readonly string SCopyright;
        private readonly string SDescription;
        private readonly string AppUrl; 
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly ICodeProcessor BbCodeProcessor;
        private readonly IMember _memberService;

        public RssFeed(ISnitzConfig config,SnitzDbContext dbContext, ICodeProcessor bbcodeProcessor,IMember member)
        {
            STitle = config.ForumTitle;
            SSiteUrl = config.ForumUrl;
            SCopyright = config.Copyright;
            SDescription = config.ForumDescription;
            AppUrl = config.ForumUrl; 
            _dbContext = dbContext;
            _config = config;
            BbCodeProcessor = bbcodeProcessor;
            _memberService = member;
        }

        public SyndicationFeed ActiveFeed(HttpRequest request)
        {

            List<Post> topics;

            #region -- Load database records --
            topics = _dbContext.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member).OrderByDescending(post => post.LastPostDate).Take(10).ToList();
            #endregion

            #region -- Create Feed --
            var feed = SyndicationFeed(AppUrl, STitle, SDescription, DateTime.UtcNow);

            // Add the URL that will link to your published feed when it's done
            SyndicationLink link = new SyndicationLink(new Uri(AppUrl + "/RssFeed/Active"))
                                   {
                                       RelationshipType = "self",
                                       MediaType = "text/html",
                                       Title =
                                           SSiteUrl +
                                           " Active Topics"
                                   };
            feed.Links.Add(link);

            // Add your site link
            link = new SyndicationLink(new Uri(AppUrl)) {MediaType = "text/html", Title = STitle};
            feed.Links.Add(link);
            #endregion

            #region -- Loop over active topics to add feed items --
            List<SyndicationItem> items = new List<SyndicationItem>();

            foreach (Post t in topics)
            {
                // create new entry for feed
                SyndicationItem item = new SyndicationItem();
                feed.Categories.Add(new SyndicationCategory(t.Category?.Name));
                // set the entry id to the URL for the item
                string url = string.Format(AppUrl + "Topic/Posts/{0}/?pagenum=-1#{1}", t.Id,t.LastPostReplyId);

                item.Id = url;
                
                // Add the URL for the item as a link
                link = new SyndicationLink(new Uri(AppUrl + url));
                item.Links.Add(link);
                // Fill some properties for the item
                
                item.Title = new TextSyndicationContent(t.Title);
                if (t.LastPostDate != null) item.LastUpdatedTime = t.LastPostDate.FromForumDateStr();
                if (t.Created != null) item.PublishDate = t.Created.FromForumDateStr();
                item.Authors.Add(new SyndicationPerson(t.Member.Name));
                item.Contributors.Add(new SyndicationPerson(t.LastPostAuthor?.Name));
                // Fill the item content            
                string html = BbCodeProcessor.Format(t.Content,false,false,true); 
                TextSyndicationContent content
                    = new TextSyndicationContent(html, TextSyndicationContentKind.Html);
                item.Content = content;
                item.Summary = GetSummary(html);
                item.Categories.Add(new SyndicationCategory(t.Forum.Title));
                // Finally add this item to the item collection
                items.Add(item);

            }

            feed.Items = items;

            #endregion

            return feed;

        }

        private static TextSyndicationContent GetSummary(string html)
        {
            string summaryHtml = string.Empty;

            // load our html document
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            int wordCount = 0;


            foreach (var element in htmlDoc.DocumentNode.ChildNodes)
            {
                // inner text will strip out all html, and give us plain text
                string elementText = element.InnerText;

                // we split by space to get all the words in this element
                string[] elementWords = elementText.Split(new char[] { ' ' });

                // and if we haven't used too many words ...
                if (wordCount <= 100)
                {
                    // add the *outer* HTML (which will have proper 
                    // html formatting for this fragment) to the summary
                    summaryHtml += element.OuterHtml;

                    wordCount += elementWords.Count() + 1;
                }
                else
                {
                    break;
                }
            }

            return new TextSyndicationContent(summaryHtml, TextSyndicationContentKind.Html);
        }

        public SyndicationFeed TopicFeed(int id, HttpRequest request)
        {
            
            var topic = _dbContext.Posts.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Forum)
                .Include(p => p.Member)
                .Include(p=>p.LastPostAuthor)
                .Include(p=>p.Replies).ThenInclude(r=>r.Member).SingleOrDefault(t=>t.Id == id);// Topic.WithAuthor(id);

            if (topic == null || !_memberService.AllowedForums().Contains(topic.ForumId))
            {
                return null;
            }
            #region -- Create Feed --
            var feed = SyndicationFeed(AppUrl + "/RssFeed/Topic/" + id, STitle + " : " +
                                            topic.Forum.Title + " - " +
                                            topic.Title, BbCodeProcessor.Format(topic.Content), topic.LastPostDate.FromForumDateStr());

            // Add the URL that will link to your published feed when it's done
            SyndicationLink link = new SyndicationLink(new Uri(AppUrl + "/RssFeed/Topic/" + id))
                                   {
                                       RelationshipType =
                                           "self",
                                       MediaType =
                                           "text/html",
                                       Title =
                                           topic.Title
                                   };
            feed.Links.Add(link);

            // Add your site link
            link = new SyndicationLink(new Uri(AppUrl)) {MediaType = "text/html", Title = STitle};
            feed.Links.Add(link);
            feed.Categories.Add(new SyndicationCategory(topic.Category.Name));
            #endregion
            List<SyndicationItem> items = new List<SyndicationItem>();
            #region Topic
            SyndicationItem topicItem = new SyndicationItem();
            string topicurl = string.Format("Topic/Posts/{0}/?pagenum=-1#{1}", topic.Id, topic.LastPostReplyId);
            topicItem.Id = topicurl;

            // Add the URL for the item as a link
            link = new SyndicationLink(new Uri(AppUrl + topicurl));
            topicItem.Links.Add(link);
            // Fill some properties for the item

            topicItem.Title = new TextSyndicationContent(topic.Title);
            if (topic.LastPostDate != null) topicItem.LastUpdatedTime = topic.LastPostDate.FromForumDateStr();
            if (topic.Created != null) topicItem.PublishDate = topic.Created.FromForumDateStr();
            topicItem.Authors.Add(new SyndicationPerson(topic.Member.Name));
            topicItem.Contributors.Add(new SyndicationPerson(topic.LastPostAuthor?.Name));
            // Fill the item content            
            string topichtml = BbCodeProcessor.Format(topic.Content, false, false, true);
            TextSyndicationContent topiccontent
                = new TextSyndicationContent(topichtml, TextSyndicationContentKind.Html);
            topicItem.Content = topiccontent;
            topicItem.Summary = GetSummary(topichtml);
            topicItem.Categories.Add(new SyndicationCategory(topic.Forum.Title));
            // Finally add this item to the item collection
            items.Add(topicItem);
            #endregion
            #region -- Loop over replies to add feed items --
            

            foreach (PostReply r in topic.Replies)
            {
                // create new entry for feed
                SyndicationItem item = new SyndicationItem();

                // set the entry id to the URL for the item
                string url = string.Format("Topic/Posts/{0}/?pagenum=-1#{1}", topic.Id,r.Id);

                item.Id = url;

                // Add the URL for the item as a link
                link = new SyndicationLink(new Uri(AppUrl + url));
                item.Links.Add(link);
                // Fill some properties for the item
                item.Title = new TextSyndicationContent("Reply by " + r.Member.Name);
                if (r.LastEdited != null)
                {
                    item.LastUpdatedTime = r.LastEdited.FromForumDateStr();
                }else if (r.Created != null)
                {
                    item.LastUpdatedTime = r.Created.FromForumDateStr();
                }
                if (r.Created != null) item.PublishDate = r.Created.FromForumDateStr();
                item.Authors.Add(new SyndicationPerson(r.Member.Name));
                item.Contributors.Add(new SyndicationPerson(r.Member.Email));
                // Fill the item content            
                string html = BbCodeProcessor.Format(r.Content);
                TextSyndicationContent content
                    = new TextSyndicationContent(html, TextSyndicationContentKind.Html);
                item.Content = content;
                item.Summary = GetSummary(html);
                // Finally add this item to the item collection
                items.Add(item);

            }

            feed.Items = items;

            #endregion

            return feed;
        }

        public SyndicationFeed ForumFeed(int id, HttpRequest request)
        {
            if (!_memberService.AllowedForums().Contains(id))
            {
                return null;
            }
            var forum = _dbContext.Forums.SingleOrDefault(f=>f.Id == id);// Forum.FetchForumWithCategory(id);

            #region -- Create Feed --
            var feed = SyndicationFeed(AppUrl + "/RssFeed/Forum/" + id, STitle + " - " + forum.Title, forum.Description, forum.LastPost.FromForumDateStr());

            // Add the URL that will link to your published feed when it's done
            SyndicationLink link = new SyndicationLink(new Uri(AppUrl + "/RssFeed/Forum/" + id))
                                   {
                                       RelationshipType =
                                           "self",
                                       MediaType =
                                           "text/html",
                                       Title =
                                           forum.Title +
                                           " Active Topics"
                                   };
            feed.Links.Add(link);
            feed.Categories.Add(new SyndicationCategory(forum.Category.Name));

            // Add your site link
            link = new SyndicationLink(new Uri(AppUrl)) {MediaType = "text/html", Title = STitle};
            feed.Links.Add(link);
            #endregion

            #region -- Loop over topics to add feed items --
            List<SyndicationItem> items = new List<SyndicationItem>();

            foreach (Post t in forum.Posts)
            {
                // create new entry for feed
                SyndicationItem item = new SyndicationItem();

                // set the entry id to the URL for the item
                string url = string.Format("Topic/Posts/{0}/?pagenum=1", t.Id);

                item.Id = url;

                // Add the URL for the item as a link
                link = new SyndicationLink(new Uri(AppUrl + url));
                item.Links.Add(link);
                // Fill some properties for the item
                item.Title = new TextSyndicationContent(t.Title);
                if (t.LastPostDate != null) item.LastUpdatedTime = t.LastPostDate.FromForumDateStr();
                if (t.Created != null) item.PublishDate = t.Created.FromForumDateStr();
                item.Authors.Add(new SyndicationPerson(t.Member.Name));
                item.Contributors.Add(new SyndicationPerson(t.LastPostAuthor.Name));
                // Fill the item content            
                string html = BbCodeProcessor.Format(t.Content, false, false, true);
                TextSyndicationContent content
                    = new TextSyndicationContent(html, TextSyndicationContentKind.Html);
                item.Content = content;
                item.Summary = GetSummary(html);
                // Finally add this item to the item collection
                items.Add(item);

            }

            feed.Items = items;

            #endregion

            return feed;

        }

        private SyndicationFeed SyndicationFeed(string id, string subject, string description, DateTime? lastUpdated)
        {
            SyndicationFeed feed = new SyndicationFeed
            {
                Id = id,
                Title = new TextSyndicationContent(subject),
                Description = new TextSyndicationContent(description),
                Copyright = new TextSyndicationContent(SCopyright),
                LastUpdatedTime = DateTimeOffset.UtcNow, // new DateTimeOffset(lastUpdated.GetValueOrDefault()),
                Generator = "Snitz Forums Mvc RSS Generator v 1.0",
                ImageUrl = new Uri(AppUrl + "/Content/images/logo.png")
            };
            return feed;
        }

        public SyndicationFeed ForumFeed(List<int> id, HttpRequest request)
        {
            //if (id.Count == 1)
            //{
            //    return ForumFeed(id.First(), request);
            //}

            if (!_memberService.AllowedForums().Intersect(id).Any())
            {
                return null;
            }
            

            #region -- Create Feed --
            SyndicationFeed feed = new SyndicationFeed
            {
                Id = AppUrl + "/RssFeed/Forum/" + id,
                Title = new TextSyndicationContent(STitle),
                Description = new TextSyndicationContent(SDescription),
                Copyright = new TextSyndicationContent(SCopyright),
                
                Generator = "Snitz Forums Mvc RSS Generator v 1.0",
                ImageUrl = new Uri(AppUrl + "/Content/images/logo.png")
            };

            // Add the URL that will link to your published feed when it's done
            SyndicationLink link = new SyndicationLink(new Uri(AppUrl + "/RssFeed/Forum/" + id))
            {
                RelationshipType ="self",
                MediaType ="text/html",
                Title = " Active Topics"
            };
            feed.Links.Add(link);
            

            // Add your site link
            link = new SyndicationLink(new Uri(AppUrl)) { MediaType = "text/html", Title = STitle };
            feed.Links.Add(link);
            #endregion

            #region -- Loop over topics to add feed items --
            List<SyndicationItem> items = new List<SyndicationItem>();

            //var idlink = "?";

            foreach (var i in _memberService.AllowedForums().Intersect(id))
            {
                //idlink += "id=" + i;

                var forum = _dbContext.Forums.AsNoTracking().Include(f=>f.Category).Include(f=>f.Posts).ThenInclude(p=>p.Member).SingleOrDefault(f=>f.Id == i);// Forum.FetchForumWithCategory(i);
                feed.Categories.Add(new SyndicationCategory(forum.Category.Name));
                foreach (Post t in forum.Posts)
                {
                    // create new entry for feed
                    SyndicationItem item = new SyndicationItem();

                    // set the entry id to the URL for the item
                    string url = string.Format("Topic/Posts/{0}/?pagenum=1", t.Id);

                    item.Id = url;

                    // Add the URL for the item as a link
                    link = new SyndicationLink(new Uri(AppUrl + url));
                    item.Links.Add(link);
                    // Fill some properties for the item
                    item.Title = new TextSyndicationContent(t.Title);
                    if (t.LastPostDate != null) item.LastUpdatedTime = t.LastPostDate.FromForumDateStr();
                    if (t.Created != null) item.PublishDate = t.Created.FromForumDateStr();
                    item.Authors.Add(new SyndicationPerson(t.Member?.Name));
                    item.Contributors.Add(new SyndicationPerson(t.LastPostAuthor?.Name));
                    // Fill the item content            
                    string html = BbCodeProcessor.Format(t.Content, false, false, true);
                    TextSyndicationContent content
                        = new TextSyndicationContent(html, TextSyndicationContentKind.Html);
                    item.Summary = GetSummary(html);
                    item.Content = content;

                    // Finally add this item to the item collection
                    items.Add(item);

                }
                //idlink += "&";
            }

            feed.Items = items.OrderByDescending(t=>t.LastUpdatedTime).Take(10);
            feed.LastUpdatedTime = feed.Items.Last().LastUpdatedTime;
            #endregion

            return feed;
        }
    }
}
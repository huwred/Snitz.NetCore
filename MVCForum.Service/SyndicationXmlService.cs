using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;


public class SyndicationXmlService : ISyndicationXmlService
{
    private readonly IPost _dbContext;
    private readonly ISnitzConfig _config;
    public SyndicationXmlService(IPost dbContext, ISnitzConfig config)
    {
        _dbContext = dbContext;
        _config = config;
    }

    public Rss20FeedFormatter GenerateRssXml(string feedTitle, string feedDescription)
    {
        return new Rss20FeedFormatter(GenerateSyndicationFeed(feedTitle, feedDescription));
    }

    public Atom10FeedFormatter GenerateAtomXml(string feedTitle, string feedDescription)
    {

        return new Atom10FeedFormatter(GenerateSyndicationFeed(feedTitle, feedDescription));
    }

    private SyndicationFeed GenerateSyndicationFeed(string feedTitle, string feedDescription)
    {
        
        var feed = new SyndicationFeed(feedTitle, feedDescription,
            new Uri(_config.ForumUrl),
            null,DateTime.Now)
        {
            Copyright = new TextSyndicationContent($"{DateTime.Now.Year} Copyright Huw Reddick")
        };
        
        var nodes = _dbContext.GetAllTopicsAndRelated()
                .OrderByDescending(t=>t.LastPostDate).AsEnumerable().Take(10);

        var items = new List<SyndicationItem>();

        foreach (var node in nodes)
        {
            var url = new Uri(_config.ForumUrl + "Topic/" + node.Id);
            var title = !string.IsNullOrEmpty(node.Title) ? node.Title : "node.Name";
            var description = new TextSyndicationContent(node.Content);
            var item =  new SyndicationItem(
                    title,
                    node.Content,
                    url,
                    node.Id.ToString(),
                    node.Created.FromForumDateStr());
            //item.Categories.Add(new SyndicationCategory(node.Forum.Title));

            items.Add(item);
        }
        feed.Items = items;

        return feed;
    }
}


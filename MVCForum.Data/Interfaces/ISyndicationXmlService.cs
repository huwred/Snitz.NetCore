using System.ServiceModel.Syndication;

public interface ISyndicationXmlService
{
    Atom10FeedFormatter GenerateAtomXml(string feedTitle, string feedDescription);
    Rss20FeedFormatter GenerateRssXml(string feedTitle, string feedDescription);
}

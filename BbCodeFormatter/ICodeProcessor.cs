namespace BbCodeFormatter;

public interface ICodeProcessor
{
    /// <summary>
    /// Parses all the [bbcode] tags into valid HTML
    /// </summary>
    /// <param name="data">The string to parse for BB codes</param>
    /// <param name="parseurls">If true removes anchor tags from url's</param>
    /// <param name="tooltip">If true, removes certain code from display in tooltips</param>
    /// <param name="newsfeed"></param>
    /// <returns>Valid HTML5</returns>
    string? Format(string? data,bool parseurls=true, bool tooltip = false, bool newsfeed = false);

    /// <summary>
    /// Converts data stored as html in the database
    /// into [bbcode] tags
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Message containing only [bbcode] tags</returns>
    string CleanCode(string data);

    /// <summary>
    /// Removes Html from data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    string RemoveHtmlTags(string data);

    /// <summary>
    /// Converts some [bbcode] tags into Html before saving to the Snitz database.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Classic Snitz compatible message string</returns>
    string Post(string data);

    string? Subject(string? data);

    /// <summary>
    /// Remove bbcode tags
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    string StripTags(string data);

    /// <summary>
    /// Strip content from [code] tags
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    string StripCodeContents(string data);

    List<string> GetAltTags(string data);
}
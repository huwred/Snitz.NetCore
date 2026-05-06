namespace BbCodeFormatter;

public interface ICodeProcessor
{
    /// <summary>
    /// Parses all the [bbcode] tags into valid HTML
    /// </summary>
    /// <param name="data">The string to parse for BB codes</param>
    /// <param name="parseurls">If true removes anchor tags from url's</param>
    /// <param name="tooltip">If true, removes certain code from display in tooltips</param>
    /// <param name="newsfeed">If true, uses full URLs for news feed compatibility</param>
    /// <returns>Valid HTML5</returns>
    string? Format(string? data, bool parseurls = true, bool tooltip = false, bool newsfeed = false);

    /// <summary>
    /// Converts data stored as html in the database
    /// into [bbcode] tags
    /// </summary>
    /// <param name="data">HTML content to convert</param>
    /// <returns>Message containing only [bbcode] tags</returns>
    string CleanCode(string? data);

    /// <summary>
    /// Removes Html from data
    /// </summary>
    /// <param name="data">Content with HTML tags</param>
    /// <returns>Content with HTML tags removed</returns>
    string RemoveHtmlTags(string? data);

    /// <summary>
    /// Converts some [bbcode] tags into Html before saving to the Snitz database.
    /// </summary>
    /// <param name="data">BBCode content</param>
    /// <returns>Classic Snitz compatible message string</returns>
    string Post(string? data);

    /// <summary>
    /// Process subject line content
    /// </summary>
    /// <param name="data">Subject text</param>
    /// <returns>Processed subject text</returns>
    string? Subject(string? data);

    /// <summary>
    /// Remove bbcode tags
    /// </summary>
    /// <param name="data">Content with BBCode tags</param>
    /// <returns>Plain text content</returns>
    string StripTags(string? data);

    /// <summary>
    /// Strip content from [code] tags
    /// </summary>
    /// <param name="data">Content with code blocks</param>
    /// <returns>Content with code block contents removed</returns>
    string StripCodeContents(string? data);

    /// <summary>
    /// Extract alt tags from content
    /// </summary>
    /// <param name="data">Content to parse</param>
    /// <returns>List of alt tag values, or empty list if none found</returns>
    List<string> GetAltTags(string? data);
}
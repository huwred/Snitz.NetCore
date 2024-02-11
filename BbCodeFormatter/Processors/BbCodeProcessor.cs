using System.Text.RegularExpressions;
using System.Web;
using BbCodeFormatter.Formatters;
using Microsoft.AspNetCore.Http;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;

// expanded from original code found here: http://forums.asp.net/p/1087581/1635776.aspx
// Modified to support Snitz Forums bbcode http://forum.snitz.com/forum/


namespace BbCodeFormatter.Processors
{
  /// <summary>
  /// BBCode Helper allows formatting of text usin code [tags]
  /// without the need to use html
  /// </summary>
public class BbCodeProcessor : ICodeProcessor
  {
    #region  Private Class Member Declarations
        private readonly ISnitzConfig _config;
        private readonly SnitzDbContext _dbContext;
    private bool _useFullUrl;
    private readonly List<IHtmlFormatter> _formatters;
    private readonly List<IHtmlFormatter> _postformatters;
    private readonly List<IHtmlFormatter> _cleancodeformatters;
    private readonly List<IHtmlFormatter> _urlformatters;
    private readonly List<IHtmlFormatter> _codeformatter;
    private readonly List<IHtmlFormatter> _quoteformatter;
    private readonly List<IHtmlFormatter> _tables;

    #endregion  Private Class Member Declarations

    #region  Static Constructors

    public BbCodeProcessor(ISnitzConfig config,IHttpContextAccessor httpContextAccessor,SnitzDbContext dbContext)
    {
        _dbContext = dbContext;
        _config = config;
        _formatters = new List<IHtmlFormatter>();
        _tables = new List<IHtmlFormatter>();
        _postformatters = new List<IHtmlFormatter>();
        _cleancodeformatters = new List<IHtmlFormatter>();
        _urlformatters = new List<IHtmlFormatter>();
        _codeformatter = new List<IHtmlFormatter>();
        _quoteformatter = new List<IHtmlFormatter>();

        #region UrlTags
        //lets replace any old links to TOPICS etc
        //https://forum.snitz.com/forum/(?![?])\S+(TOPIC_ID=)([0-9]+)([#0-9]+)
        _urlformatters.Add(new UrlFormatter(config.ForumUrl + @"(?![?])\S+(TOPIC_ID=)([0-9]+)(#*[0-9]*)", config.ForumUrl + "Topic/Posts/$2?pagenum=-1$3"));
        _urlformatters.Add(new UrlFormatter(config.ForumUrl + @"(?![?])\S+(FORUM_ID=)([0-9]+)", config.ForumUrl + "Forum/Posts/$2"));
        _urlformatters.Add(new UrlFormatter(config.ForumUrl + "search.asp", config.ForumUrl + "Forum/Search"));
        _urlformatters.Add(new UrlFormatter(config.ForumUrl + "faq.asp", config.ForumUrl + "Help"));
        //Parse links not in [Url] tags

        _urlformatters.Add(new UrlFormatter(@"(?<rawurl>(?<=([^'""-=\]]|^))(?:(?:https?|ftp|file)://)[^\s]*[A-Z0-9+&@#/%=~_|$])", "<a href=\"${rawurl}\" target=\"_blank\" rel=\"nofollow\" title=\"${rawurl}\">${rawurl}</a>"));
        _urlformatters.Add(new UrlFormatter(@"(?<=(\s|^))(?:(?:www|forum)\.)[^\s|\[]*[A-Z0-9+&@#/%=~_|$]", "<a href=\"http://$&\" target=\"_blank\" rel=\"nofollow\" title=\"$&\">$&</a>"));
        //parse any [url] tags without http etc
        _urlformatters.Add((new UrlFormatter(@"(?<start>\[url(?:\s*)\])(?!(https?|ftp|file|/))(?<url>(.|\n)*?)(?<end>\[/url(?:\s*)\])", "[url=\"http://${url}\"]${url}[/url]")));
        _urlformatters.Add(new UrlFormatter(@"(?<start>\[url=)(?:['""])(?!(https?|ftp|file|/))(?<url>(?:.|\n)*?)(?:['""])*(?<end>(?:\s*)\](?<content>(?:.|\n)*?)\[/url(?:\s*)\])", "${start}\"http://${url}\"${end}"));
        //parse links in [url] tags
        _urlformatters.Add(new UrlFormatter(@"(?<start>\[url(?:\s*)\])(?<url>(.|\n)*?)(?<end>\[/url(?:\s*)\])", "<a href=\"${url}\" target=\"_blank\" rel=\"nofollow\" title=\"${url}\">${url}</a>"));
        _urlformatters.Add(new UrlFormatter(@"(?<start>\[url=)("")*(?<url>(.|\n)*?)("")*(?<end>(?:\s*)\](?<content>(.|\n)*?)\[/url(?:\s*)\])", "<a href=\"${url}\" target=\"_blank\" rel=\"nofollow\" title=\"\">${content}</a>"));
        _urlformatters.Add((new UrlFormatter(@"(?<start>\[urlpreview(?:\s*)\])(?!(?:https?/))(?<url>(?:.|\n)*?)(?<end>\[/urlpreview(?:\s*)\])", config.ForumUrl + "websitethumbnailhandler.ashx?url=${url}&tw=400&th=300")));

    #endregion
        //_formatters.Add(new RegexFormatter(@"(?:\[file])([^?#\[]*/)([^.?\[]+)/([^\[]+)(?:\[/file])", "<span class=\"file-attachment\">$3 <a href=\"$1$2/$3\" rel=\"nofollow\" title=\"Download file\" data-toggle=\"tooltip\" ><i class=\"fa  fa-download fa-1_5x\"></i></a></span>"));
        _formatters.Add(new RegexFormatter(@"(?:\[file((.|\n)*?)(?:\s*)])([^?#\[]*/)([^.?\[]+)/([^\[]+)(?:\[/file])", "<span class=\"file-attachment\">$5 <a href=\"$3$4/$5\" rel=\"nofollow\" title=\"Download file\" data-toggle=\"tooltip\" ><i class=\"fa  fa-download fa-1_5x\"></i></a>$1</span>"));

        string embed = "<embed src=\"$1$2$3\" type=\"application/pdf\" class=\"object-pdf\" />";
        //embed += WebUtility.HtmlDecode(String.Format(ResourceManager.GetLocalisedString("pdfLabel", "General"), "$1$2$3"));
        //embed += "</embed>";

        _formatters.Add(new RegexFormatter(@"(?:\[pdf])([^?#\[]*/)([^.?\[]+)([^\[]+)(?:\[/pdf])", embed));
        //var postedvia = String.Format(ResourceManager.GetLocalisedString("strPostedFrom", "Api"), "$2");
        //_formatters.Add(new RegexFormatter(@"(\[APIPOST=)(.+(?=]))]", postedvia));
        //(\[APIPOST=)(.+(?=]))]
    #region Lists
    //we use a ListFormatter here to remove any newlines outside of the tags
    // \[list(?:\s*)]|\[list=(?<start>[ai1-9]*)(?:\s*)\]
    // \[/list[=ai1-9]*]
    _formatters.Add(new ListFormatter(@"\[list(?:\s*)](\r\n)*", "starttag"));
    _formatters.Add(new ListFormatter(@"\[list=(?<type>[ai1-9]{1})(?<start>[ai1-9]*)(?:\s*)\](\r\n)*", "starttag"));
    _formatters.Add(new ListFormatter(@"\[/list(?:\s*)](\r\n)*", "endtag"));
    _formatters.Add(new ListFormatter(@"\[/list(?<type>[=ai1-9]*)](\r\n)*", "endtag"));

    _formatters.Add(new RegexFormatter(@"\[\*(?:\s*)]", "<li>"));
    _formatters.Add(new RegexFormatter(@"\[/\*(?:\s*)](\r\n)*", "</li>"));
    //some stray break tags in lists fail validation so get rid of them
    _formatters.Add(new SearchReplaceFormatter("</li><br />", "</li>"));
    #endregion

    _formatters.Add(new RegexFormatter(@"(?<!t.+])(?:\r\n|\r|\n|\\n)", "<br/>"));
    _formatters.Add(new RegexFormatter(@"(?<=t.+])(?:\r\n|\r|\n|\\n)", " "));
    _formatters.Add(new SearchReplaceFormatter(@"\r", ""));

    //some users use both tags, so lets filter that out
    _codeformatter.Add(new SearchReplaceFormatter("[scrollcode][code]", "[scrollcode]"));
    _codeformatter.Add(new SearchReplaceFormatter("[/code][/scrollcode]", "[/scrollcode]"));
    _codeformatter.Add(new RegexFormatter(@"\[code(?:=""(?<class>[\S\s][^""]*)"")?(?:\s*)\]((.|\n)*?)((\[/code(?:=""\k<class>"")])|(\[/code(?:\s*)]))", "<div class=\"bbc-codetitle\" dir=\"ltr\">Code:</div><div class=\"bbc-codecontent\" dir=\"ltr\"><pre><code class=\"${class}\">$1</code></pre></div>"));
    _codeformatter.Add(new RegexFormatter(@"\[scrollcode(?:\s*)\]((.|\n)*?)\[/scrollcode(?:\s*)]", "<div class=\"bbc-codetitle\" dir=\"ltr\">Code:</div><div class=\"bbc-codecontent\" dir=\"ltr\"><pre><code>$1</code></pre></div>"));

    #region tables
    _tables.Add(new RegexFormatter(@"(\[table([^]]*)])", "<div name=\"divTable\" $2>"));
    _tables.Add(new RegexFormatter(@"(\[\/table])", "</div>"));
    _tables.Add(new RegexFormatter(@"(\[(tr)([^]]*)])", "<div name=\"divTableRow\" $3>"));
    _tables.Add(new RegexFormatter(@"(\[(td)([^]]*)])", "<div name=\"divTableCell\" $3>"));
    _tables.Add(new RegexFormatter(@"(\[(th)([^]]*)])", "<div name=\"divTableHead\" $3>"));
    _tables.Add(new RegexFormatter(@"(\[\/t([rdh])](\r\n|\r|\n)*)", "</div>"));
    _tables.Add(new RegexFormatter(@"(\[tbody([^]]*)])", "<div name=\"divTableBody\" $2>"));
    _tables.Add(new RegexFormatter(@"(\[\/tbody])", "</div>"));
    _tables.Add(new RegexFormatter(@"(\[thead([^]]*)])", "<div name=\"divTableHeader\" $2>"));
    _tables.Add(new RegexFormatter(@"(\[\/thead])", "</div>"));
    _tables.Add(new RegexFormatter(@"(\[tfoot([^]]*)])", "<div name=\"divTableFooter\" $2>"));
    _tables.Add(new RegexFormatter(@"(\[\/tfoot])", "</div>"));
    #endregion

    _formatters.Add(new LineBreaksFormatter(new[] { "html", "csharp", "code", "jscript", "sql", "vb", "php" }));
    _formatters.Add(new RegexFormatter(@"\[(red|blue|pink|brown|black|orange|violet|yellow|green|gold|white|purple|maroon|teal|limegreen|navy)(?:\s*)\]((.|\n)*?)\[/\1(?:\s*)]", "<span style=\"color:$1;\">$2</span>"));
    _formatters.Add(new RegexFormatter(@"\[h([1-6])(?:\s*)\]((.|\n)*?)\[/h[1-6](?:\s*)\]", "<h$1>$2</h$1>"));
  
    _formatters.Add(new RegexFormatter(@"\[i(?:\s*)\]((.|\n)*?)\[/i(?:\s*)\]", "<em>$1</em>"));
    _formatters.Add(new RegexFormatter(@"(?:\[i class=""([^]\[]+)""])(?:\[/i])", "<i class=\"$1\"></i>"));
    _formatters.Add(new RegexFormatter(@"\[b(?:\s*)\]((.|\n)*?)\[/b(?:\s*)\]", "<b>$1</b>"));
    _formatters.Add(new RegexFormatter(@"\[s(?:\s*)\]((.|\n)*?)\[/s(?:\s*)\]", "<s>$1</s>"));
    _formatters.Add(new RegexFormatter(@"\[u(?:\s*)\]((.|\n)*?)\[/u(?:\s*)\]", "<u>$1</u>"));
    _formatters.Add(new RegexFormatter(@"\[(su[bp])\]((.|\n)*?)\[/\1]", "<$1>$2</$1>"));

    _formatters.Add(new RegexFormatter(@"\[br(?:\s*)\]", "<br/>"));
    _formatters.Add(new RegexFormatter(@"\[left(?:\s*)\]((.|\n)*?)\[/left(?:\s*)]", "<span class=\"pull-left flip\">$1</span>"));
    _formatters.Add(new RegexFormatter(@"\[center(?:\s*)\]((.|\n)*?)\[/center(?:\s*)]", "<span class=\"align-center\">$1</span>"));
    _formatters.Add(new RegexFormatter(@"\[right(?:\s*)\]((.|\n)*?)\[/right(?:\s*)]", "<span class=\"pull-right flip\">$1</span>"));

    _quoteformatter.Add(new RegexFormatter(@"\[/quote=((.|\n)*?)(?:\s*)\]", "<cite>$1</cite></blockquote>"));
    _quoteformatter.Add(new RegexFormatter(@"\[/quote(?:\s*)\]", "</blockquote>"));
    _quoteformatter.Add(new RegexFormatter(@"\[quote=((.|\n)*?)(?:\s*)\]", "<blockquote class=\"newquote\" ><cite>$1</cite>"));
    _quoteformatter.Add(new RegexFormatter(@"\[quote(?:\s*)\]", "<blockquote class=\"newquote\" >"));

    if (_config.GetValue("STRIMGINPOSTS") == "1")
    {
        _formatters.Add(new RegexFormatter(@"(?<!"")\[img(?:\s*)\]((.|\n)*?)\[/img(?:\s*)\](?!"")", "<img loading=\"lazy\" src=\"$1\" border=\"0\" alt=\"\" />"));
        _formatters.Add(new RegexFormatter(@"\[img align=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/img(?:\s*)\]", "<img src=\"$3\" border=\"0\" loading=\"lazy\" class=\"img-$1\" alt=\"\" />"));
        _formatters.Add(new RegexFormatter(@"\[img=((.|\n)*?)((?:[x])((.|\n)*?))*(?:\s*)\]((.|\n)*?)\[/img(?:\s*)\]", "<img loading=\"lazy\" width=\"$1\" height=\"$4\" src=\"$6\" border=\"0\" alt=\"\" />"));
        _formatters.Add(new RegexFormatter(@"\[img=((.|\n)*?)\]((.|\n)*?)\[/img=((.|\n)*?)\]", "<img src=\"$3\" border=\"0\" loading=\"lazy\" class=\"img-$1\" alt=\"\" />"));
        //Config.ForumUrl

    }

        _formatters.Add(new RegexFormatter(@"\[color=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/color(?:\s*)\]", "<span style=\"color:$1;\">$3</span>"));
    _formatters.Add(new RegexFormatter(@"\[highlight(?:\s*)\]((.|\n)*?)\[/highlight(?:\s*)]", "<span class=\"bbc-highlight\">$1</span>"));
    _formatters.Add(new RegexFormatter(@"\[spoiler(?:\s*)\]((.|\n)*?)\[/spoiler(?:\s*)]", "<span class=\"bbc-spoiler\"><span class=\"bbc-spoiler-head\">Reveal hidden content</span><span class=\"bbc-spoiler-content\">$1</span></span>"));
    _formatters.Add(new RegexFormatter(@"\[indent(?:\s*)\]((.|\n)*?)\[/indent(?:\s*)]", "<div class=\"bbc-indent\">$1</div>"));
    _formatters.Add(new RegexFormatter(@"\[hr(?:\s*)\]", "<hr />"));
    _formatters.Add(new RegexFormatter(@"\[rule=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/rule(?:\s*)\]", "<div style=\"height: 0pt; border-top: 1px solid $3; margin: auto; width: $1;\"></div>"));
    _formatters.Add(new RegexFormatter(@"\[small(?:\s*)\]((.|\n)*?)\[/small(?:\s*)]", "<small>$1</small>"));
    _formatters.Add(new RegexFormatter(@"\[size=+((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/size(?:\s*)\]", "<span class=\"text-size-$1\">$3</span>"));
    _formatters.Add(new RegexFormatter(@"\[size=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/size=((.|\n)*?)(?:\s*)\]", "<span class=\"text-size-$1\" >$3</span>"));
    _formatters.Add(new RegexFormatter(@"\[font=(.*?)\]([^\[]*)?\[/font=(.*?)\]", "<span style=\"font-family:$1;\">$2</span>"));
    _formatters.Add(new RegexFormatter(@"\[align=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/align(?:\s*)\]", "<span style=\"text-align:$1;\">$3</span>"));
    _formatters.Add(new RegexFormatter(@"\[float=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/float(?:\s*)\]", "<span style=\"float:$1;\" class=\"clearfix\">$3</div>"));
    _formatters.Add(new RegexFormatter(@"\[rtlf\]((.|\n)*?)\[/rtlf]", "<span dir=\"rtl\" class=\"pull-right\">$1</span>"));
    _formatters.Add(new RegexFormatter(@"\[ltrf\]((.|\n)*?)\[/ltrf]", "<span dir=\"ltr\" class=\"pull-left\">$1</span>"));
    _formatters.Add(new RegexFormatter(@"\[rtl\]((.|\n)*?)\[/rtl]", "<span dir=\"rtl\" >$1</span>"));
    _formatters.Add(new RegexFormatter(@"\[ltr\]((.|\n)*?)\[/ltr]", "<span dir=\"ltr\" >$1</span>"));

    _formatters.Add(new RegexFormatter(@"&(?![A-Za-z]+;|#[0-9]+;)", "&amp;", false));
    if(_config.GetIntValue("STRICONS")==1)
        _formatters.Add(new EmoticonFormatter(config,httpContextAccessor));
  

    #region PostFormat
    //_postformatters.Add(new RegexFormatter(@"\[quote(?:\s*)\]", "<blockquote id=\"quote\"><font size=\"" + ClassicConfig.FooterFontsize + "\" face=\"" + ClassicConfig.DefaultFontFace + "\" id=\"quote\">quote:<hr height=\"1\" noshade id=\"quote\">"));
    //_postformatters.Add(new RegexFormatter(@"\[/quote(?:\s*)\]", "<hr height=\"1\" noshade id=\"quote\"></font id=\"quote\"></blockquote id=\"quote\">"));
    _postformatters.Add(new RegexFormatter(@"\[b(?:\s*)\]((.|\n)*?)\[/b(?:\s*)\]", "<b>$1</b>"));
    _postformatters.Add(new RegexFormatter(@"\[i(?:\s*)\]((.|\n)*?)\[/i(?:\s*)\]", "<i>$1</i>"));
    _postformatters.Add(new RegexFormatter(@"\[s(?:\s*)\]((.|\n)*?)\[/s(?:\s*)\]", "<s>$1</s>"));
    _postformatters.Add(new RegexFormatter(@"\[u(?:\s*)\]((.|\n)*?)\[/u(?:\s*)\]", "<u>$1</u>"));
    _postformatters.Add(new RegexFormatter(@"\[(red|blue|pink|brown|black|orange|violet|yellow|green|gold|white|purple)(?:\s*)\]((.|\n)*?)\[/\1(?:\s*)]", "<font color=\"$1\">$2</font id=\"$1\">"));
    _postformatters.Add(new RegexFormatter(@"\[font=(.*?)\]([^\[]*)?\[/font=(.*?)\]", "<font face=\"$1\">$2</font id=\"$1\">"));
    _postformatters.Add(new RegexFormatter(@"\[h([1-6])(?:\s*)\]((.|\n)*?)\[/h[1-6](?:\s*)\]", "<h$1>$2</h$1>"));
    _postformatters.Add(new RegexFormatter(@"\[size=((.|\n)*?)(?:\s*)\]((.|\n)*?)\[/size=((.|\n)*?)(?:\s*)\]", "<font size=\"$1\">$3</font id=\"size$1\">"));
    _postformatters.Add(new SearchReplaceFormatter("[hr]", "<hr noshade size=\"1\">"));
    if (_config.GetValue("STRBADWORDFILTER") == "1")
    {
        _postformatters.Add(new BadWordFilter(_dbContext));
    }

    #endregion

    #region CleanCode
    _cleancodeformatters.Add(new RegexFormatter(@"(<table([^>]*)>)", "[table$2]"));

    _cleancodeformatters.Add(new RegexFormatter(@"(<\/table>)", "[/table]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<t([rdh])([^>]*)>)", "[t$2 $3]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<\/t([rdh])>)", "[/t$2]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<tbody([^>]*)>)", "[tbody$2]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<\/tbody>)", "[/tbody]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<thead([^>]*)>)", "[thead$2]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<\/thead>)", "[/thead]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<tfoot([^>]*)>)", "[tfoot$2]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<\/tfoot>)", "[/tfoot]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<br />", "\r\n"));
        //<blockquote id="quote"><(font) (\b[^>]*)>(.*?)</font></blockquote>
        _cleancodeformatters.Add(new RegexFormatter(@"<blockquote id=""quote""><(font) (\b[^>]*)>(.*?)</font></blockquote>", "[quote]$3[/quote]"));

        _cleancodeformatters.Add(new RegexFormatter(@"(<blockquote id=(?:\\""|"")quote(?:\\""|"")>.*quote:<hr (?:id|height)=(?:\\""|"")[a-z0-9]+(?:\\""|"") noshade (?:id|height)=(?:\\""|"")[a-z0-9]+(?:\\""|"")>)", "[quote]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<hr (?:id|height)=(?:\\""|"")[a-z0-9]+(?:\\""|"") noshade (?:id|height)=(?:\\""|"")[a-z0-9]+(?:\\""|"")></font id=(?:\\""|"")quote(?:\\""|"")></blockquote id=(?:\\""|"")quote(?:\\""|"")>)", "[/quote]"));
    _cleancodeformatters.Add(new RegexFormatter(@"(<pre id=""code""><font.*"">)(.*)(</font id=""code""></pre id=""code"">)", "[code]$2[/code]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<em(?:\s*)\>((.|\n)*?)\</em(?:\s*)\>", "<i>$1</i>"));      
    _cleancodeformatters.Add(new RegexFormatter(@"<a href=['""]?((.|\n)*?)['""]?(?:\s*)title=['""]?((.|\n)*?)['""]?(?:\s*)>((.|\n)*?)</a>", "[url=\"$5\"]$3[/url]"));
    _cleancodeformatters.Add(new RegexFormatter(@"\<(b)(?:\s*)\>((.|\n)*?)\</\1(?:\s*)\>", "[$1]$2[/$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"\<(i)(?:\s*)\>((.|\n)*?)\</\1(?:\s*)\>", "[$1]$2[/$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"\<(s)(?:\s*)\>((.|\n)*?)\</\1(?:\s*)\>", "[$1]$2[/$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"\<(u)(?:\s*)\>((.|\n)*?)\</\1(?:\s*)\>", "[$1]$2[/$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<font face=['""]?((.|\n)*?)['""]?(?:\s*)\>((.|\n)*?)\</font id=['""]?\1['""]?(?:\s*)\>", "[font=$1]$3[/font=$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<font color=['""]?((.|\n)*?)['""]?(?:\s*)\>((.|\n)*?)\</font id=['""]?\1['""]?(?:\s*)\>", "[$1]$3[/$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<font size=['""]?((.|\n)*?)['""]?(?:\s*)\>((.|\n)*?)\</font id=['""]?size\1['""]?(?:\s*)\>", "[size=$1]$3[/size=$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<h([1-6])(?:\s*)\>((.|\n)*?)\</h\1(?:\s*)\>", "[h$1]$2[/h$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<ul(?:\s*)\>((.|\n)*?)\</ul(?:\s*)\>", "[list]$1[/list]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<ol type=(?:\\""|""|')?(1|a)(?:\\""|""|')?(?:\s*)\>((.|\n)*?)\</ol id=(?:\\""|""|')?\1(?:\\""|""|')?(?:\s*)\>", "[list=$1]$2[/list=$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<li(?:\s*)\>((.|\n)*?)\</li(?:\s*)\>", "[*]$1[/*]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<div id=\"scrollcode\" class=\"scrollcode\"><pre>", "[scrollcode]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("</pre></div id=\"scrollcode\">", "[/scrollcode]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_angry.gif border=0 align=middle>", "[:(!]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_blackeye.gif border=0 align=middle>", "[B)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_dead.gif border=0 align=middle>", "[xx(]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_dead.gif border=0 align=middle>", "[XX(]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_shock.gif border=0 align=middle>", "[:O]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_shock.gif border=0 align=middle>", "[:o]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_shock.gif border=0 align=middle>", "[:0]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_blush.gif border=0 align=middle>", "[:I]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_sad.gif border=0 align=middle>", "[:(]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_shy.gif border=0 align=middle>", "[8)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile.gif border=0 align=middle>", "[:)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_evil.gif border=0 align=middle>", "[}:)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_big.gif border=0 align=middle>", "[:D]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_cool.gif border=0 align=middle>", "[8D]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_sleepy.gif border=0 align=middle>", "[|)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_clown.gif border=0 align=middle>", "[:o)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_clown.gif border=0 align=middle>", "[:O)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_clown.gif border=0 align=middle>", "[:0)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_tongue.gif border=0 align=middle>", "[:P]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_tongue.gif border=0 align=middle>", "[:p]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_wink.gif border=0 align=middle>", "[;)]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_8ball.gif border=0 align=middle>", "[8]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_question.gif border=0 align=middle>", "[?]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_approve.gif border=0 align=middle>", "[^]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_disapprove.gif border=0 align=middle>", "[V]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_disapprove.gif border=0 align=middle>", "[v]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_dissapprove.gif border=0 align=middle>", "[V]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_dissapprove.gif border=0 align=middle>", "[v]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_kisses.gif border=0 align=middle>", "[:X]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<img src=icon_smile_kisses.gif border=0 align=middle>", "[:x]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<img src=['""]?((.|\n)*?)['""]?(?:\s*)(align(=right|=left))? border=0\>", "[img$4]$1[/img$4]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<div align=['""]?((.|\n)*?)['""]?(?:\s*)\>((.|\n)*?)\</div id=['""]?\1['""]?(?:\s*)\>", "[$1]$3[/$1]"));
    _cleancodeformatters.Add(new RegexFormatter(@"<hr \b[^>]*>", "[hr]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("<center>", "[center]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("</center>", "[/center]"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("&#39;","'"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("&#187;", "»"));
    _cleancodeformatters.Add(new SearchReplaceFormatter("&#171;", "«"));
    #endregion

    }

    #endregion  Static Constructors

    #region  Public Class Methods

    /// <summary>
    /// Parses all the [bbcode] tags into valid HTML
    /// </summary>
    /// <param name="data">The string to parse for BB codes</param>
    /// <param name="parseurls">If true removes anchor tags from url's</param>
    /// <param name="tooltip">If true, removes certain code from display in tooltips</param>
    /// <param name="newsfeed"></param>
    /// <returns>Valid HTML5</returns>
    public string? Format(string? data,bool parseurls=true, bool tooltip = false, bool newsfeed = false)
    {
        if (data == null)
        {
            return data;
        }
        _useFullUrl = newsfeed;
    if (_config.GetValue("STRALLOWFORUMCODE") != "1")
        return data;

    if (String.IsNullOrWhiteSpace(data))
        return data;
    //classic forum stores some codes as html, so lets' parse it back into [bbcode]
    data = CleanCode(data);
    
    //parse any [noparse] tags
    data = NoParse(data);

    //now we can turn bbtags into html5
    if (parseurls)
    {
        foreach (IHtmlFormatter urlformatter in _urlformatters)
        {
            data = urlformatter.Format(data);
        }
    }
    if (!tooltip)
    {
            foreach (IHtmlFormatter formatter in _quoteformatter)
            {
                data = formatter.Format(data);
            }
            //_quoteformatter
            foreach (IHtmlFormatter formatter in _codeformatter)
            {
                data = formatter.Format(data);
            }


    }
    else
    {
        var removecode = new List<IHtmlFormatter>
        {
            new SearchReplaceFormatter("[scrollcode][code]", "[scrollcode]"),
            new SearchReplaceFormatter("[/code][/scrollcode]", "[/scrollcode]"),
            new RegexFormatter(@"\[code(?:=""([\S\s]*)"")*(?:\s*)\]((.|\n)*?)\[/code(?:\s*)]", ""),
            new RegexFormatter(@"\[scrollcode(?:\s*)\]((.|\n)*?)\[/scrollcode(?:\s*)]", "")
        };
        //some users use both tags, so lets filter that out
        foreach (IHtmlFormatter formatter in removecode)
        {
            data = formatter.Format(data);
        }
    }
    foreach (IHtmlFormatter formatter in _tables)
    {
        data = formatter.Format(data);
    }
    foreach (IHtmlFormatter formatter in _formatters)
    {
        data = formatter.Format(data);
    }

    if (_config.GetIntValue("STRPHOTOALBUM") == 1 || _config.TableExists("FORUM_IMAGES"))
    {
        if (_useFullUrl)
        {
            data = Regex.Replace(data, @"\[image=(?<id>\d+)]", "<a href=\"" + _config.ForumUrl + "PhotoAlbum/GetPhoto/${id}\" class=\"view-image\" target=\"_blank\"><img loading=\"lazy\" src=\"" + _config.ForumUrl + "PhotoAlbum/Thumbnail/${id}\" border=\"0\" title=\"${id}\" /></a>", RegexOptions.IgnoreCase);
        }
        else
        {
            data = Regex.Replace(data, @"\[image=(?<id>\d+)]", "<a href=\"" + _config.RootFolder + "/PhotoAlbum/GetPhoto/${id}\" class=\"view-image\" target=\"_blank\"><img loading=\"lazy\" src=\"" + _config.RootFolder + "/PhotoAlbum/Thumbnail/${id}\" border=\"0\" /></a>", RegexOptions.IgnoreCase);
        }

        var imgf = new AlbumImageFormatter(_config,@"\[cimage=(?<id>\d+)]");
        data = imgf.Format(data);
    }

    if (_config.TableExists("FORUM_BBCODE"))
    {
        data = CustomCode(data);
    }




    if (_config.ContentFolder != "Content")
    {
        data = Regex.Replace(data,"/Content/Members", "/ProtectedContent/Members",RegexOptions.IgnoreCase);
        data = Regex.Replace(data, "/Content/Avatar", "/ProtectedContent/Avatar", RegexOptions.IgnoreCase);
        if (_config.GetIntValue("INTPROTECTPHOTO") == 1)
        {
            data = Regex.Replace(data, "/Content/PhotoAlbum", "/ProtectedContent/PhotoAlbum", RegexOptions.IgnoreCase);
        }
    }


    return data.Replace("'", "&#39;");
    }

    /// <summary>
    /// Converts data stored as html in the database
    /// into [bbcode] tags
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Message containing only [bbcode] tags</returns>
    public string CleanCode(string data)
    {
        if (String.IsNullOrWhiteSpace(data))
            return data;

        //turn any stored html back into bbcode
        foreach (IHtmlFormatter formatter in _cleancodeformatters)
            data = formatter.Format(data);

        return data;
    }
    
    /// <summary>
    /// Removes Html from data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public string RemoveHtmlTags(string data)
    {
        if (String.IsNullOrWhiteSpace(data))
            return data;
        var removecodeformatters = new List<IHtmlFormatter>
        {
            new RegexFormatter(@"(<table([^>]*)>)", ""),
            new RegexFormatter(@"(<\/table>)", ""),
            new RegexFormatter(@"(<t([rdh])([^>]*)>)", ""),
            new RegexFormatter(@"(<\/t([rdh])>)", ""),
            new RegexFormatter(@"(<tbody([^>]*)>)", ""),
            new RegexFormatter(@"(<\/tbody>)", ""),
            new RegexFormatter(@"(<thead([^>]*)>)", ""),
            new RegexFormatter(@"(<\/thead>)", ""),
            new RegexFormatter(@"(<tfoot([^>]*)>)", ""),
            new RegexFormatter(@"(<\/tfoot>)", "")
        };
        //remove any stored html 
        foreach (IHtmlFormatter formatter in removecodeformatters)
            data = formatter.Format(data);

        return data;
    }

    /// <summary>
    /// Converts some [bbcode] tags into Html before saving to the Snitz database.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Classic Snitz compatible message string</returns>
    public string Post(string data)
    {
        if (String.IsNullOrWhiteSpace(data))
            return data;
        data = HttpUtility.HtmlEncode(data).Replace("&quot;","\"").Replace("&amp;", "&");
        
        //parse the tags that Snitz stores as Html in the database
        foreach (IHtmlFormatter formatter in _postformatters)
            data = formatter.Format(data);

        return data;
    }
    public string? Subject(string? data)
    {
        if (String.IsNullOrWhiteSpace(data))
            return data;
        data = data.Replace("&quot;","\"").Replace("&amp;", "&");
        
        //parse the tags that Snitz stores as Html in the database
        foreach (IHtmlFormatter formatter in _postformatters)
            data = formatter.Format(data);

        return data;
    }
    /// <summary>
    /// Remove bbcode tags
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public string StripTags(string data)
    {
        // [\[\<][^\]|\>]*[\]\>]
        if (String.IsNullOrWhiteSpace(data))
            return data;
        const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline;
        Regex regex = new Regex(@"[\[\<][^\]|\>]*[\]\>]", options);

        data = regex.Replace(data, "");

        return data;
    }

    /// <summary>
    /// Strip content from [code] tags
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public string StripCodeContents(string data)
    {
        var removecode = new List<IHtmlFormatter>
        {
            new SearchReplaceFormatter("[scrollcode][code]", "[scrollcode]"),
            new SearchReplaceFormatter("[/code][/scrollcode]", "[/scrollcode]"),
            new RegexFormatter(@"\[code(?:=""([\S\s]*)"")*(?:\s*)\]((.|\n)*?)\[/code(?:\s*)]", ""),
            new RegexFormatter(@"\[scrollcode(?:\s*)\]((.|\n)*?)\[/scrollcode(?:\s*)]", "")
        };
        //some users use both tags, so lets filter that out
        foreach (IHtmlFormatter formatter in removecode)
        {
            data = formatter.Format(data);
        }
        return data;
    }
    #endregion  Public Class Methods

    /// <summary>
    /// modifies [bbcode] tags that we don't want to parse into html
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string NoParse(string data)
    {
        if (String.IsNullOrWhiteSpace(data))
            return data;
        const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline;
        Regex regex = new Regex(@"(\[noparse])(.*?)(\[/noparse])", options);

        foreach (Match m in regex.Matches(data))
        {
            var content = CleanCode(m.Groups[2].Value).Replace(":/", "&#58;&#47;");
            var repl = Regex.Replace(content, @"(\[)", delegate(Match match)
            {
                string v = match.ToString();
                //replace the opening [ with html code to prevent parsing
                return "&#91;" + v.Substring(1);
            });
            data = Regex.Replace(data, Regex.Escape(m.Groups[0].Value), repl);
        }

        return data;

    }

    /// <summary>
    /// Passes the string data through any custom defined format tags
    /// </summary>
    /// <param name="data">The string to parse for Custom Tags</param>
    /// <returns></returns>
    private string CustomCode(string data)
    {
        if (String.IsNullOrWhiteSpace(data))
            return data;
        var formatters = new List<IHtmlFormatter>();

        //foreach (CustomBBcode bcode in CustomBBcode.All())
        //{
        //    if(bcode.Active)
        //        formatters.Add(new RegexFormatter(bcode.Pattern, bcode.Replace));
        //}
        foreach (IHtmlFormatter formatter in formatters)
        {
            data = formatter.Format(data);
        }
        return data;
    }


  }
}

using System.Text.RegularExpressions;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;

namespace BbCodeFormatter.Formatters
{
    public class AlbumImageFormatter : IHtmlFormatter
    {
        #region  Private Member Declarations  

        private Regex _regex;
        private string _replace;
        private string _pattern;

        #endregion  Private Member Declarations  

        public AlbumImageFormatter(ISnitzConfig config,string pattern)
        {
            RegexOptions options = RegexOptions.Compiled;
            options |= RegexOptions.Multiline;
            options |= RegexOptions.IgnoreCase;

            _replace =
                "<figure><a href=\"" + config.ForumUrl + "PhotoAlbum/GetPhoto/${id}\" class=\"view-image\" target=\"_blank\"><img loading=\"lazy\" src=\"" + config.ForumUrl + "PhotoAlbum/Thumbnail/${id}\" border=\"0\" alt=\"\" /></a>" +
                 "<figcaption class=\"fig-caption\" data-id=\"${id}\"></figcaption> " +
                "</figure> ";
            _pattern = pattern;
            _regex = new Regex(pattern, options);
        }

        public string Format(string data)
        {
            if (_regex.IsMatch(data))
            {
                return Regex.Replace(data, _pattern, _replace);
            }
            return data;
        }

    }
}

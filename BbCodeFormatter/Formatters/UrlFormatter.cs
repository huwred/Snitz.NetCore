using System.Text.RegularExpressions;

namespace BbCodeFormatter.Formatters
{
    internal class UrlFormatter : IHtmlFormatter
    {
		#region  Private Member Declarations  

        private readonly Regex _regex;
        private readonly string _replace;
        private readonly string _pattern;

		#endregion  Private Member Declarations  

		#region  Public Constructors  

        public UrlFormatter(string pattern, string replace, bool ignoreCase = true)
        {
            RegexOptions options = RegexOptions.Compiled;
            options |= RegexOptions.Multiline;
            if (ignoreCase)
                options |= RegexOptions.IgnoreCase;
        
            _replace = replace;
            _pattern = pattern;
            _regex = new Regex(pattern, options);
        }

		#endregion  Public Constructors  

		#region  Public Virtual Methods  

        public string Format(string data)
        {
            if (_regex.IsMatch(data))
            {
                data = Regex.Replace(data, _pattern, ParseUrlCallback);

                var test =  _regex.Replace(data, _replace);
                var test2  = Regex.Replace(test, @"href=""(?<url>[^\s]*)""", ParseUrlCallback2);
                return test2;
            }
            return data;
        }

        private string ParseUrlCallback2(Match match)
        {
            try
            {
                Uri siteUri = new Uri(match.Groups["url"].Value);
                var res = "href=\"" + siteUri.AbsoluteUri + "\"";

                return String.IsNullOrWhiteSpace(res) ? match.Value : res;
            }
            catch (Exception)
            {
                return match.Value;
            }

        }

        /// <summary>
        /// Replace callback allows parsing of the url parts
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        static string ParseUrlCallback(Match match)
        {
            var res = 
                match.Groups["start"].Value +
                match.Groups["url"].Value.Replace("\\", "/").Replace(@"\", "/") + 
                match.Groups["end"].Value;
            return String.IsNullOrWhiteSpace(res) ? match.Value : res;
        }
		#endregion  Public Virtual Methods  

		#region  Protected Properties  

        protected Regex Regex
        { get { return _regex; } }

        protected string Replace
        { get { return _replace; } }

		#endregion  Protected Properties  
    }
}

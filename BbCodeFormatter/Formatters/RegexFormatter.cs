using System.Text.RegularExpressions;

namespace BbCodeFormatter.Formatters
{
  internal class RegexFormatter : IHtmlFormatter
  {
		#region  Private Member Declarations  

    private readonly Regex _regex;
    private readonly string _replace;

		#endregion  Private Member Declarations  

		#region  Public Constructors  

    public RegexFormatter(string pattern, string replace, bool ignoreCase = true)
    {
        RegexOptions options = RegexOptions.Multiline | RegexOptions.Singleline;

        if (ignoreCase)
            options |= RegexOptions.IgnoreCase;

        _replace = replace;
        _regex = new Regex(pattern, options);
    }

		#endregion  Public Constructors  

		#region  Public Virtual Methods  

    public string Format(string data)
    {
      return _regex.Replace(data, _replace);
    }

		#endregion  Public Virtual Methods  

		#region  Protected Properties  

    protected Regex Regex
    { get { return _regex; } }

    protected string Replace
    { get { return _replace; } }

		#endregion  Protected Properties  
  }

    internal class ListFormatter : IHtmlFormatter
    {
        #region  Private Member Declarations  

        private readonly Regex _regex;

        private readonly string _replace;

        #endregion  Private Member Declarations  

        #region  Public Constructors  

        public ListFormatter(string pattern, string replace, bool ignoreCase = true)
        {
            RegexOptions options = RegexOptions.Multiline;

            if (ignoreCase)
                options |= RegexOptions.IgnoreCase;

            _replace = replace;
            _regex = new Regex(pattern, options);

        }

        #endregion  Public Constructors  

        #region  Public Virtual Methods  

        public string Format(string data)
        {
            string replace = "";
            if (_regex.IsMatch(data))
            {
                foreach (Match m in _regex.Matches(data))
                {
                    if (_replace == "endtag")
                    {
                        replace = @"</ul>";
                        if (m.Groups["type"].Length > 0)
                        {
                            replace = @"</ol>";
                        }
                    }
                    if (_replace == "starttag")
                    {
                        if (m.Groups.Count > 1)
                        {
                            string t = m.Groups["type"].Value;
                            string s = m.Groups["start"].Value;
                            if (t.Length == 0)
                            {
                                replace = "<ul class=\"bbc-list\">";
                            }
                            else if (s.Length == 1)
                            {
                                replace = "<ol start=\"${start}\" class=\"bbc-list\" type=\"${type}\">";

                            }
                            else
                            {
                                replace = "<ol start=\"1\" class=\"bbc-list\" type=\"${type}\">";
                            }

                        }
                        else
                        {
                            replace = "<ul class=\"bbc-list\">";
                        }
                    }
                }
                return _regex.Replace(data, replace);
            }


            return data;

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

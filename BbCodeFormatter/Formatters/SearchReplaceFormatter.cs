
using System.Text.RegularExpressions;

namespace BbCodeFormatter.Formatters
{
  internal class SearchReplaceFormatter : IHtmlFormatter
  {
		#region  Private Member Declarations  

    private readonly string _pattern;
    private readonly string _replace;

		#endregion  Private Member Declarations  

		#region  Public Constructors  

    public SearchReplaceFormatter(string pattern, string replace)
    {
      _pattern = pattern;
      _replace = replace;
    }

		#endregion  Public Constructors  

		#region  Public Methods  

    public string Format(string data)
    {
        if (String.IsNullOrWhiteSpace(data))
            return data;
        try
        {
            if (Regex.Match(data, _pattern).Success)
            {
                return data.Replace(_pattern, _replace);
            }
        }
        catch (Exception)
        {

        }

        return data;
    }

		#endregion  Public Methods  
  }
}

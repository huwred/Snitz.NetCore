using Flurl;
using SnitzCore.Data.Interfaces;
using System.Net;
using System.Xml.Linq;

namespace BbCodeFormatter.Formatters
{
  internal class EmoticonFormatter : IHtmlFormatter
  {
	#region  Private Member Declarations  
        private readonly List<IHtmlFormatter>? _formatters;
    #endregion  Private Member Declarations  

	#region  Public Constructors  
      /// <summary>
      /// replace emoticon tags with images
      /// </summary>
      public EmoticonFormatter(ISnitzConfig config)
      {
        var urlpath = Url.Combine(config.ForumUrl, "/images/emoticon/");
        if (_formatters == null)
        {
            _formatters = new List<IHtmlFormatter>();
                XElement emoticons = XElement.Load(@"App_Data\emoticons.xml");

                List<XElement> childList =
                    (from el in emoticons.Elements()
                    select el).ToList();

                foreach (XElement el in childList)
                {
                    string emoticon = el.Attribute("code")!.Value;
                    string image = el.Attribute("image")!.Value;
                    string title = el.Attribute("name")!.Value;
                    string alt = el.Attribute("name")!.Value;

                    string link =
                        $"<img title=\"{title}\" data-toggle=\"tooltip\" src=\"{urlpath}{image}\" alt=\"{alt}\" class=\"emote\" loading=\"lazy\" />";

                    _formatters.Add(new SearchReplaceFormatter(WebUtility.HtmlEncode(emoticon), link));
                }                
        }


      }

		#endregion  Public Constructors  

		#region  Public Methods  

    public string Format(string data)
    {
        var originalData = data;
        try
        {
            if (_formatters != null && _formatters.Count > 0)
            {
                foreach (IHtmlFormatter formatter in _formatters.ToList())
                { 
                        data = formatter.Format(data); 
                }
            }
        }
        catch (Exception e)
        {

            throw;
        }

        return data;
    }

		#endregion  Public Methods  
  }
}

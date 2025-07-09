using System.Net;
using System.Xml.Linq;
using Flurl;
using SnitzCore.Data.Interfaces;

namespace BbCodeFormatter.Formatters
{
  internal class EmoticonFormatter : IHtmlFormatter
  {
	#region  Private Member Declarations  
        static List<IHtmlFormatter>? _formatters;
    #endregion  Private Member Declarations  

	#region  Public Constructors  
      /// <summary>
      /// replace emoticon tags with images
      /// </summary>
      public EmoticonFormatter(ISnitzConfig config)
      {
          //string urlpath = config.ForumUrl(); // HttpContext.Current.Request.ApplicationPath == "/" ? Config.ForumUrl : HttpContext.Current.Request.ApplicationPath;
          var urlpath = Url.Combine(config.ForumUrl, "/images/emoticon/");
         _formatters = new List<IHtmlFormatter>();
         //string appdata = Path.Combine(httpContextAccessor.HttpContext!.Request.PathBase, @"App_Data\emoticons.xml");
         XElement emoticons = XElement.Load(@"App_Data\emoticons.xml");

          IEnumerable<XElement> childList =
                from el in emoticons.Elements()
                select el;
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

		#endregion  Public Constructors  

		#region  Public Methods  

    public string Format(string data)
    {
        if(_formatters != null && _formatters.Count > 0)
        {
            foreach (IHtmlFormatter formatter in _formatters)
            { 
                    data = formatter.Format(data); 
            }

        }

        return data;
    }

		#endregion  Public Methods  
  }
}

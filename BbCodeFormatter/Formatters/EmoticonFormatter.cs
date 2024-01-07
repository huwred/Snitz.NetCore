using System.Net;
using System.Xml.Linq;
using Flurl;
using Microsoft.AspNetCore.Http;
using SnitzCore.Data;
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
      public EmoticonFormatter(ISnitzConfig config,IHttpContextAccessor httpContextAccessor)
      {
          //string urlpath = config.ForumUrl(); // HttpContext.Current.Request.ApplicationPath == "/" ? Config.ForumUrl : HttpContext.Current.Request.ApplicationPath;
          var urlpath = Url.Combine(config.ForumUrl, "/images/emoticon/");
         _formatters = new List<IHtmlFormatter>();
         string appdata = Path.Combine(httpContextAccessor.HttpContext!.Request.PathBase, @"App_Data\emoticons.xml");
         XElement emoticons = XElement.Load(appdata);

          IEnumerable<XElement> childList =
                from el in emoticons.Elements()
                select el;
          foreach (XElement el in childList)
          {
              string emoticon = el.Attribute("code")!.Value;
              string image = el.Attribute("image")!.Value;
              string title = el.Attribute("name")!.Value;
              string alt = el.Attribute("name")!.Value;

              string link = String.Format("<img title=\"{0}\" data-toggle=\"tooltip\" src=\"{1}{2}\" alt=\"{3}\" class=\"emote\" loading=\"lazy\" />", title, urlpath, image, alt);

              _formatters.Add(new SearchReplaceFormatter(WebUtility.HtmlEncode(emoticon), link));
          }

      }

		#endregion  Public Constructors  

		#region  Public Methods  

    public string Format(string data)
    {
        foreach (IHtmlFormatter formatter in _formatters!)
            data = formatter.Format(data);

        return data;
    }

		#endregion  Public Methods  
  }
}

using BbCodeFormatter.Formatters;

namespace BbCodeFormatter.Processors
{
  public static class PlainTextProcessor
  {
    static readonly List<IHtmlFormatter> _formatters;

    static PlainTextProcessor()
    {
      _formatters = new List<IHtmlFormatter>
      {
          new SearchReplaceFormatter("\r", ""),
          new SearchReplaceFormatter("\n", "<br />")
      };
    }

    public static string Format(string data)
    {
      foreach (IHtmlFormatter formatter in _formatters)
        data = formatter.Format(data);

      return data;
    }
  }
}

namespace BbCodeFormatter.Formatters
{
  internal class LineBreaksFormatter : IHtmlFormatter
  {
    #region  Private Member Declarations

    private string[] _exclusionCodes;
    private List<IHtmlFormatter> _formatters;

    #endregion  Private Member Declarations

    #region  Public Constructors

    public LineBreaksFormatter(string[] exclusionCodes)
    {
      _exclusionCodes = exclusionCodes;

        _formatters = new List<IHtmlFormatter>
        {
            new SearchReplaceFormatter("\r", ""),
            new SearchReplaceFormatter("\n", "<br />")
        };
        //_formatters.Add(new SearchReplaceFormatter("\n\n", "</p><p>"));
    }

    #endregion  Public Constructors

    #region  Public Methods

    public string Format(string data)
    {
      int blockStart;

        var blockEnd = 0;
          do
          {
              blockStart = GetNextBlockStart(blockEnd, data, out string? tagName);
              string? nonBlockText;
              if (blockStart != -1)
                nonBlockText = data.Substring(blockEnd, blockStart - blockEnd);
              else if (blockEnd != -1 && blockEnd < data.Length)
                nonBlockText = data.Substring(blockEnd);
              else
                nonBlockText = null;

            if (nonBlockText != null)
            {
                var originalLength = nonBlockText.Length;

              foreach (IHtmlFormatter formatter in _formatters)
                nonBlockText = formatter.Format(nonBlockText);

              if (blockStart != -1)
              {
                data = data.Substring(0, blockEnd) + nonBlockText + data.Substring(blockStart);

                blockStart += (nonBlockText.Length - originalLength);
                blockEnd = GetBlockEnd(blockStart, data, tagName);
              }
              else
                data = data.Substring(0, blockEnd) + nonBlockText;
            }

          } while (blockStart != -1);

      return data;
    }

    #endregion  Public Methods

    #region  Private Methods

    private int GetBlockEnd(int startingPosition, string data, string? tag)
    {
        string fullTag = String.Format("[/{0}]", tag);
      var matchPosition = data.IndexOf(fullTag, startingPosition, StringComparison.InvariantCultureIgnoreCase);

      if (matchPosition == -1)
        matchPosition = data.Length;

      return matchPosition;
    }

    private int GetNextBlockStart(int startingPosition, string data, out string? matchedTag)
    {
        var lowestPosition = -1;
      matchedTag = null;

      foreach (string exclusion in _exclusionCodes)
      {
          string tag = String.Format("[{0}]",exclusion);
        var matchPosition = data.IndexOf(tag, startingPosition, StringComparison.InvariantCultureIgnoreCase);

        if (matchPosition > -1 && (matchPosition < lowestPosition || lowestPosition == -1))
        {
          matchedTag = exclusion;
          lowestPosition = matchPosition;
        }
      }

      return lowestPosition;
    }

    #endregion  Private Methods
  }
}

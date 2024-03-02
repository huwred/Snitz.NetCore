using System.Text.RegularExpressions;
using SnitzCore.Data;

namespace BbCodeFormatter.Formatters
{
    /// <summary>
    /// Filters badwords from posts
    /// </summary>
    internal class BadWordFilter : IHtmlFormatter
    {
        private readonly List<IHtmlFormatter> _formatters;

        /// <summary>
        /// Filter out bad words
        /// </summary>
        public BadWordFilter(SnitzDbContext dbContext)
        {
            _formatters = new List<IHtmlFormatter>();
            var badwords = dbContext.Badwords.AsQueryable();
            foreach (var badword in badwords)
            {
                _formatters.Add(new RegexFormatter(@"\b" + Regex.Escape(badword.Word) + @"\b", badword.ReplaceWith!));
            }
        }
        public string Format(string data)
        {
            foreach (IHtmlFormatter formatter in _formatters)
            {
                data = formatter.Format(data);
            }

            return data;
        }
    }
}
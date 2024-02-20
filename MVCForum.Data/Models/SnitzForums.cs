namespace SnitzCore.Data.Models
{
    public class SnitzForums
    {
        public const string SectionName = "SnitzForums";
        public string? strForumUrl { get; set; }
        public string? strForumDescription { get; set; }
        public string? strForumTitle { get; set; }
        public string? forumTablePrefix { get; set; }
        public string? memberTablePrefix { get; set; }
        public string? strCopyright { get; set; }
        public string? strUniqueId { get; set; }
        public string? LanguageConnectionString { get; set;}
        public string? strVersion { get; set;}
    }

}

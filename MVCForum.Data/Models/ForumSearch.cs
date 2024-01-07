namespace SnitzCore.Data.Models
{
    public class ForumSearch
    {
        public string? Terms { get; set; }
        public SearchFor SearchFor { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool SearchArchives { get; set; }
        public bool SearchMessage { get; set; }
        public int? SearchCategory { get; set; }
        public int[] SearchForums { get; set; }

        public SearchDate SinceDate { get; set; }

    }
}

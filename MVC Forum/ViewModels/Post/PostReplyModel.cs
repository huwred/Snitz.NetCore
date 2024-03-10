namespace MVCForum.ViewModels.Post
{
    public class PostReplyModel : PostBase
    {
        public int PostId;

        public int AuthorPosts { get; set; }
        public short AuthorRole { get; set; }
        public SnitzCore.Data.Models.Member Author { get; set; } = null!;
        public bool Answer { get; internal set; }
    }
}

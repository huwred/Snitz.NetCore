namespace MVCForum.Models.Post
{
    public class PostReplyModel : PostBase
    {
        public PostReplyModel()
        {

        }
        public int PostId;

        public int AuthorPosts { get; set; }
        public short AuthorRole { get; set; }
        public SnitzCore.Data.Models.Member Author { get; set; }
    }
}

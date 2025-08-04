using SnitzCore.Data.Models;

namespace MVCForum.ViewModels.Post
{
    public class PostReplyModel : PostBase
    {
        public int PostId;

        public PostReply Reply { get; set; } = null!;
        public ArchivedReply ArchivedReply { get; set; } = null!;
        public int AuthorPosts { get; set; }
        public short AuthorRole { get; set; }
        public SnitzCore.Data.Models.Member Author { get; set; } = null!;
        public bool Answer { get; internal set; }

        public short Status { get; set; }

        public bool ShowSig { get; set; }
    }
}

namespace PostThanks.Models
{
    public class PostThanksViewModel
    {
        public int UserId { get; set; }
        public int TopicId { get; set; }
        public int ReplyId { get; set; }
        public bool PostAuthor { get; set; }
        public bool Thanked { get; set; }
        public bool ShowCount { get; set; }
        public bool Showlink { get; set; }
        public int ThanksCount { get; set; }
    }

    public class PostThanksProfile
    {
        public int Received { get; set; }
        public int Given { get; set; }
    }
}
namespace MVCForum.Models.Member
{
    public class EmailMemberViewModel
    {
        public string To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}

namespace MVCForum.ViewModels
{
    public class EmailTemplateViewModel
    {
        public required string Subject { get; set; }
        public required string Message { get; set; }
        public required string FromEmail { get; set; }
        public required string FromName { get; set; }
        public required string ToEmail { get; set; }
        public required string ToName { get; set; }

        public string? ReturnUrl { get; set; }
        public string? ForumUrl { get; set; }
        public string? ForumTitle { get; set; }

        public SnitzCore.Data.Models.Member? Member { get; set; }
    }
}

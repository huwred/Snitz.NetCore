using SnitzCore.Data.Models;

namespace MVCForum.Models.Member
{
    public class MemberDetailModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string Email { get; set; }
        public string? Title { get; set; }
        public ForumUser UserModel { get; set; }

        public SnitzCore.Data.Models.Member? Member { get; set; }
        public bool CanEdit { get; set; }
    }
}

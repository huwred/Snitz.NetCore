using SnitzEvents.Helpers;

namespace SnitzEvents.Models
{
    public class CalForumAuth
    {
        public CalEnums.CalAllowed Allowed { get; set; }
        public int ForumId { get; set; }
    }
}
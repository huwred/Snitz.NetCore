using Microsoft.Build.Framework;

namespace MVCForum.ViewModels
{
    public class ChangeUsernameModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public int CurrentUserId { get; set; }

    }
}
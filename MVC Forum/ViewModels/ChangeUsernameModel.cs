
using System.ComponentModel.DataAnnotations;

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
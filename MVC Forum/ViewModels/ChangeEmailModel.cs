using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels
{
    public class ChangeEmailModel
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        public required string CurrentEmail { get; set; }

        [Required]
        [EmailAddress]
        [Compare("CurrentEmail")]
        public required string Email { get; set; }

        [Required]
        [EmailAddress]
        public string? NewEmail { get; set; }

        [Required]
        [PasswordPropertyText]
        public string? Password { get; set; }
        
    }
}

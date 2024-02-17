using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels.User
{
    public class UserSignInModel
    {
        [Required]
        [Display(Name = "Username / Email")]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? ReturnUrl { get; set; } = null;
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}

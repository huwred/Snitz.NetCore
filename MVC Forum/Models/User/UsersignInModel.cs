using System.ComponentModel.DataAnnotations;

namespace MVCForum.Models.User
{
    public class UserSignInModel
    {
        [Required]
        [Display(Name="Username / Email")]
        public string Username { get; set; }
  
        [Required]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; } = null;
        [Display(Name="Remember Me")]
        public bool RememberMe { get; set; }
    }
}

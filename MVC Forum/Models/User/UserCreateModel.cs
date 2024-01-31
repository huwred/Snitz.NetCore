using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Models.User
{
    public class UserCreateModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Name { get; set; } = null!;

        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; } = null!;

        [Required]
        [PasswordPropertyText(true)]
        public string Password { get; set; } = null!;

        public List<string>? RequiredFields { get; set; }
    }
}

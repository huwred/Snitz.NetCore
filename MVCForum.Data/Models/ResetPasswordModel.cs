using System.ComponentModel.DataAnnotations;

namespace SnitzCore.Data.Models
{
    public class ResetPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
        [Required]
        public string Username { get; set; } = null!;

        public string? Token { get; set; }
    }
}

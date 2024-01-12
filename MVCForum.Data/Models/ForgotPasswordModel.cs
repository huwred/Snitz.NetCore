using System.ComponentModel.DataAnnotations;

namespace SnitzCore.Data.Models
{
    public class ForgotPasswordModel
    {
        //[Required]
        //[EmailAddress]
        //public string? Email { get; set; }
        [Required]
        public string? Username { get; set; }
    }
}
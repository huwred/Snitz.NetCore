using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MVCForum.ViewModels
{
    public class EmailViewModel
    {

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string FromEmail { get; set; }
        [Required]
        public string FromName { get; set; }

        [Required]
        public string ToName { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                           @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                           @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
            ErrorMessage = "Email is not valid")]
        public string ToEmail { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public string ReturnUrl { get; set; }

        public bool AdminEmail { get; set; }

        public IFormFile Attachment { get; set; }
        public int MemberId { get; set; }
    }
}

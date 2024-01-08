using System.ComponentModel.DataAnnotations;

namespace MVCForum.Models.PrivateMessage
{
    public class PrivateMessagePostModel
    {
        [Required(ErrorMessage = "Please add a recipient")]
        [MinLength(3,ErrorMessage = "You must provide 3 characters or more")]
        public string To { get; set; }
        public string[] Recipients { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Please provide a message")]
        public string Message { get; set; }
        [Display(Name = "Include signature")]
        public bool IncludeSig { get; set; }
        [Display(Name = "Save to sent items")]
        public bool SaveToSent { get; set; }
        [Display(Name = "Save as draft")]
        public bool Draft { get; set; }

        public bool IsReply { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels.PrivateMessage
{
    public class PrivateMessagePostModel
    {
        [Required(ErrorMessage = "Please add a recipient")]
        [MinLength(3, ErrorMessage = "You must provide 3 characters or more")]
        public string To { get; set; } = null!;

        public string[]? Recipients { get; set; }
        [Required]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Please provide a message")]
        public string Message { get; set; } = null!;

        [Display(Name = "Include signature")]
        public bool IncludeSig { get; set; }
        [Display(Name = "Save to sent items")]
        public bool SaveToSent { get; set; }
        [Display(Name = "cbxDraft")]
        public bool Draft { get; set; }

        public bool IsReply { get; set; }

        public bool IsPopUp { get; set; }
    }
}

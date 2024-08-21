using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels.Post
{
    /// <summary>
    /// View Model for Post Moderation
    /// </summary>
    public class ApproveTopicViewModal
    {
        public string? ApprovalMessage { get; set; }
        [Required(ErrorMessage = "Please select an Action")]
        public string? PostStatus { get; set; }
        public int Id { get; set; }

        public bool EmailAuthor { get; set; }
    }
}

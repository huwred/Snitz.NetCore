using System.Collections.Generic;
using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels.Post
{
    public class NewPostModel : PostBase
    {
        public new int Id { get; set; }
        [Required]
        public string? ForumName { get; set; }
        public int ForumId { get; set; }
        public int CatId { get; set; }
        public string? ImageUrl { get; set; }
        [RequiredIfTrue(nameof(IsPost), ErrorMessage = "Please provide a Title for the Topic")]
        public string Title { get; set; } = null!;

        public int TopicId { get; set; }

        public bool AllowRating {get;set;}
        public bool AllowTopicRating {get;set;}
        public bool IsAuthor {get;set;}

        public bool IsPost { get; set; }
        public bool UseSignature { get; set; }
        public bool IsLocked { get; set; }
        public bool IsSticky { get; set; }
        public bool DoNotArchive { get; set; }
        public bool Answer { get; internal set; }

        public bool IsArchived { get; set; }

        public Dictionary<int, string>? Forums { get; set; }
    }
}

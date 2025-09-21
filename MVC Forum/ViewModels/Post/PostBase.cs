using System;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels.Post
{
    public abstract class PostBase
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public string? AuthorTitle { get; set; } = null!;
        public int AuthorRating { get; set; }
        public string? AuthorImageUrl { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTime Created { get; set; }

        public DateTime? Edited { get; set; }
        public string? EditedBy { get; set; }

    }
}

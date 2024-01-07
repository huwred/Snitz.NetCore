using System;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Models.Post
{
    public abstract class PostBase
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int AuthorRating { get; set; }
        public string AuthorImageUrl { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Created { get; set; }


    }
}

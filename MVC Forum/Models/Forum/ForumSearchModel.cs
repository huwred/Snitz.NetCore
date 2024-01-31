using Microsoft.AspNetCore.Mvc.Rendering;
using SnitzCore.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Models.Forum
{
    public class ForumSearchModel
    {
        [Required]
        [MinLength(3)]
        public string? Terms { get; set; }
        public SearchFor SearchFor { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool SearchArchives { get; set; }
        public bool SearchMessage { get; set; }
        public int SearchCategory { get; set; }
        public int[]? SearchForums { get; set; }

        public SearchDate SinceDate { get; set; }

        public SelectList? Categories { get; set; }
        public SelectList? Forums { get; set; }

        public ForumTopicModel? Results { get; set; }

    }
}

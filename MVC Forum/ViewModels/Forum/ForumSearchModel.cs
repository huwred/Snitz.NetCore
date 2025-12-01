using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using SnitzCore.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.ViewModels.Forum
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

        public System.DateTime? SinceDate { get; set; }

        public string SinceWhen {get;set;} = "after";

        public SelectList? Categories { get; set; }
        public SelectList? Forums { get; set; }

        public ForumTopicModel? Results { get; set; }

    }
}

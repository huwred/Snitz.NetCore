using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Models;

namespace MVCForum.ViewModels
{
    public class PMSearchViewModel
    {
        [Display(Name ="Search_Term")]
        //[RequiredIf("MemberName", "", "PropertyRequired")]
        //[SnitzCore.Filters.StringLength(100, MinimumLength = 3)]
        public string Term { get; set; } = null!;

        //[Required]
        [Display(Name ="Search_PhraseType")]
        public SearchFor PhraseType { get; set; }

        [Display(Name ="Search_Date")]
        public SearchDate SearchByDays { get; set; }

        ////[LocalisedDisplayName("Search_Member", "labels")]
        [Remote("UserExists", "Account")]
        public string? MemberName { get; set; }

        //[Required]
        [Display(Name ="Search_In")]
        public SearchIn SearchIn { get; set; }
    }
}

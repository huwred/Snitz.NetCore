using Microsoft.AspNetCore.Mvc;

namespace MVCForum.ViewModels
{
    public class PMSearchViewModel
    {
        //[LocalisedDisplayName("Search_Term", "labels")]
        //[RequiredIf("MemberName", "", "PropertyRequired")]
        //[SnitzCore.Filters.StringLength(100, MinimumLength = 3)]
        public string Term { get; set; } = null!;

        //[Required]
        //public SearchWordMatch PhraseType { get; set; }

        ////[LocalisedDisplayName("Search_Date", "labels")]
        //public SearchDays SearchByDays { get; set; }

        ////[LocalisedDisplayName("Search_Member", "labels")]
        [Remote("UserExists", "Account")]
        public string? MemberName { get; set; }

        //[Required]
        ////[LocalisedDisplayName("Search_In", "labels")]
        //public SearchIn SearchIn { get; set; }
    }
}

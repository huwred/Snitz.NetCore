namespace Snitz.PhotoAlbum.ViewModels
{
    public class SearchViewModel
    {
        public SearchViewModel() { }
        /// <summary>
        /// 0 = Member
        /// 1 = Scientific name
        /// 2 = Local name
        /// 3 = Description
        /// </summary>
        public string[]? SrchIn { get; set; }
   
        public int SrchGroupId { get; set; }    = 0; 
        public string searchTerms { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
    }
}

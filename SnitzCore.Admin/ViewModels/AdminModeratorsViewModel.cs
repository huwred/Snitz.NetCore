using System.Collections.ObjectModel;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace SnitzCore.BackOffice.ViewModels;

public class AdminModeratorsViewModel
{

    public int ForumId { get; set; }
    public int MemberId { get; set; }

    public Dictionary<int, string> ForumList { get; set; }
    public Dictionary<int, string>? ModList { get; set; }
    public ICollection<int> ForumModerators { get; set; }
    public List<Group>? Groups { get; set; }
    public List<Badword>? Badwords { get; set; }

    public List<MemberNamefilter>? UserNamefilters { get; set; }

    public AdminModeratorsViewModel()
    {
        this.ForumModerators = new Collection<int>();
        this.ForumList = new Dictionary<int, string> { { -1, "--Select Forum--" } };
            
    }
    public AdminModeratorsViewModel(IForum forumservice)
    {

        this.ForumList = new Dictionary<int, string> { { -1, "--Select Forum--" } };
        foreach (KeyValuePair<int, string> forum in forumservice.ForumList().ToDictionary(t => t.Key, t => t.Value))
        {
            this.ForumList.Add(forum.Key, forum.Value);
        }
        this.ForumModerators = new Collection<int>();           
    }
    public class ArchivesViewModel
    {
        public List<Category>? Categories { get; set; }
        //public List<Forum> Forums { get; set; }
    }
}
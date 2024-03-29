using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Data.Interfaces
{
    public interface IForum
    {
        Forum GetById(int id);
        IPagedList<Post> GetPagedTopics(int id, int pagesize, int page);
        IEnumerable<Forum> GetAll();
        Dictionary<int, string?> CategoryList();
        Post? GetLatestPost(int forumId);
        Task Create(Forum forum);
        Task Delete(int forumId);
        Task Update(Forum forum);
        Task UpdatePostCount(int forumId, int topic = 0, int reply = 0);
        Task UpdateForumTitle(int forumId, string newTitle);
        Task UpdateForumDescription(int forumId, string newDescription);
        Dictionary<int, string> ForumList();
        string ForumName(string rolename);
        Task EmptyForum(int id);
        Task<Forum> UpdateLastPost(int forumid);
        PagedList<Post> FetchMyForumTopics(int pagesize, int pagenum, IEnumerable<int> forumids);
        IEnumerable<string> GetTagStrings(List<int> list);
        IEnumerable<MyViewTopic> FetchAllMyForumTopics(IEnumerable<int> forumids);

        int PollsAuth(int id);
    }
}

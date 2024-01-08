using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Data.Interfaces
{
    public interface IPost
    {
        Post GetTopicWithRelated(int id);
        PostReply GetReply(int id);
        IEnumerable<Post> GetAllTopicsAndRelated();
        IPagedList<Post> GetFilteredPost(string searchQuery, out int totalcount, int pagesize, int page);
        IPagedList<Post> Find(ForumSearch searchQuery, out int totalcount, int pagesize, int page);
        Post GetLatestReply(int id);
        Task Create(Post post);
        Task DeleteTopic(int id);
        Task DeleteReply(int id);
        Task Update(Post post);
        Task Update(PostReply post);

        Task UpdateTopicContent(int id, string content);
        Task UpdateReplyContent(int id, string content);
        Task UpdateViewCount(int id);
        IEnumerable<Post> GetLatestPosts(int n);
        IPagedList<PostReply> GetPagedReplies(int topicid, int pagesize, int pagenumber);
        Task Create(PostReply post);
        Task<bool> LockTopic(int id, short status);
    }
}

using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Data.Interfaces
{
    public interface IPost
    {
        Post GetTopic(int id);
        Post GetTopicForUpdate(int id);
        Post GetTopicWithRelated(int id);
        PostReply GetReply(int id);
        PostReply GetReplyForUdate(int id);

        IEnumerable<Post> GetAllTopicsAndRelated();
        IPagedList<Post> GetFilteredPost(string searchQuery, out int totalcount, int pagesize, int page);
        IPagedList<Post> Find(ForumSearch searchQuery, out int totalcount, int pagesize, int page);
        Post GetLatestReply(int id);
        Task<int>  Create(Post post);
        Task DeleteTopic(int id);
        Task DeleteReply(int id);
        Task Update(Post post);
        Task Update(PostReply post);
        Task UpdateTopicContent(int id, string content);
        Task UpdateReplyContent(int id, string content);
        Task UpdateViewCount(int id);
        IEnumerable<Post> GetLatestPosts(int n);
        IPagedList<PostReply> GetPagedReplies(int topicid, int pagesize, int pagenumber);
        Task<int>  Create(PostReply post);
        Task<bool> LockTopic(int id, short status);

        Task UpdateLastPost(int topicid, int? moderatedcount);
        Task<bool> Answer(int id);
        ArchivedTopic GetArchivedTopic(int id);
        ArchivedTopic GetArchivedTopicWithRelated(int id);
        Task SetStatus(int id, Status status);
        Task SetReplyStatus(int id, Status status);

        bool HasPoll(int id);
        Poll? GetPoll(int topicid);
    }
}

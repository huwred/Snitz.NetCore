using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace SnitzCore.Data.Interfaces
{
    public interface IPost
    {
        Post? GetTopic(int id);
        Task<Post?> GetTopicAsync(int id);
        Post GetTopicForUpdate(int id);
        Task<Post?> GetTopicWithRelated(int id);
        PostReply? GetReply(int id);
        ArchivedReply? GetArchivedReply(int id);
        PostReply? GetReplyForUdate(int id);
        List<Post> GetById(int[] ids);
        IEnumerable<Post> GetAllTopicsAndRelated();
        IPagedList<Post> GetFilteredPost(string searchQuery, out int totalcount, int pagesize, int page, int catid,int forumid);
        IPagedList<Post> Find(ForumSearch searchQuery, out int totalcount, int pagesize, int page);
        IPagedList<ArchivedPost> FindArchived(ForumSearch searchQuery, out int totalcount, int pagesize, int page);
        Post GetLatestReply(int id);
        Task<int>  Create(Post post);
        Task DeleteTopic(int id);
        Task DeleteReply(int id);
        void Update(Post post);
        Task Update(PostReply post);

        void UpdateViewCount(int id, int viewCount);
        void UpdateArchivedViewCount(int id, int viewCount);
        IEnumerable<Post> GetLatestPosts(int n);
        IPagedList<PostReply> GetPagedReplies(int topicid, int pagesize, int pagenumber);
        Task<int>  Create(PostReply post);
        int  CreateForMerge(int[]? selected);
        Task<bool> LockTopic(int id, short status);

        decimal GetTopicRating(int topicid);
        decimal GetReplyRating(int topicid);

        Task UpdateLastPost(int topicid, int? moderatedcount);
        Task<bool> Answer(int id);
        ArchivedPost? GetArchivedTopic(int id);
        ArchivedPost? GetArchivedTopicWithRelated(int id);
        Task SetStatus(int id, Status status);
        Task SetReplyStatus(int id, Status status);

        bool HasPoll(int id);
        Poll? GetPoll(int topicid);
        void MoveSubscriptions(int oldtopicid, int newtopicid, int newforumId, int newcatId);
        void MoveReplies(int oldtopicid, Post newTopic);
        Post? SplitTopic(string[] ids, int forumId, string subject);
        Task<bool> MakeSticky(int id, short status = 0);
        Task UpdateReplyTopic(Post post);
        Task DeleteArchivedReply(int id);
        Task DeleteArchivedTopic(int id);
    }
}

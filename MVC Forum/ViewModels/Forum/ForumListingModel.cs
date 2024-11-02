using SnitzCore.Data.Models;
using System;

namespace MVCForum.ViewModels.Forum
{
    public class ForumListingModel
    {
        public int CategoryId { get; set; }
        public int Id { get; set; }
        public int Polls { get; set; }
        public int Topics { get; set; }
        public int Posts { get; set; }
        public int Status { get; set; }
        public ForumAuthType AccessType { get; set; }
        public Moderation ForumModeration { get; set; }
        public CategorySubscription? CategorySubscription { get; set; }
        public ForumSubscription? ForumSubscription { get; set; }
        public DateTime? LastPostDateTime { get; set; }
        public int? LastPostAuthorId { get; set; }
        public int? LastPostTopicId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public string? CategoryName { get; set; }
        public SnitzCore.Data.Models.Member? LastPostAuthor { get; set; }
        public SnitzCore.Data.Models.Post? LastPost { get; set; }
        public ForumType ForumType { get; set; }
        public string? Url { get; set; }
        public DefaultDays DefaultView { get; set; }
        public string? OrderBy { get; set; }
        public string? SortDir { get; set; }
        public int? LastPostReplyId { get; set; }
        public int Order { get; set; }

        public int? ArchivedCount { get; set; }
    }

    public class ForumList{
        public int ForumId { get; set; }
        public string ForumName { get; set; }
        public int CatId { get; set; }
    }
}

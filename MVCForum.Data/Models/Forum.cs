using SnitzCore.Data.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SnitzCore.Data.Models;

[SnitzTable("FORUM", "FORUM")]
public class Forum
{
    [Key]
    [Column("FORUM_ID")]
    public int Id { get; set; }

    [Column("CAT_ID")]
    public int CategoryId { get; set; }

    [Column("F_STATUS")]
    public short Status { get; set; }

    [Column("F_MAIL")]
    public short Mail { get; set; }

    [Column("F_SUBJECT")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    [Column("F_URL")]
    [StringLength(255)]
    public string? Url { get; set; }

    [Column("F_DESCRIPTION")]
    public string Description { get; set; } = null!;

    [Column("F_TOPICS")]
    public int TopicCount { get; set; }

    [Column("F_COUNT")]
    public int ReplyCount { get; set; }

    [Column("F_LAST_POST")]
    [StringLength(14)]
    public string? LastPost { get; set; }

    [Column("F_PASSWORD_NEW")]
    public string? Password { get; set; }

    [Column("F_PRIVATEFORUMS")]
    public ForumAuthType Privateforums { get; set; }

    [Column("F_TYPE")]
    public short Type { get; set; }

    [Column("F_IP")]
    public string? Ip { get; set; }

    [Column("F_LAST_POST_AUTHOR")]
    public int? LastPostAuthorId { get; set; }

    [Column("F_LAST_POST_TOPIC_ID")]
    [ForeignKey("LatestTopic")]
    public int? LatestTopicId { get; set; }

    [Column("F_LAST_POST_REPLY_ID")]
    public int? LatestReplyId { get; set; }

    [Column("F_A_TOPICS")]
    public int ArchivedTopics { get; set; }

    [Column("F_A_COUNT")]
    public int? ArchivedCount { get; set; }

    [Column("F_MODERATION")]
    public Moderation Moderation { get; set; }

    [Column("F_SUBSCRIPTION")]
    public int Subscription { get; set; }

    [Column("F_ORDER")]
    public int Order { get; set; }

    [Column("F_DEFAULTDAYS")]
    public int Defaultdays { get; set; }

    [Column("F_COUNT_M_POSTS")]
    public short CountMemberPosts { get; set; }

    [Column("F_L_ARCHIVE")]
    public string? LastArchived { get; set; }

    [Column("F_ARCHIVE_SCHED")]
    public int ArchiveSched { get; set; }

    [Column("F_L_DELETE")]
    public string? LastDelete { get; set; }

    [Column("F_DELETE_SCHED")]
    public int DeleteSched { get; set; }

    [Column("F_POLLS")]
    public int Polls { get; set; }

    [Column("F_RATING")]
    public short Rating { get; set; }

    [Column("F_POSTAUTH")]
    public int Postauth { get; set; }

    [Column("F_REPLYAUTH")]
    public int Replyauth { get; set; }

    public virtual IEnumerable<Post>? Posts { get; set; }
    public virtual Category? Category { get; set; }

    public virtual Member? LastPostAuthor { get; set; }
    [NotMapped]
    public virtual Post? LatestTopic { get; set; }
    public virtual IEnumerable<ForumModerator>? ForumModerators { get; set; }
    public virtual IEnumerable<ArchivedPost>? ArchivedPosts { get; set; }
}

using SnitzCore.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("TOPICS","FORUM")]
public partial class Post
{
    [Column("TOPIC_ID")]
    [Key]
    public int Id { get; set; }

    [Column("CAT_ID")]
    public int CategoryId { get; set; }

    [Column("FORUM_ID")]
    public int ForumId { get; set; }

    [Column("T_STATUS")]
    public short Status { get; set; }

    [Column("T_MAIL")]
    public short Mail { get; set; }

    [Column("T_SUBJECT")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    [Column("T_MESSAGE")]
    public string Content { get; set; } = null!;

    [Column("T_AUTHOR")]
    [ForeignKey("Member")]
    public int MemberId { get; set; }

    [Column("T_REPLIES")]
    public int ReplyCount { get; set; }

    [Column("T_UREPLIES")]
    public int UnmoderatedReplies { get; set; }

    [Column("T_VIEW_COUNT")]
    public int ViewCount { get; set; }

    [Column("T_LAST_POST")]
    [StringLength(14)]
    public string? LastPostDate { get; set; }

    [Column("T_DATE")]
    [StringLength(14)]
    public string Created { get; set; } = null!;

    //public int? LastPoster { get; set; }

    [Column("T_IP")]
    [StringLength(50)]
    public string? Ip { get; set; }

    [Column("T_LAST_POST_AUTHOR")]
    [ForeignKey("LastPostAuthor")]
    public int? LastPostAuthorId { get; set; }

    [Column("T_LAST_POST_REPLY_ID")]
    public int? LastPostReplyId { get; set; }

    [Column("T_ARCHIVE_FLAG")]
    public int? ArchiveFlag { get; set; }

    [Column("T_LAST_EDIT")]
    [StringLength(14)]
    public string? LastEdit { get; set; }

    [Column("T_LAST_EDITBY")]
    public int? LastEditby { get; set; }

    [Column("T_STICKY")]
    public short IsSticky { get; set; }

    [Column("T_SIG")]
    public short Sig { get; set; }

    [Column("T_ISPOLL")]
    public short Ispoll { get; set; }

    [Column("T_POLLSTATUS")]
    public short Pollstatus { get; set; }

    [Column("T_RATING_TOTAL_COUNT")]
    public int RatingTotalCount { get; set; }

    [Column("T_RATING_TOTAL")]
    public int RatingTotal { get; set; }

    [Column("T_ALLOW_RATING")]
    public int AllowRating { get; set; }
    [Column("T_ANSWERED")]
    public bool Answered { get; set; }
    public virtual Member? Member { get; set; }
    public virtual Member? LastPostAuthor { get; set; }

    public virtual Forum? Forum { get; set; }
    public virtual Category? Category { get; set; }
    public virtual IEnumerable<PostReply> Replies { get; set; }

    public decimal GetTopicRating()
        {
            //var ratings = repo.First<Topic>("WHERE TOPIC_ID=@0", this.Id);
            decimal rating = 0;
            if (this.RatingTotal > 0)
            {
                decimal ratingSum = Decimal.Divide(this.RatingTotal,10);
                var ratingCount = this.RatingTotalCount;
                rating = (ratingSum / ratingCount);
            }
            return decimal.Parse(rating.ToString());
        }

    public Post(){
        Created = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        LastPostDate = Created;
        LastPostReplyId = null;
        LastPostAuthorId = null;
        LastEdit = null;
        LastEditby = null;
        Title = string.Empty;
        Content = string.Empty;
        Ip = string.Empty;
        IsSticky = 0;
        Sig = 0;
        Ispoll = 0;
        Pollstatus = 0;
        ReplyCount = 0;
        UnmoderatedReplies = 0;
        ViewCount = 0;
        Status = 1; // Open
        Replies = new HashSet<PostReply>();
    }
}

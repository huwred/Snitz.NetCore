using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("POLL_VOTES","FORUM")]
public partial class PollVote
{
    [Column("POLLVOTES_ID")]
    [Key]
    public int Id { get; set; }

    [Column("POLL_ID")]
    public int PollId { get; set; }

    [Column("CAT_ID")]
    public int CategoryId { get; set; }

    [Column("FORUM_ID")]
    public int ForumId { get; set; }

    [Column("TOPIC_ID")]
    public int PostId { get; set; }

    [Column("MEMBER_ID")]
    public int MemberId { get; set; }

    [Column("GUEST_VOTE")]
    public int GuestVote { get; set; }

    public virtual Poll Poll { get; set; }
}

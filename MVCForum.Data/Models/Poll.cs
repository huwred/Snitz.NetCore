using System.Collections.Generic;
using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("POLLS","FORUM")]
public partial class Poll
{
    [Column("POLL_ID")]
    [Key]
    public int Id { get; set; }

    [Column("CAT_ID")]
    public int CatId { get; set; }

    [Column("FORUM_ID")]
    public int ForumId { get; set; }

    [Column("TOPIC_ID")]
    public int TopicId { get; set; }

    [Column("P_WHOVOTES")]
    [StringLength(50)]
    public string? Whovotes { get; set; }

    [Column("P_LASTVOTE")]
    [StringLength(14)]
    public string? Lastvote { get; set; }

    [Column("P_QUESTION")]
    [StringLength(255)]
    public string Question { get; set; } = null!;

    public virtual Post? Topic { get; set; }
    public virtual IEnumerable<PollAnswer> PollAnswers { get; set; }
    public virtual IEnumerable<PollVote>? PollVotes { get; set; }
}

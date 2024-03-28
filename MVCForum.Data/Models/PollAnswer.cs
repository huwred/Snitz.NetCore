using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("POLL_ANSWERS","FORUM")]
public partial class PollAnswer
{
    [Column("POLLANSWER_ID")]
    [Key]
    public int Id { get; set; }

    [Column("POLL_ID")]
    public int PollId { get; set; }

    [Column("POLLANSWER_LABEL")]
    [StringLength(255)]
    public string Label { get; set; } = null!;

    [Column("POLLANSWER_ORDER")]
    public int Order { get; set; }

    [Column("POLLANSWER_COUNT")]
    public int Count { get; set; }

    public virtual Poll Poll { get; set; }
}

using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("A_REPLY", "FORUM")]
public partial class ArchivedReply
{
    [Column("REPLY_ID")]
    public int Id { get; set; }

    [Column("CAT_ID")]
    public int CategoryId { get; set; }

    [Column("FORUM_ID")]
    public int ForumId { get; set; }

    [Column("TOPIC_ID")]
    public int ArchivedPostId { get; set; }

    [Column("R_MAIL")]
    public short Mail { get; set; }

    [Column("R_AUTHOR")]
    public int MemberId { get; set; }

    [Column("R_MESSAGE")]
    public string Content { get; set; } = null!;

    [Column("R_DATE")]
    [StringLength(14)]
    public string Created { get; set; } = null!;

    [Column("R_IP")]
    [StringLength(50)]
    public string? Ip { get; set; }

    [Column("R_STATUS")]
    public short Status { get; set; }

    [Column("R_LAST_EDIT")]
    [StringLength(14)]
    public string? LastEdited { get; set; }

    [Column("R_LAST_EDITBY")]
    public int? LastEditby { get; set; }

    [Column("R_SIG")]
    public short Sig { get; set; }

    [Column("R_RATING")]
    public int Rating { get; set; }

    public virtual Member? Member { get; set; }
}

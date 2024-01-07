using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("MODERATOR","FORUM")]
public partial class ForumModerator
{
    [Column("MOD_ID")]
    [Key]
    public int Id { get; set; }

    [Column("FORUM_ID")]
    public int ForumId { get; set; }

    [Column("MEMBER_ID")]
    public int MemberId { get; set; }

    [Column("MOD_TYPE")]
    public short? Type { get; set; }

    public virtual Member? Member { get; set; }
}

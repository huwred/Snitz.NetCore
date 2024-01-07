using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("ALLOWED_MEMBERS", "FORUM")]
public partial class ForumAllowedMember
{
    [Column("MEMBER_ID")]
    public int MemberId { get; set; }

    [Column("FORUM_ID")]
    [ForeignKey("Forum")]
    public int ForumId { get; set; }

    public virtual Forum Forum { get; set; }
}

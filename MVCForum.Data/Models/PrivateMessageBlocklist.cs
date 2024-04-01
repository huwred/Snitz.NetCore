using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("PM_BLOCKLIST","FORUM")]
public partial class PrivateMessageBlocklist
{
    [Column("BL_ID")]
    [Key]
    public int Id { get; set; }

    [Column("BL_MEMBER_ID")]
    public int MemberId { get; set; }

    [Column("BL_BLOCKED_ID")]
    public int BlockedId { get; set; }

    [Column("BL_BLOCKED_NAME")]
    [StringLength(100)]
    public string BlockedName { get; set; } = null!;
}

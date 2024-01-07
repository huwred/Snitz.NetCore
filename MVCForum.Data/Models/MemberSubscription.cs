using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("SUBSCRIPTIONS","FORUM")]
public partial class MemberSubscription
{
    [Column("SUBSCRIPTION_ID")]
    [Key]
    public int Id { get; set; }

    [Column("MEMBER_ID")]
    public int MemberId { get; set; }

    [Column("CAT_ID")]
    public int CategoryId { get; set; }

    [Column("FORUM_ID")]
    public int ForumId { get; set; }

    [Column("TOPIC_ID")]
    public int PostId { get; set; }
}

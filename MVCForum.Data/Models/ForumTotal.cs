using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("TOTALS","FORUM")]
public partial class ForumTotal
{
    [Column("COUNT_ID")]
    public short Id { get; set; }

    [Column("P_COUNT")]
    public int PostCount { get; set; }

    [Column("P_A_COUNT")]
    public int ArchivedPostCount { get; set; }

    [Column("T_COUNT")]
    public int TopicCount { get; set; }

    [Column("T_A_COUNT")]
    public int ArchivedTopicCount { get; set; }

    [Column("U_COUNT")]
    public int UserCount { get; set; }
}

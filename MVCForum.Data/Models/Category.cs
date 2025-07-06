using SnitzCore.Data.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("CATEGORY", "FORUM")]
public class Category
{
    [Key]
    [Column("CAT_ID")]
    public int Id { get; set; }

    [Column("CAT_STATUS")]
    public short? Status { get; set; }

    [Column("CAT_NAME")]
    public string? Name { get; set; }

    [Column("CAT_MODERATION")]
    public int? Moderation { get; set; }

    [Column("CAT_SUBSCRIPTION")]
    public int? Subscription { get; set; }

    [Column("CAT_ORDER")]
    public int Sort { get; set; }

    public virtual required IEnumerable<Forum> Forums { get; set; }
    //public virtual Group?  Group { get; set; }
}

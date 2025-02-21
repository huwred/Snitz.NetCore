using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("SPAM_MAIL","FORUM")]
public partial class SpamFilter
{
    [Column("SPAM_ID")]
    [Key]
    public int Id { get; set; }

    [Column("SPAM_SERVER")]
    [StringLength(255)]
    public string Server { get; set; } = null!;

    [NotMapped]public bool Ischecked{ get; set; }
}

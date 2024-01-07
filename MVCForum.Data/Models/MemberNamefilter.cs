using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("NAMEFILTER","FORUM")]
public partial class MemberNamefilter
{
    [Column("N_ID")]
    [Key]
    public int Id { get; set; }

    [Column("N_NAME")]
    [StringLength(75)]
    public string Name { get; set; } = null!;
}

using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("BADWORDS", "FORUM")]
public partial class Badword
{
    [Column("B_ID")]
    [Key]
    public int Id { get; set; }

    [Column("B_BADWORD")]
    [StringLength(50)]
    public string Word { get; set; } = null!;

    [Column("B_REPLACE")]
    [StringLength(50)]
    public string? ReplaceWith { get; set; }

    [NotMapped]
    public bool IsDeleted { get; set; }
}

using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("GROUP_NAMES","FORUM")]
public partial class GroupName
{
    [Column("GROUP_ID")]
    [Key]
    public int Id { get; set; }

    [Column("GROUP_NAME")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("GROUP_DESCRIPTION")]
    [StringLength(255)]
    public string? Description { get; set; }

    [Column("GROUP_ICON")]
    [StringLength(255)]
    public string? Icon { get; set; }

    [Column("GROUP_IMAGE")]
    [StringLength(255)]
    public string? Image { get; set; }


}

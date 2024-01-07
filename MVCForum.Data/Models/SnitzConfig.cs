using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("CONFIG_NEW","FORUM")]
public partial class SnitzConfig
{
    [Column("ID")]
    [Key]
    public int Id { get; set; }

    [Column("C_VARIABLE")]
    [StringLength(255)]
    public string Key { get; set; } = null!;

    [Column("C_VALUE")]
    [StringLength(255)]
    public string? Value { get; set; }
}

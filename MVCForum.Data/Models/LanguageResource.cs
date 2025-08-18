using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("LANGUAGE_RES","")]
public partial class LanguageResource
{
    [Column("pk")]
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Column("ResourceId")]
    [Required]
    public string Name { get; set; } = null!;

    public string? Value { get; set; } = null!;

    [StringLength(6)]
    [Required]
    public string Culture { get; set; } = null!;

    [StringLength(32)]
    public string? Type { get; set; } = "string";

    [StringLength(256)]
    public string? ResourceSet { get; set; } = null!;
}

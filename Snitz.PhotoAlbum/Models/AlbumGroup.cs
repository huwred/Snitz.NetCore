using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SnitzCore.Data.Extensions;

namespace Snitz.PhotoAlbum.Models;

[SnitzTable("ORG_GROUP", "FORUM")]
public class AlbumGroup
{
    [Column("O_GROUP_ID")]
    [Key]
    public int Id { get; set; }
    [Column("O_GROUP_ORDER")]
    public int Order { get; set; }
    [Column("O_GROUP_NAME")]
    public string? Description { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data.Extensions;

namespace Snitz.PhotoAlbum.Models;

[SnitzTable("IMAGE_CAT","FORUM")]
public class AlbumCategory
{
    [Column("CAT_ID")]
    [Key]
    public int CatId { get; set; }
    [Column("MEMBER_ID")]
    public int MemberId { get; set; }
    [Column("CAT_DESC")]
    public string? Description { get; set; }
}
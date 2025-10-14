using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("RANKING","FORUM")]
public partial class MemberRanking
{
    [Column("RANK_ID")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Column("R_TITLE")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    [Column("R_IMAGE")]
    [StringLength(50)]
    public string Image { get; set; } = null!;

    [Column("R_POSTS")]
    public int? Posts { get; set; }

    [Column("R_IMG_REPEAT")]
    public int ImgRepeat { get; set; }


}

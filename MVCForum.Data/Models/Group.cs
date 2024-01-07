using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("GROUPS","FORUM")]    
public partial class Group
{
    [Column("GROUP_KEY")]
    [Key]
    public int Id { get; set; }

    [Column("GROUP_ID")]
    public int? GroupNameId { get; set; }

    [Column("GROUP_CATID")]
    public int CategoryId { get; set; }

    public virtual GroupName GroupName { get; set; }
    public virtual Category Category { get; set; }
}

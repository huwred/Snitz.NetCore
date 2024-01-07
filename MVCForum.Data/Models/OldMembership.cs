using SnitzCore.Data.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models
{
    [Table("webpages_Membership")]
    public class OldMembership
    {
        [Key]
        [Column("UserId")]
        public int Id { get; set; }

        [Column("CreateDate")]
        public DateTime Created { get; set; }

        [Column("Password")]
        [StringLength(75)]
        public string Password { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;

namespace Snitz.PhotoAlbum.Models
{
    public class AlbumImageEntityConfiguration: IEntityTypeConfiguration<AlbumImage>
    {
        public void Configure(EntityTypeBuilder<AlbumImage> builder)
        {
            builder.HasIndex(e => e.Id).IsUnique();
            builder.HasIndex(e => e.MemberId);
        }
    }
    [SnitzTable("IMAGES", "FORUM")]
    public class AlbumImage
    {
        [Column("I_ID")]
        [Key]
        public int Id { get; set; }

        [Column("I_MID")]
        public int MemberId { get; set; }
        [Column("I_CAT")]
        public int CategoryId { get; set; }

        [Column("I_WIDTH")]
        public int? Width { get; set; }
        [Column("I_HEIGHT")]
        public int? Height { get; set; }
        [Column("I_SIZE")]
        public int Size { get; set; }
        [Column("I_VIEWS")]
        public int? Views { get; set; }

        /// <summary>
        /// Original FileName
        /// </summary>
        [Column("I_LOC")]
        public string? Location { get; set; }
        [Column("I_DESC")]
        public string? Description { get; set; }
        [Column("I_TYPE")]
        public string? Mime { get; set; }

        [Column("I_DATE")]
        public string? Timestamp { get; set; }

        [Column("I_GROUP_ID")]
        public int? GroupId { get; set; }

        [Column("I_SCIENTIFICNAME")]
        public string? ScientificName { get; set; }

        [Column("I_NORWEGIANNAME")]
        public string? CommonName { get; set; }
        /// <summary>
        /// Do not show to other Members
        /// </summary>
        [Column("I_PRIVATE")]
        public bool IsPrivate { get; set; }
        /// <summary>
        /// DO not use as Featured Image
        /// </summary>
        [Column("I_NOTFEATURED")]
        public bool DoNotFeature { get; set; }

        public virtual Member? Member { get; set; } = null!;

        //public virtual string MemberName { get; set; }

        public virtual AlbumGroup? Group { get; set; }
        public virtual AlbumCategory? Category { get; set; }

        [NotMapped]
        public string ImageName { get; set; } = null!;

        //public byte[] ImageData { get; set; }

        //public System.Drawing.Image Img { get; set; }

    }

    public class Slider
    {
        public string Src { get; set; }
        public string Title { get; set; }
    }
}
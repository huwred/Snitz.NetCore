using SnitzCore.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snitz.PhotoAlbum.Models
{
    [NotMapped]
    public class ExtendedMember : Member
    {
        public virtual IEnumerable<AlbumImage> Images { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Snitz.PhotoAlbum.Models;

public class AlbumList
{
    //a.MEMBER_ID, a.M_NAME, Count(b.I_ID) AS imgCount, Max(b.I_DATE) AS imgLDate
    //[Column("MEMBER_ID")]
    public int MemberId { get; set; }
    //[Column("M_NAME")]
    public string Username { get; set; }

    public virtual int imgCount { get; set; }

    public virtual string imgLastUpload { get; set; }
    public List<AlbumImage> Items { get; set; }
}
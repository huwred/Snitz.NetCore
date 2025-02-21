using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SnitzCore.Data.Extensions;

namespace SnitzCore.Data.Models
{
    [SnitzTable("BOOKMARKS", "MEMBER")]
    public class BookmarkEntry
    {
        [Column("BOOKMARK_ID")]
        [Key]
        public int Id { get; set; }
        [Column("B_TOPICID")]
        public int TopicId { get; set; }
        [Column("B_MEMBERID")]
        public int MemberId { get; set; }

        //[NotMapped]
        public virtual Post Topic { get; set; } = null!;

        //[NotMapped]
        public virtual Member Member { get; set; } = null!;
    }
}

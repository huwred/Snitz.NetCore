using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
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

        public virtual Post Topic { get; set; }
        public virtual Member? Author { get; set; }

    }
}

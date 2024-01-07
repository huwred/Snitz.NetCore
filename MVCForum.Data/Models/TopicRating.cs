using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("TOPIC_RATINGS","")]
public partial class TopicRating
{
    [Column("RATING")]
    [Key]
    public int Id { get; set; }

    [Column("RATINGS_BYMEMBERID")]
    public int RatingsBymemberId { get; set; }

    [Column("RATINGS_TOPIC_ID")]
    public int RatingsTopicId { get; set; }
}

using System;
using SnitzCore.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("PM","FORUM")]
public partial class PrivateMessage
{
    [Column("M_ID")]
    [Key]
    public int Id { get; set; }

    [Column("M_SUBJECT")]
    [StringLength(100)]
    [Required]
    public string Subject { get; set; } = null!;

    [Column("M_FROM")]
    public int FromId { get; set; }

    [Column("M_TO")]
    public int To { get; set; }

    [Column("M_SENT")]
    [StringLength(14)]
    public string? SentDate { get; set; }

    [Column("M_MESSAGE")]
    [Required]
    public string? Message { get; set; }

    [Column("M_PMCOUNT")]
    [StringLength(50)]
    public string? Pmcount { get; set; }

    [Column("M_READ")]
    public int Read { get; set; }

    [Column("M_MAIL")]
    [StringLength(50)]
    public string? Mail { get; set; }

    [Column("M_OUTBOX")]
    public short SaveSentMessage { get; set; }

    [Column("PM_DEL_FROM")]
    public int HideFrom { get; set; }

    [Column("PM_DEL_TO")]
    public int HideTo { get; set; }

    public virtual Member From { get; set; }
    [NotMapped]
    public virtual int? Notify { get; set; }
}

public enum SearchIn
{
    [Display(Name="Message")]
    Message,
    [Display(Name="Subject")]
    Subject
}


public class PrivateMessageListingModel
{
    public int Id { get; set; }

    public bool Read { get; set; }
    public DateTime? Sent { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public string? ToMemberName { get; set; }
    public string? FromMemberName { get; set; }
    public int ToMemberId { get; set; }
    public int FromMemberId { get; set; }
}
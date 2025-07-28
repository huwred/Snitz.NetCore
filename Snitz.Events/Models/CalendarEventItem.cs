using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;

namespace Snitz.Events.Models
{
    public class CalendarEventItemEntityConfiguration: IEntityTypeConfiguration<CalendarEventItem>
    {
        public void Configure(EntityTypeBuilder<CalendarEventItem> builder)
        {
            builder.HasIndex(e => e.Id).IsUnique();
        }
    }
    public class ClubCalendarCategoryEntityConfiguration: IEntityTypeConfiguration<ClubCalendarCategory>
    {
        public void Configure(EntityTypeBuilder<ClubCalendarCategory> builder)
        {
            builder.HasIndex(e => e.Id).IsUnique();
        }
    }
    public class ClubCalendarClubEntityConfiguration: IEntityTypeConfiguration<ClubCalendarClub>
    {
        public void Configure(EntityTypeBuilder<ClubCalendarClub> builder)
        {
            builder.HasIndex(e => e.Id).IsUnique();
        }
    }
    public class ClubCalendarLocationEntityConfiguration: IEntityTypeConfiguration<ClubCalendarLocation>
    {
        public void Configure(EntityTypeBuilder<ClubCalendarLocation> builder)
        {
            builder.HasIndex(e => e.Id).IsUnique();
        }
    }
    [SnitzTable("CAL_EVENTS", "")]
    public class CalendarEventItem 
    {
        [Column("C_ID")]
        [Key]
        public int Id { get; set; }
        [Column("TOPIC_ID")]
        public int TopicId { get; set; }
        [Column("EVENT_ALLDAY")]
        public bool IsAllDayEvent { get; set; }
        [Column("EVENT_DATE")]
        public string? Start { get; set; }
        [Column("EVENT_ENDDATE")]
        public string? End { get; set; }
        [Column("EVENT_RECURS")]
        public CalEnums.CalRecur Recurs { get; set; }
        [Column("EVENT_DAYS")]
        public string? RecurDays { get; set; }
        [Column("EVENT_TITLE")]
        public string? Title { get; set; }
        [Column("EVENT_DETAILS")]
        public string? Description { get; set; }
        [Column("DATE_ADDED")]
        public string? Posted { get; set; }

        [Column("CLUB_ID")]
        public int? ClubId { get; set; }
        [Column("CAT_ID")]
        public int? CatId { get; set; }
        [Column("LOC_ID")]
        public int? LocId { get; set; }
        [Column("AUTHOR_ID")]
        public int? AuthorId { get; set; }

        public virtual Member? Author { get; set; }

        public virtual Post? Topic { get; set; }

        [NotMapped]
        public DateTime? StartDate
        {
            get
            {
                return Start.FromForumDateStr();
            }
            set
            {
                Start = value.HasValue ? value.Value.ToForumDateStr() : null;
            }
        }
        [NotMapped]
        public DateTime? EndDate
        {
            get
            {
                return End.FromForumDateStr();
            }
            set
            {
                End = value.HasValue ? value.Value.ToForumDateStr() : null;
            }
        }

        [NotMapped]
        public virtual IEnumerable<string> SelectedDays { get; set; }

        public virtual ClubCalendarCategory Cat { get; set; }
        public virtual ClubCalendarClub Club { get; set; }
        public virtual ClubCalendarLocation Loc { get; set; }

        [NotMapped]
        public virtual int[] Days { get; set; }

    }



    public class ClubCalendarEventItem : CalendarEventItem
    {



        [NotMapped]
        public string CategoryName { get; set; }
        [NotMapped]
        public string ClubName { get; set; }
        [NotMapped]
        public string ClubAbbr { get; set; }
        [NotMapped]
        public string Location { get; set; }

        [NotMapped]
        public DateTime? PostedDate
        {
            get { return Posted.FromForumDateStr(); }
            set
            {
                Posted = value.HasValue ? value.Value.ToForumDateStr() : null;
            }
        }

        public ClubCalendarEventItem()
        {
            StartDate = DateTime.UtcNow;
            EndDate = DateTime.UtcNow.AddHours(1);
        }
    }

    [SnitzTable("EVENT_CAT", "")]
    public class ClubCalendarCategory 
    {
        [Column("CAT_ID")]
        [Key]
        public int Id { get; set; }

        [Column("CAT_NAME")]
        public String Name { get; set; }

        [Column("CAT_ORDER")]
        public int Order { get; set; }
    }

    [SnitzTable("EVENT_CLUB", "")]
    public class ClubCalendarClub 
    {
        [Column("CLUB_ID")]
        [Key]
        public int Id { get; set; }

        [Column("CLUB_L_NAME")]
        public String LongName { get; set; }
        [Column("CLUB_S_NAME")]
        public String ShortName { get; set; }
        [Column("CLUB_ABBR")]
        public String Abbreviation { get; set; }
        [Column("CLUB_ORDER")]
        public int Order { get; set; }
        [Column("CLUB_DEF_LOC")]
        public int DefLocId { get; set; }
    }

    [SnitzTable("EVENT_LOCATION", "")]
    public class ClubCalendarLocation 
    {
        [Column("LOC_ID")]
        [Key]
        public int Id { get; set; }

        [Column("LOC_NAME")]
        public String Name { get; set; }

        [Column("LOC_ORDER")]
        public int Order { get; set; }
    }

    [SnitzTable("EVENT_SUBSCRIPTIONS", "")]
    public class ClubCalendarSubscriptions 
    {
        [Column("SUB_ID")]
        [Key]
        public int Id { get; set; }

        [Column("CLUB_ID")]
        public int ClubId { get; set; }
        [Column("MEMBER_ID")]
        public int MemberId { get; set; }
    }

    public class CatSummary
    {
        public int CatId { get; set; }
        public string Name { get; set; }
        public int EventCount { get; set; }
    }
}
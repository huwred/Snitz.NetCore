using Microsoft.EntityFrameworkCore;

namespace Snitz.Events.Models
{

    public class EventContext : DbContext
    {

        public DbSet<CalendarEventItem> EventItems { get; set; }

        public EventContext(DbContextOptions<EventContext> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Essential to call the base method!

            modelBuilder.Entity<CalendarEventItem>();
            modelBuilder.Entity<ClubCalendarCategory>();
            modelBuilder.Entity<ClubCalendarClub>();
            modelBuilder.Entity<ClubCalendarLocation>();
            modelBuilder.Entity<ClubCalendarSubscriptions>();
        }
    }
}

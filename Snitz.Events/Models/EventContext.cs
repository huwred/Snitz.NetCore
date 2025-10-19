using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Snitz.Events.Models
{
    public class EventContextFactory : IDesignTimeDbContextFactory<EventContext>
    {
        public EventContext CreateDbContext(string[] args)
        {
            var path = System.IO.Path.Combine(System.Text.RegularExpressions.Regex.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\\bin$", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase), "App_Data", "SnitzForums2000.sqlite.db");

            var optionsBuilder = new DbContextOptionsBuilder<EventContext>();
            optionsBuilder.UseSqlite(path);

            return new EventContext(optionsBuilder.Options);
        }
    }

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

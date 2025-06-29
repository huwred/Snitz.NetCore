using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Snitz.Events.Models
{
    public static class Extensions
    {
        public static void AddEventsServices(this IServiceCollection serviceCollection,ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");

            serviceCollection.AddDbContext<EventContext>(
                options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.Events");}),ServiceLifetime.Transient
            );
            using (var scope = serviceCollection.BuildServiceProvider().CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<EventContext>())
                {
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        dbContext.Database.Migrate();
                    }
                }
            }
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

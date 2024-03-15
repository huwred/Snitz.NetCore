using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data;
using SnitzEvents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Snitz.Events.Models
{
    public static class Extensions
    {
        public static void AddEventsServices(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddTransient<SnitzDbContext, EventContext>();
            //serviceCollection.AddScoped<IRclService2, RclService2>();
        }
    }

    public class EventContext : SnitzDbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<CalendarEventItem> EventItems { get; set; }
        //public DbSet<AlbumGroup> AlbumGroups { get; set; }
        //public DbSet<AlbumCategory> AlbumCategories { get; set; }
        //public DbSet<ExtendedMember> ExtendedMembers { get; set; }
        public EventContext(){}
        public EventContext(DbContextOptions<SnitzDbContext> options,IServiceProvider serviceProvider,IConfiguration configuration) : base(options,serviceProvider)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                var connectionString = configuration.GetConnectionString("SnitzConnection");

                optionsBuilder.UseSqlServer(connectionString,
                    options => options.MigrationsAssembly("MSSqlMigrations"));
            }
            base.OnConfiguring(optionsBuilder);
        }

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

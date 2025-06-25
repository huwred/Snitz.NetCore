using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SnitzCore.Data;


namespace Snitz.PhotoAlbum.Models
{
    public static class Extensions
    {
        public static void AddAlbumServices(this IServiceCollection serviceCollection,ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");

            serviceCollection.AddDbContext<PhotoContext>(
                options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.PhotoAlbum");})
            );
            using (var scope = serviceCollection.BuildServiceProvider().CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<PhotoContext>())
                {
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        dbContext.Database.Migrate();
                    }
                }
            }
        }
    }
    public class PhotoContext : DbContext
    {

        public DbSet<AlbumImage> AlbumImages { get; set; }
        //public DbSet<AlbumGroup> AlbumGroups { get; set; }
        //public DbSet<AlbumCategory> AlbumCategories { get; set; }
        //public DbSet<ExtendedMember> ExtendedMembers { get; set; }
        public PhotoContext(DbContextOptions<PhotoContext> options) : base(options){} 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Essential to call the base method!

            modelBuilder.Entity<AlbumImage>();
            modelBuilder.Entity<AlbumGroup>();
            modelBuilder.Entity<AlbumCategory>();

        }
    }
}

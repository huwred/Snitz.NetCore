using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snitz.PostThanks.Models;


namespace Snitz.PhotoAlbum.Models
{
    public static class Extensions
    {
        public static void AddPostThanksServices(this IServiceCollection serviceCollection,ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");

            serviceCollection.AddDbContext<PostThanksContext>(
                options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.PostThanks");})
            );
            using (var scope = serviceCollection.BuildServiceProvider().CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<PostThanksContext>())
                {
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        dbContext.Database.Migrate();
                    }
                }
            }
        }
    }
    public class PostThanksContext : DbContext
    {

        public DbSet<PostThanksEntry> PostThanks { get; set; }

        public PostThanksContext(DbContextOptions<PostThanksContext> options) : base(options){} 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Essential to call the base method!

            modelBuilder.Entity<PostThanksEntry>().HasKey(e => new { e.MemberId, e.TopicId,e.ReplyId });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snitz.PhotoAlbum.Models;


namespace Snitz.PhotoAlbum.Helpers
{
    public static class Extensions
    {
        public static void AddAlbumServices(this IServiceCollection serviceCollection,ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");

            serviceCollection.AddDbContext<PhotoContext>(
                options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.PhotoAlbum");}),ServiceLifetime.Transient
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
}

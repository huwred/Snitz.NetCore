using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snitz.PostThanks.Models;


namespace Snitz.PostThanks.Helpers
{
    public static class Extensions
    {
        public static void AddPostThanksServices(this IServiceCollection serviceCollection,ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");

            serviceCollection.AddDbContext<PostThanksContext>(
                options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.PostThanks");}),ServiceLifetime.Transient
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
}

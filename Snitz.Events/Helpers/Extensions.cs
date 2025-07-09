using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snitz.Events.Models;

namespace Snitz.Events.Helpers
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
}

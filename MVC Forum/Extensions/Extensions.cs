using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCore.AutoRegisterDi;
using Snitz.Events.Models;
using Snitz.PhotoAlbum.Models;
using Snitz.PostThanks.Models;
using System;
using System.Linq;
using System.Reflection;


namespace MVCForum.Extensions
{
    public static class Extensions
    {
        public static void RegisterPlugins(this IServiceCollection serviceCollection,ConfigurationManager configuration)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Snitz.")).ToArray();
            var results = serviceCollection.RegisterAssemblyPublicNonGenericClasses(assemblies)
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces();

            serviceCollection.AddEventsServices(configuration);
            serviceCollection.AddAlbumServices(configuration);
            serviceCollection.AddPostThanksServices(configuration);
        }
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

using Microsoft.AspNetCore.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCore.AutoRegisterDi;
using SkiaSharp;
using Snitz.Events.Models;
using Snitz.PhotoAlbum.Models;
using Snitz.PostThanks.Models;
using System;
using System.Linq;


namespace MVCForum.Extensions
{
    public static class Extensions
    {


        public static void RegisterPlugins(this IServiceCollection serviceCollection,ConfigurationManager configuration, string datapath)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Snitz.")).ToArray();
            var results = serviceCollection.RegisterAssemblyPublicNonGenericClasses(assemblies)
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces();

            serviceCollection.AddEventsServices(configuration,datapath);
            serviceCollection.AddAlbumServices(configuration,datapath);
            serviceCollection.AddPostThanksServices(configuration,datapath);
        }
        public static void AddAlbumServices(this IServiceCollection serviceCollection,ConfigurationManager configuration, string datapath)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");
            var providor = configuration.GetConnectionString("DBProvider");
            switch (providor)
            {
                case "mssql" :
                        serviceCollection.AddDbContext<PhotoContext>(
                            options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.PhotoAlbum");}),ServiceLifetime.Transient
                        );
                    break;
                    case "sqlite":
                        serviceCollection.AddDbContext<PhotoContext>(
                            options => options.UseSqlite(connectionString?.Replace("|DataDirectory|",datapath),o => {o.MigrationsAssembly("Snitz.PhotoAlbum");}),ServiceLifetime.Transient
                        );
                    break;
                case "mysql" :
                        serviceCollection.AddDbContext<PhotoContext>(
                            options => options.UseMySql(connectionString,ServerVersion.AutoDetect(connectionString),o => {o.MigrationsAssembly("Snitz.PhotoAlbum");}),ServiceLifetime.Transient
                        );
                    break;
            }
        }
        public static void AddEventsServices(this IServiceCollection serviceCollection,ConfigurationManager configuration, string datapath)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");
            var providor = configuration.GetConnectionString("DBProvider");
            switch (providor)
            {
                case "mssql" :
                        serviceCollection.AddDbContext<EventContext>(
                            options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.Events");}),ServiceLifetime.Transient
                        );
                    break;
                    case "sqlite":
                        serviceCollection.AddDbContext<EventContext>(
                            options => options.UseSqlite(connectionString?.Replace("|DataDirectory|",datapath),o => {o.MigrationsAssembly("Snitz.Events");}),ServiceLifetime.Transient
                        );
                    break;
                case "mysql" :
                        serviceCollection.AddDbContext<EventContext>(
                            options => options.UseMySql(connectionString,ServerVersion.AutoDetect(connectionString),o => {o.MigrationsAssembly("Snitz.Events");}),ServiceLifetime.Transient
                        );
                    break;
            }
        }

        public static void AddPostThanksServices(this IServiceCollection serviceCollection,ConfigurationManager configuration, string datapath)
        {
            var connectionString = configuration.GetConnectionString("SnitzConnection");
            var providor = configuration.GetConnectionString("DBProvider");
            switch (providor)
            {
                case "mssql" :
                        serviceCollection.AddDbContext<PostThanksContext>(
                            options => options.UseSqlServer(connectionString,o => {o.UseCompatibilityLevel(120);o.MigrationsAssembly("Snitz.PostThanks");}),ServiceLifetime.Transient
                        );
                    break;
                    case "sqlite":
                        serviceCollection.AddDbContext<PostThanksContext>(
                            options => options.UseSqlite(connectionString?.Replace("|DataDirectory|", datapath),o => {o.MigrationsAssembly("Snitz.PostThanks");}),ServiceLifetime.Transient
                        );
                    break;
                case "mysql" :
                        serviceCollection.AddDbContext<PostThanksContext>(
                            options => options.UseMySql(connectionString,ServerVersion.AutoDetect(connectionString),o => {o.MigrationsAssembly("Snitz.PostThanks");}),ServiceLifetime.Transient
                        );
                    break;
            }
        }

        public static WebApplication AddImageAlbum(this WebApplication webApp)
        {
            using var scope = webApp.Services.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<PhotoContext>();
            try
            {
                if (appContext.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("Applying Photo Album Migrations");
                    appContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                //Log errors or do anything you think it's needed
                Console.WriteLine(ex.Message);
                //throw;
            }
            return webApp;
        }
        public static WebApplication AddEvents(this WebApplication webApp)
        {
            using var scope = webApp.Services.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<EventContext>();
            try
            {
                if (appContext.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("Applying Calendar/Event Migrations");
                    appContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                //Log errors or do anything you think it's needed
                Console.WriteLine(ex.Message);
                //throw;
            }
            return webApp;
        }
        public static WebApplication AddPostThanks(this WebApplication webApp)
        {
            using var scope = webApp.Services.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<PostThanksContext>();
            try
            {
                if (appContext.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("Applying PostThanks Migrations");
                    appContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                //Log errors or do anything you think it's needed
                Console.WriteLine(ex.Message);
                //throw;
            }
            return webApp;
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SnitzCore.Data;
using System;

namespace MVCForum.Extensions
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase(this WebApplication webApp)
        {

            using (var scope = webApp.Services.CreateScope())
            {
                using (var appContext = scope.ServiceProvider.GetRequiredService<SnitzDbContext>())
                {
                    try
                    {
                        appContext.Database.Migrate();

                    }
                    catch (Exception ex)
                    {
                        //Log errors or do anything you think it's needed
                        Console.WriteLine(ex.Message);
                        //throw;
                    }
                }
            }
            return webApp;
        }
    }
}

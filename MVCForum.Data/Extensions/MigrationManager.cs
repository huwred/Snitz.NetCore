using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace MVCForum.Extensions
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase(this WebApplication webApp)
        {
            //webApp.Configuration.GetSection("").GetChildren(""); 
            using var scope = webApp.Services.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<SnitzDbContext>();
            try
            {
                if (appContext.Database.GetPendingMigrations().Any())
                {
                    //Console.WriteLine("Perform Database Update");
                    appContext.Database.Migrate();
                }

                UploadCSV(webApp, appContext);
            }
            catch (Exception ex)
            {
                //Log errors or do anything you think it's needed
                //Console.WriteLine(ex.Message);
                //throw;
            }

            return webApp;
        }

        /// <summary>
        /// Imports language resource strings from a CSV file into the database.
        /// </summary>
        /// <remarks>This method reads a predefined CSV file containing language resource strings,
        /// processes its contents,  and updates or inserts the data into the database. If the file does not exist, the
        /// method logs a message  and exits without performing any operations. After a successful import, the file is
        /// renamed to prevent  re-importing it in the future. <para> If a resource with the same <c>Culture</c> and
        /// <c>ResourceId</c> already exists in the database,  it is updated with the new values. Otherwise, a new
        /// resource is added. The method uses a database  transaction to ensure that all changes are committed
        /// automatically. </para> <para> If an exception occurs during the import process, the error message is logged,
        /// and no changes are committed. </para></remarks>
        /// <param name="webApp">The <see cref="WebApplication"/> instance, used to determine the application's content root path.</param>
        /// <param name="dbContext">The <see cref="SnitzDbContext"/> instance used to interact with the database.</param>
        private static void UploadCSV(WebApplication webApp, SnitzDbContext dbContext)
        {
            
            var path = System.IO.Path.Combine(webApp.Environment.ContentRootPath, "App_Data");
            var filename = "initiallang_en.csv";

            if (!File.Exists(Path.Combine(path,filename)))
            {
                ////Console.WriteLine("Missing Language strings");
                return;
            }

            var csv = new CSVFiles(Path.Combine(path,filename), new[]
            {
                new DataColumn("pk", typeof(string)),
                new DataColumn("ResourceId", typeof(string)),
                new DataColumn("Value", typeof(string)),
                new DataColumn("Culture", typeof(string)),
                new DataColumn("Type", typeof(string)),
                new DataColumn("ResourceSet", typeof(string))
            });

            DataTable dt = csv.Table;

            try
            {
                //Console.WriteLine("Importing Language strings");
                using var transaction = dbContext.Database.BeginTransaction();
                foreach (DataRow row in dt.Rows)
                {
                    if (String.IsNullOrWhiteSpace(row.Field<string>("ResourceId")))
                        continue;
                    LanguageResource res = new LanguageResource
                    {
                        Name = row.Field<string>("ResourceId")!,
                        ResourceSet = row.Field<string>("ResourceSet"),
                        Value = row.Field<string>("Value"),
                        Culture = row.Field<string>("Culture")!
                    };

                    var existing = dbContext.LanguageResources.SingleOrDefault(l=>l.Culture == res.Culture && l.Name == res.Name);

                    if (existing != null && !existing.Equals(default(LanguageResource)))
                    {
                        if (res.ResourceSet !=null)
                        {
                            existing.Value = res.Value;
                            existing.ResourceSet = res.ResourceSet;
                            dbContext.LanguageResources.Update(existing);
                        }
                    }
                    else
                    {
                        dbContext.LanguageResources.Add(res);
                    }
                    dbContext.SaveChanges();
                }
                transaction.Commit();
                //rename the file so it doesn't get re-imported
                File.Move(Path.Combine(path,filename), Path.Combine(path,$"{filename}.done"));

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);

            }

        }

    }
}

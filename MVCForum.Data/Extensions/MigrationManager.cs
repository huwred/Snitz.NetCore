using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                appContext.Database.Migrate();
                UploadCSV(webApp, appContext);
            }
            catch (Exception ex)
            {
                //Log errors or do anything you think it's needed
                Console.WriteLine(ex.Message);
                //throw;
            }

            return webApp;
        }

    private static void UploadCSV(WebApplication webApp, SnitzDbContext dbContext)
    {
            var path = System.IO.Path.Combine(webApp.Environment.ContentRootPath, "App_Data");
            var filename = "initiallang_en.csv";

            if (!File.Exists(Path.Combine(path,filename)))
            {
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
                File.Move(Path.Combine(path,filename), Path.Combine(path,"export_en.done"));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }

    }

    }
}

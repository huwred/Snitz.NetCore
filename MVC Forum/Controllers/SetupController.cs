using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MVCForum.Extensions;
using MVCForum.ViewModels;
using MySql.Data.MySqlClient;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using SnitzCore.Data.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;


public class SetupController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IShutdownService _shutdownService;

    public SetupController(IWebHostEnvironment env,IConfiguration configuration,IHostApplicationLifetime appLifetime,IShutdownService shutdownService)
    {
        _env = env;
        _config = configuration;
        _appLifetime = appLifetime;
        _shutdownService = shutdownService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var vm = new DatabaseSetupViewModel();
        vm.ShowAdminForm = true;
        var connectionString = _config.GetConnectionString("SnitzConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            vm.IsConfigured = true;
            vm.Provider = _config.GetConnectionString("DBProvider");
            vm.ShowAdminForm = !AdminUserExists(connectionString,vm.Provider);

        }
        vm.ForumUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}/";
        vm.LandingPage = "AllForums";
        return View(vm);

    }


    /// <summary>
    /// Handles the submission of the database setup form, tests the database connection, and saves the connection
    /// settings.
    /// </summary>
    /// <remarks>
    /// SQL Server: Server=your_server_name;Database=your_database_name;User Id=your_username;Password=your_password;
    ///             Server=(localdb)\\mssqllocaldb;Database=SchoolDb;UserId=myuserid;Password=mypwd;
    /// MySQL:      Server=your_server_name;Database=your_database_name;User=your_username;Password=your_password;
    /// SQLite:     Data Source=your_database_file_path.db;
    /// 
    /// </remarks>
    /// <param name="model">The <see cref="DatabaseSetupViewModel"/> containing the database configuration details provided by the user.</param>
    /// <param name="action">The action to perform. If set to "test", the method tests the database connection; otherwise, it saves the
    /// connection settings.</param>
    /// <returns>An <see cref="IActionResult"/> that renders the view with the updated model if the connection test is performed
    /// or the model state is invalid. If the connection settings are successfully saved, redirects to the "Success"
    /// action.</returns>
    [HttpPost]
    public IActionResult Index(DatabaseSetupViewModel model,string action)
    {
        if(model.Provider == "localdb" || model.Provider == "sqlite")
        {
            ModelState.Remove("Server");
        }


        if (!ModelState.IsValid && !model.IsConfigured)
            return View(model);

        string connectionString = model.IsConfigured ? _config.GetConnectionString("SnitzConnection") : BuildConnectionString(model);

        if (action == "test")
        {
            try
            {
                switch (model.Provider)
                {
                    case "sqlite":
                        var path = Path.Combine(_env.ContentRootPath, "App_Data");
                        var connString = connectionString.Replace("|DataDirectory|", path);
                        using (var sqliteConnection = new SqliteConnection(connString))
                        {
                            sqliteConnection.Open();
                            sqliteConnection.Close();
                        }

                        break;
                        case "mysql":
                            using (var mySqlConnection = new MySqlConnection(connectionString))
                            {
                                mySqlConnection.Open();
                                mySqlConnection.Close();

                            }
                            break;
                        case "mssql":
                            case "localdb":
                            using (var connection = new SqlConnection(connectionString))
                            { connection.Open(); }
                            break;
                    default:
                        break;
                }

                model.TestResult = "✅ Connection successful!";
                model.ShowAdminForm = !model.IsConfigured || !AdminUserExists(connectionString,model.Provider);
            }
            catch (Exception ex)
            {
                model.TestResult = $"❌ Connection failed: {ex.Message}";
            }
            return View(model);
        }

        if (model.ShowAdminForm)
        {
            var hasher = new PasswordHasher<ForumUser>();
            var user = new ForumUser(); // or your custom IdentityUser class
            if(model.FEmail == null || model.FPassword == null || model.FUsername == null)
            {
                ModelState.AddModelError("FUsername", "Please provide details for the Forum Administrator account.");
                if(model.FEmail == null)
                {
                    ModelState.AddModelError("FEmail", "Please provide an Email address.");
                }
                if(model.FPassword == null)
                {
                    ModelState.AddModelError("FPassword", "You must provide a password for the Administrator.");
                }
                return View(model);
            }
            string hashedPassword = hasher.HashPassword(user, model.FPassword);

            SetupCacheProvider.GetOrCreate("AdminUser", () => new AdminUserInfo(){ UserName = model.FUsername, Email = model.FEmail, Password = hashedPassword }, TimeSpan.FromMinutes(10));
        }

        // load appsettings.json from disk into a dynamic object
        var configPath = Path.Combine(_env.ContentRootPath, "appsettings.json");
        var json = System.IO.File.ReadAllText(configPath);
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        
        jsonObj["ConnectionStrings"]["SnitzConnection"] = connectionString;
        jsonObj["ConnectionStrings"]["HangfireConnection"] = connectionString;
        jsonObj["ConnectionStrings"]["DBProvider"] = model.Provider == "localdb" ? "mssql" : model.Provider;

        jsonObj["SnitzForums"]["strVersion"] = model.Version;
        if(HttpContext.Request.PathBase.HasValue)
        {
            jsonObj["SnitzForums"]["strForumUrl"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";
            jsonObj["SnitzForums"]["VirtualPath"] = HttpContext.Request.PathBase.Value;
        }
        else
        {
            jsonObj["SnitzForums"]["strForumUrl"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            jsonObj["SnitzForums"]["VirtualPath"] = "";
        }
        if(!string.IsNullOrWhiteSpace(model.LandingPage)){
            jsonObj["SnitzForums"]["LandingPage"] = model.LandingPage;
        } else
        {
            jsonObj["SnitzForums"]["LandingPage"] = "";
        }

        // save back to disk
        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        System.IO.File.WriteAllText(configPath, output);

        // Optionally redirect to restart or notify user 
        return RedirectToAction("Success", new {provider = model.Provider,connectionString = connectionString});
    }
    public async Task<IActionResult> SuccessAsync(string provider, string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SnitzDbContext>();
        var appdataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        optionsBuilder.UseDatabase(provider == "localdb" ? "mssql" : provider,connectionString,appdataPath);
        using var context = new SnitzDbContext(optionsBuilder.Options);
        //Console.WriteLine("Performing Database Upgrade");
        await context.Database.MigrateAsync();
        //Console.WriteLine("Upgrade complete");
        return View();
    }
    
    /// <summary>
    /// Determines whether an administrator user exists in the forum database.
    /// </summary>
    /// <remarks>An administrator user is identified by a membership level of 3 in the <c>[FORUM_MEMBERS]</c>
    /// table.</remarks>
    /// <param name="connectionString">The connection string used to connect to the database.</param>
    /// <param name="provider">The database provider to use. Supported values are <c>"mssql"</c>, <c>"mysql"</c>, and <c>"sqlite"</c>.</param>
    /// <returns><see langword="true"/> if at least one administrator user exists in the database; otherwise, <see
    /// langword="false"/>.</returns>
    private bool AdminUserExists(string connectionString, string? provider)
    {
        try
        {
            string sql = "select count([MEMBER_ID]) from [FORUM_MEMBERS] where [M_LEVEL]=3";
            switch (provider)
            {
                case "mssql" :
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = sql;
                        var count = cmd.ExecuteScalar();
                        conn.Close();
                        return count != null ? (int)count > 0 : false;
                    }
                case "mysql" :
                     sql = "select count(MEMBER_ID) from FORUM_MEMBERS where M_LEVEL=3";
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = sql;
                        var count = cmd.ExecuteScalar();
                        conn.Close();
                        return count != null ? (long)count > 0 : false;
                    }
                case "sqlite" : 
                    using (var conn = new SqliteConnection(connectionString))
                    {
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = sql;
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        conn.Close();
                        return count > 0;
                    }
                default:
                    break;
            }
        }
        catch (Exception)
        {
            return false;
        }


        return false;
    }

    /// <summary>
    /// Builds a connection string based on the specified database provider and configuration settings.
    /// </summary>
    /// <remarks>The method supports the following database providers: <list type="bullet"> <item>
    /// <term>Sqlite</term> <description>Generates a connection string for SQLite with shared caching, foreign key
    /// support, and pooling enabled.</description> </item> <item> <term>mysql</term> <description>Generates a
    /// connection string for MySQL with server, database, username, and password details, along with pooling and a
    /// 30-second timeout.</description> </item> <item> <term>mssql</term> <description>Generates a connection string
    /// for Microsoft SQL Server, supporting options for encryption, trusted server certificates, and integrated
    /// security.</description> </item> <item> <term>localdb</term> <description>Generates a connection string for a
    /// LocalDB instance, attaching the database file from the application's "App_Data" directory.</description> </item>
    /// </list></remarks>
    /// <param name="model">An instance of <see cref="DatabaseSetupViewModel"/> containing the database configuration details, such as
    /// provider, server, database name, and authentication settings.</param>
    /// <returns>A connection string formatted for the specified database provider. Returns an empty string if the provider is
    /// not recognized.</returns>
    private string BuildConnectionString(DatabaseSetupViewModel model)
    {
        switch (model.Provider)
        {
            case "sqlite":
                return $"Data Source=|DataDirectory|\\{model.Database}.db;Cache=Shared;Foreign Keys=True;Pooling=True";
            case "mysql":
                return $"Server={model.Server};Database={model.Database};Uid={model.Username};Pwd={model.Password};Pooling=True;Connection Timeout=30;SslMode=none";
            case "mssql":
                var encryption = model.UseEncryption ? "Encrypt=True;" : "Encrypt=False;";
                
                var trustcertificate = model.TrustServerCertificate ? "TrustServerCertificate=true;" : "";
                var integratedSecurity = model.UseIntegratedSecurity ? "Trusted_Connection=True;" : $"User Id={model.Username};Password={model.Password};";
                return $"Server={model.Server};Database={model.Database};MultipleActiveResultSets=true;{integratedSecurity}{encryption}{trustcertificate}";
            case "localdb":
                var appdataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                return $"Server=(localdb)\\MSSQLLocalDB;AttachDbFileName={appdataPath}\\{model.Database}.mdf;MultipleActiveResultSets=true;Trusted_Connection=True";
            default:
                break;
        }
        return "";
    }
}


using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MVCForum.Extensions;
using MVCForum.ViewModels;
using SnitzCore.Data;
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
        var connectionString = _config.GetConnectionString("SnitzConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            vm.IsConfigured = true;
            vm.Provider = _config.GetConnectionString("DBProvider");

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
        //// Cache a default admin user for use in the migration - check if the member table exists first?
        // var adminUser =
        //CacheProvider.GetOrCreate("AdminUser", () => new AdminUserInfo(){ UserName = "Adminstrator", Email = "xxx@test.com", Password = "Passw0rd!" }, TimeSpan.FromMinutes(10));

        if (!ModelState.IsValid && !model.IsConfigured)
            return View(model);

        string connectionString = model.IsConfigured ? _config.GetConnectionString("SnitzConnection") : BuildConnectionString(model);

        if (action == "test")
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                model.TestResult = "✅ Connection successful!";
            }
            catch (Exception ex)
            {
                model.TestResult = $"❌ Connection failed: {ex.Message}";
            }

            return View(model);
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

        optionsBuilder.UseDatabase(provider == "localdb" ? "mssql" : provider,connectionString,"");
        using var context = new SnitzDbContext(optionsBuilder.Options);

         await context.Database.MigrateAsync();

        return View();
    }

    private string BuildConnectionString(DatabaseSetupViewModel model)
    {

        switch (model.Provider)
        {
            case "Sqlite":
                return $"Data Source=|DataDirectory|\\{model.Database}.db;Cache=Shared;Foreign Keys=True;Pooling=True";
            case "mysql":
                return $"Server={model.Server};Database={model.Database};Uid={model.Username};Pwd={model.Password};Pooling=True;Connection Timeout=30";
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

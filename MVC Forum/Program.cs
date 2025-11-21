using BbCodeFormatter;
using BbCodeFormatter.Processors;
using Hangfire;
using KestrelWAF;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MVCForum.Extensions;
using MVCForum.Security;
using NUglify.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SmartBreadcrumbs.Extensions;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using SnitzCore.Service.Extensions;
using SnitzCore.Service.Hangfire;
using SnitzCore.Service.MiddleWare;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using YG.ASPNetCore.FileManager;

WebApplication? app;
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var connectionString = config.GetConnectionString("SnitzConnection");
var version = config.GetSection("SnitzForums")["strVersion"];

var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
builder.Services.AddSingleton<IXmlFaqService>(new XmlFaqService(dataDir));

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)       
    .AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (string.IsNullOrEmpty(version))
{
    builder.Services.AddRazorPages();
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
    builder.Services.AddSingleton<IShutdownService, ShutdownService>();

    app = builder.Build();
    app.UseStaticFiles();
    app.UseRouting();

    app.MapControllerRoute(
    name: "setup",
    pattern: "{controller=Setup}/{action=Index}/{id?}");

    app.MapRazorPages();
    app.Run();

    return;

}

builder.Services.Configure<SnitzForums>(builder.Configuration.GetSection("SnitzForums"));
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddDbContext<SnitzDbContext>(options =>
{
    options.ConfigureWarnings(warnings => warnings
        .Ignore(CoreEventId.FirstWithoutOrderByAndFilterWarning)
        .Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS)
        .Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning)
    );
    options.UseDatabase(builder.Configuration.GetConnectionString("DBProvider"), builder.Configuration, System.IO.Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableDetailedErrors();

},ServiceLifetime.Scoped);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//Set Values by default
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartBoundaryLengthLimit = int.MaxValue;
    x.MultipartHeadersCountLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddDefaultIdentity<ForumUser>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        options.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 4;

    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SnitzDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<EmailConfirmationTokenProvider<ForumUser>>("emailconfirmation")
    .AddPasswordValidator<CustomPasswordValidator<ForumUser>>();

builder.Services.AddScoped<IPasswordHasher<IdentityUser>, CustomPasswordHasher>();
builder.Services.Configure<IdentityOptions>(builder.Configuration.GetSection(nameof(IdentityOptions)));
builder.Services.ConfigureApplicationCookie(options =>
{
    //Location for your Custom Access Denied Page
    options.AccessDeniedPath = "/Account/Login";
    //Location for your Custom Login Page
    options.LoginPath = "/Account/Login";

    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    // Cookie settings
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;


});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential 
    // cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.ConsentCookieValue = "true";
});
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromHours(12));
builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromDays(14));
builder.Services.AddTransient<ISnitzCookie, SnitzCookie>();

builder.Services.AddScoped<ICategory, CategoryService>();
builder.Services.AddScoped<IMember, MemberService>();
builder.Services.AddScoped<IForum, ForumService>();
builder.Services.AddScoped<IPost, PostService>();
builder.Services.AddScoped<IPrivateMessage, PrivateMessageService>();
builder.Services.AddScoped<IBookmark, BookmarkService>();
builder.Services.AddScoped<ISubscriptions, ProcessSubscriptions>();
builder.Services.AddScoped<IEmoticon, EmoticonService>();
builder.Services.AddScoped<ISnitz, SnitzService>();
builder.Services.AddScoped<IGroups, GroupService>();
builder.Services.AddTransient<ISnitzConfig, ConfigService>();
builder.Services.AddTransient<ICodeProcessor, BbCodeProcessor>();
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IHtmlLocalizerFactory, EFStringLocalizerFactory>();
builder.Services.AddTransient<IPasswordPolicyService, PasswordPolicyService>();
builder.Services.AddTransient<IAdRotator, BannerService>();
builder.Services.AddScoped<SignInManager<ForumUser>, CustomSignInManager>();
builder.Services.AddMvc().AddViewLocalization();
if(!string.IsNullOrWhiteSpace(builder.Configuration["SnitzForums:VisitorTracking"]))
{
    builder.Services.AddScoped<ILogRepository, VisitorLogRepository>();
    builder.Services.AddSingleton<ILoggerService, DatabaseLoggerService>();
    builder.Services.AddHostedService(sp => sp.GetService<ILoggerService>() as DatabaseLoggerService);
}


builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddYGfilemanager();
builder.Services.AddResponseCaching();
builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
    //options.DontLookForDefaultNode = builder.Configuration["SnitzForums:LandingPage"] != "";
    options.TagName = "nav";
    options.TagClasses = "";
    options.OlClasses = "breadcrumb";
    options.LiClasses = "breadcrumb-item";
    options.ActiveLiClasses = "breadcrumb-item active";
    options.ResourceType = typeof(LanguageService);
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(13);
    options.Cookie.Name = ".snitzCore.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddSingleton(builder.Configuration.GetSection("MailSettings").Get<EmailConfiguration>()!);
builder.Services.Configure<SnitzForums>(builder.Configuration.GetSection(SnitzForums.SectionName));
builder.Services.AddHttpClient();
builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");
builder.Services.AddImageSharp(
    options =>
    {
        options.OnParseCommandsAsync = c =>
        {
            if (c.Commands.Count == 0)
            {
                return Task.CompletedTask;
            }

            uint width = c.Parser.ParseValue<uint>(
                c.Commands.GetValueOrDefault(ResizeWebProcessor.Width),
                c.Culture);

            uint height = c.Parser.ParseValue<uint>(
                c.Commands.GetValueOrDefault(ResizeWebProcessor.Height),
                c.Culture);

            List<Size> allowedSizes = new List<Size> { new Size(1920, 1080), new Size(1080, 1920) };
            if (!allowedSizes.Any(x => x.Width >= width && x.Height >= height))
            {
                c.Commands.Remove(ResizeWebProcessor.Width);
                c.Commands.Remove(ResizeWebProcessor.Height);
            }

            return Task.CompletedTask;
        };
    });

//var path = System.IO.Path.Combine(builder.Environment.ContentRootPath, "App_Data");

builder.Services.RegisterPlugins(builder.Configuration,dataDir);


builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_110)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .SetStorage(builder.Configuration.GetConnectionString("HangfireConnection").Replace("|DataDirectory|",dataDir), builder.Configuration.GetConnectionString("DBProvider"))
    );
builder.Services.AddHangfireServer();
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.MaxAge = TimeSpan.FromDays(160);

});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// If using IIS:
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(builder.Environment.ContentRootFileProvider);

builder.Services.AddDataProtection()
.PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDir, @"dpkeys")))
.SetApplicationName("snitz-core");

    builder.Services.Configure<MicroRuleEngine.Rule>(opt => builder.Configuration.GetSection("Configuration:Ruleset").Bind(opt));
    builder.Services.AddSingleton(new MaxMindDb(Path.Combine(dataDir,builder.Configuration["Configuration:GeoLiteFile"])));
builder.Services.AddDetection();
app = builder.Build();

app.MigrateDatabase();
app.AddPostThanks();
app.AddEvents();
app.AddImageAlbum();
app.UseMultiLanguages(builder.Configuration.GetSection(SnitzForums.SectionName),app.Services.GetRequiredService <IOptions<RequestLocalizationOptions>>().Value);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseDeveloperExceptionPage();
    //app.UseExceptionHandler();
}

//Pi doesn't like this, could be newt!
app.UseHttpsRedirection();

app.UseImageSharp();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = (context) =>
    {
        var headers = context.Context.Response.GetTypedHeaders();

        headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(33)
        };
    }
});
app.UseYGfilemanager("");//builder.Configuration["SnitzForums:VirtualPath"]
app.UseRouting();
app.UseStatusCodePages(async context => {
    var request = context.HttpContext.Request;
    var response = context.HttpContext.Response;
    //using System.Net;
    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
    // you may also check requests path to do this only for specific methods       
    // && request.Path.Value.StartsWith("/specificPath")
    {
        response.Redirect(request.PathBase + "/Account/Login");  //redirect to the login page.
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseSession();
if (!string.IsNullOrWhiteSpace(builder.Configuration["Configuration:Enabled"]) && builder.Configuration.GetValue<bool>("Configuration:Enabled"))
{
    app.UseMiddleware<BlockIpMiddleware>();
}
app.UseOnlineUsers();
if (!string.IsNullOrWhiteSpace(builder.Configuration["SnitzForums:VisitorTracking"]))
{
    app.UseMiddleware<VisitorTrackingMiddleware>();
}
app.UseCookiePolicy();
app.UseHangfireDashboard("/snitzjobs",new DashboardOptions
{
    AppPath = builder.Configuration["SnitzForums:strForumUrl"],
    Authorization = new [] { new SnitzAuthorizationFilter() }
});
app.UseResponseCaching();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
//Add a Job to clear down old log files
app.CreateJob(builder.Configuration.GetSection("SnitzForums"),builder.Environment.ContentRootPath);


app.Run();

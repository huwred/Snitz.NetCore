using BbCodeFormatter;
using BbCodeFormatter.Processors;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
using NetCore.AutoRegisterDi;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;



var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)       
    .AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

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

},ServiceLifetime.Transient);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ForumUser>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedEmail = true;
        options.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 3;

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
#region localization

builder.Services.ConfigureOptions<SnitzRequestLocalizationOptions>();
builder.Services.AddMvc().AddViewLocalization();

#endregion
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddResponseCaching();
builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
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
EmailConfiguration emailConfig = builder.Configuration
    .GetSection("MailSettings")
    .Get<EmailConfiguration>()!;
builder.Services.AddSingleton(emailConfig);
builder.Services.Configure<SnitzForums>(builder.Configuration.GetSection(SnitzForums.SectionName));

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
builder.Services.AddTransient<ISyndicationXmlService, SyndicationXmlService>();
builder.Services.RegisterPlugins(builder.Configuration);


builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_110)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .SetStorage(builder.Configuration.GetConnectionString("HangfireConnection"),builder.Configuration.GetConnectionString("DBProvider"))
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

var app = builder.Build();

app.MigrateDatabase();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}
app.UseHttpsRedirection();

app.UseRequestLocalization(app.Services.GetRequiredService < IOptions < RequestLocalizationOptions >> ().Value);
app.UseImageSharp();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseOnlineUsers();
app.UseCookiePolicy();
app.UseHangfireDashboard("/snitzjobs",new DashboardOptions
{
    Authorization = new [] { new SnitzAuthorizationFilter() }
});
app.UseResponseCaching();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

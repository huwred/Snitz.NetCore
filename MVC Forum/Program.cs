using BbCodeFormatter;
using BbCodeFormatter.Processors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVCForum.Extensions;
using MVCForum.MiddleWare;
using SmartBreadcrumbs.Extensions;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<SnitzDbContext>(options =>
{
    options.UseDatabase(builder.Configuration.GetConnectionString("DBProvider"), builder.Configuration, System.IO.Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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
builder.Services.Configure<IdentityOptions>(builder.Configuration.GetSection(nameof(IdentityOptions)));
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromHours(2));
builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromDays(7));
builder.Services.AddOutputCache(options =>
{
    // Add a base policy that applies to all endpoints
    options.AddBasePolicy(basePolicy => basePolicy.Expire(TimeSpan.FromSeconds(120)));

    // Add a named policy that applies to selected endpoints
    options.AddPolicy("Expire20", policyBuilder => policyBuilder.Expire(TimeSpan.FromMinutes(20)));
});
builder.Services.ConfigureApplicationCookie(options =>
{
    //Location for your Custom Access Denied Page
    options.AccessDeniedPath = "/Account/Login";

    //Location for your Custom Login Page
    options.LoginPath = "/Account/Login";

    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = true;

});
builder.Services.AddScoped<ICategory, CategoryService>();
builder.Services.AddScoped<IMember, MemberService>();
builder.Services.AddScoped<IForum, ForumService>();
builder.Services.AddScoped<IPost, PostService>();
builder.Services.AddScoped<IPrivateMessage, PrivateMessageService>();
builder.Services.AddScoped<ILanguageResource, LanguageService>();
builder.Services.AddScoped<ISnitzConfig, ConfigService>();
builder.Services.AddScoped<ICodeProcessor, BbCodeProcessor>();
builder.Services.AddScoped<IEmoticon, EmoticonService>();
builder.Services.AddScoped<ISnitz, SnitzService>();
builder.Services.AddScoped<ISnitzCookie, SnitzCookie>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddControllersWithViews();
builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
    options.TagName = "nav";
    options.TagClasses = "";
    options.OlClasses = "breadcrumb";
    options.LiClasses = "breadcrumb-item";
    options.ActiveLiClasses = "breadcrumb-item active";
});
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var emailConfig = builder.Configuration
    .GetSection("MailSettings")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
//builder.Services.AddRazorPages();
//builder.Services.AddTransient<IStartupFilter,
//    RequestSetOptionsStartupFilter>();
var app = builder.Build();
app.MigrateDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePages(context => {
    var request = context.HttpContext.Request;
    var response = context.HttpContext.Response;

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
    {
        response.Redirect("/Account/Login");
    }

    return Task.CompletedTask;
});

app.UseStaticFiles();
app.UseRouting();
//app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
//app.UseDatabaseErrorPage();

app.Run();

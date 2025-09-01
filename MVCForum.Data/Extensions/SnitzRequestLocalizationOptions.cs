using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Globalization;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MVCForum.Extensions;

//public class SnitzRequestLocalizationOptions : IConfigureOptions<RequestLocalizationOptions>
//{
//    public void Configure(RequestLocalizationOptions options)
//    {

//        var supportedCultures = new List<CultureInfo>
//        {
//            new CultureInfo("en"),
//            new CultureInfo("no"),
//            new CultureInfo("fa")
//        };

//        options.DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB");
//        options.SupportedCultures = supportedCultures;
//        options.SupportedUICultures = supportedCultures;
//        options.RequestCultureProviders.Insert(0, new SnitzCultureProvider(options));
//    }

//}
public static class SnitzRequestLocalizationOptionsExtensions
{
    public static IApplicationBuilder UseMultiLanguages(this IApplicationBuilder app,IConfigurationSection config,RequestLocalizationOptions options)
    {
        var langs = config.GetSection("SupportedLanguages");
        var configs = langs.Get<string[]>();

        if (configs != null && configs.Length > 0)
        {
            var supportedCultures = new List<CultureInfo>();
            foreach (var culture in configs)
            {
                supportedCultures.Add(new CultureInfo(culture));
            }
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        }
            
        options.DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB");
        options.RequestCultureProviders.Insert(0, new SnitzCultureProvider(options));

        app.UseRequestLocalization(options);            

        return app;
    }
}
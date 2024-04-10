using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace MVCForum.Extensions;

public class SnitzRequestLocalizationOptions : IConfigureOptions<RequestLocalizationOptions>
{
    public void Configure(RequestLocalizationOptions options)
    {
        // TODO: Configure other options parameters
        var supportedCultures = new List<CultureInfo>
        {
            new CultureInfo("en"),
            new CultureInfo("no")
        };

        options.DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
        // Add the custom provider,
        // in many cases you'll want this to execute before the defaults
        options.RequestCultureProviders.Insert(0, new SnitzCultureProvider(options));
    }
}
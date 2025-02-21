using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace MVCForum.Extensions;

public class SnitzCultureProvider : RequestCultureProvider
{
    private readonly RequestLocalizationOptions _localizationOptions;
    private readonly object _locker = new object();

    // ctor with reference to the RequestLocalizationOptions
    public SnitzCultureProvider(RequestLocalizationOptions localizationOptions)
    {
        _localizationOptions = localizationOptions;

    } 

    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        // TODO: Implement GetCulture() to get a culture for the current request
        CultureInfo? culture = GetCulture(); 

        if (culture is null)
        {
            return await NullProviderCultureResult;
        }

        lock (_locker)
        {
            // check if this culture is already supported
            bool cultureExists = _localizationOptions.SupportedCultures?.Contains(culture) ?? false;

            if (!cultureExists)
            {
                // If not, add this as a supporting culture
                _localizationOptions.SupportedCultures?.Add(culture);
                _localizationOptions.SupportedUICultures?.Add(culture);
            } 
        }
        var providerResultCulture = new ProviderCultureResult(culture.Name);
        return await Task.FromResult(providerResultCulture);
    }

    private CultureInfo? GetCulture()
    {
        var cookielang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (cookielang != null)
        {
            return new CultureInfo(cookielang);
        }

        return null;
    }
}
// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Service
{
    public class LanguageService : IHtmlLocalizer
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly ISnitzCookie _cookie;
        private readonly string? _language;

        public LanguageService(IServiceProvider serviceProvider,ISnitzCookie cookie)
        {
            _serviceProvider = serviceProvider;
            _cookie = cookie;
            var cookielang = cookie.GetCookieValue("CookieLang");
            bool isRighToLeft = false;
            if (cookielang != null)
            {
                var cultureInfo = new CultureInfo(cookielang);
                CultureInfo.CurrentUICulture = cultureInfo;
                _language = cultureInfo.TwoLetterISOLanguageName;
            }
        }

        public LocalizedHtmlString this[string name]
        {
            get
            {
                var value = GetString(name);
                return new LocalizedHtmlString(name, value ?? name);
            }
        }

        public LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                var format = GetString(name);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedHtmlString(name, value);
            }
        }

        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            return new LanguageService(_serviceProvider, _cookie);
            
        }

        public LocalizedString GetString(string name, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            if (culture.StartsWith("en-"))
            {
                culture = "en";
            }

            if (_language != null)
            {
                culture = _language;
            }
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<SnitzDbContext>();
            var results = context!.LanguageResources
                .Where(r => r.Culture == culture)
                .Select(r => new LocalizedString(r.Name, r.Value));
            return results;
        }

        public LocalizedString GetString(string name)
        {
            
            var culture = CultureInfo.CurrentUICulture.Name;
            if (culture.StartsWith("en-"))
            {
                culture = "en";
            }

            if (_language != null)
            {
                culture = _language;
            }
            var service = new InMemoryCache(30);
            return service.GetOrSet($"{culture}_{name}", () => CachedStringValue(name,culture));

        }

        private LocalizedString CachedStringValue(string name, string culture)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<SnitzDbContext>();
            var result = context!.LanguageResources
                .FirstOrDefault(r => r.Name == name && r.Culture == culture)?.Value;
            return new LocalizedString(name,result ?? name);
        }
    }
}

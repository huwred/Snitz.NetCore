// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SnitzCore.Data;

namespace SnitzCore.Service
{
    public class LanguageService : IHtmlLocalizer
    {

        private readonly IServiceProvider _serviceProvider;

        public LanguageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

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
            return new LanguageService(_serviceProvider);
            
        }

        public LocalizedString GetString(string name, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            var culture = CultureInfo.CurrentCulture.Name;
            if (culture.StartsWith("en-"))
            {
                culture = "en";
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
            var culture = CultureInfo.CurrentCulture.Name;
            if (culture.StartsWith("en-"))
            {
                culture = "en";
            }
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<SnitzDbContext>();
            var result = context!.LanguageResources
                .FirstOrDefault(r => r.Name == name && r.Culture == culture)?.Value;
            return new LocalizedString(name,result ?? name);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using Microsoft.AspNetCore.Mvc.Localization;

namespace SnitzCore.Service
{
    public class EFStringLocalizerFactory : IHtmlLocalizerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public EFStringLocalizerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IHtmlLocalizer Create(Type resourceSource)
        {
            return new LanguageService(_serviceProvider);
        }

        public IHtmlLocalizer Create(string baseName, string location)
        {
            return new LanguageService(_serviceProvider);
        }
    }
}

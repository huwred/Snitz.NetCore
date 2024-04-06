// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Service
{
    public class EFStringLocalizerFactory : IHtmlLocalizerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISnitzCookie _cookie;

        public EFStringLocalizerFactory(IServiceProvider serviceProvider,ISnitzCookie cookie)
        {
            _serviceProvider = serviceProvider;
            _cookie = cookie;
        }

        public IHtmlLocalizer Create(Type resourceSource)
        {
            return new LanguageService(_serviceProvider,_cookie);
        }

        public IHtmlLocalizer Create(string baseName, string location)
        {
            return new LanguageService(_serviceProvider, _cookie);
        }
    }
}

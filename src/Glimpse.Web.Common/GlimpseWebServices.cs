﻿using System;
using System.Collections.Generic;
using Glimpse.Web;
using Glimpse.Web.Common;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection;

namespace Glimpse
{
    public class GlimpseWebServices
    {
        public static IServiceCollection GetDefaultServices()
        {
            var services = new ServiceCollection();
             
            services.AddTransient<IRequestAuthorizerProvider, DefaultRequestAuthorizerProvider>();
            services.AddTransient<IMiddlewareResourceComposerProvider, DefaultMiddlewareResourceComposerProvider>();
            services.AddTransient<IMiddlewareLogicComposerProvider, DefaultMiddlewareLogicComposerProvider>();
            services.AddTransient<IMvcRazorHost, GlimpseRazorHost>(); //TODO: This probably doesn't belong here.

            return services;
        }
    }
}
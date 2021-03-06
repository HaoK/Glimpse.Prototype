﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace Glimpse.Server.Web
{
    public interface IResourceBuilder
    {
        IApplicationBuilder AppBuilder { get; }

        IResourceBuilder Run(string name, string uriTemplate, Func<HttpContext, IDictionary<string, string>, Task> resource);
    }
}
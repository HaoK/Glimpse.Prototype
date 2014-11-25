﻿using Glimpse.Web;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Glimpse.Server.Resources
{
    public class AgentHttpResource : IRequestHandler
    {
        public bool WillHandle(IContext context)
        {
            return context.Request.Path.StartsWith("/Glimpse/Agent");
        }

        public async Task Handle(IContext context)
        {
            var response = context.Response;

            response.SetHeader("Content-Type", "text/plain");

            var data = Encoding.UTF8.GetBytes("Hello world, Glimpse!");
            await response.WriteAsync(data);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Utilitywarehouse.OpsEndpoints.Owin
{
    public class OpsEndpointsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly OpsEndpointsMiddlewareOptions _options;
        private readonly Dictionary<string, Func<HttpContext, Task>> handlers;

        public OpsEndpointsMiddleware(RequestDelegate next, OpsEndpointsMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public Task Invoke(HttpContext context)
        {
            PathString matching;
            PathString segment;
            if (context.Request.Path.StartsWithSegments("/_/", out matching, out segment))
            {
                Func<HttpContext, Task> handler;
                if (handlers.TryGetValue(segment, out handler))
                {
                    return handler.Invoke(context);
                }
            }
            return this._next.Invoke(context);
        }
    }

    public class OpsEndpointsMiddlewareOptions
    {
    }
}
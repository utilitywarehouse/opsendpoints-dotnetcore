using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Utilitywarehouse.OpsEndpoints.DTO;

using HttpHandler = System.Func<Microsoft.AspNetCore.Http.HttpContext, System.Threading.Tasks.Task>;

namespace Utilitywarehouse.OpsEndpoints.Owin
{
    [UsedImplicitly]
    public class OpsEndpointsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializer _serializer;
        private readonly Dictionary<string, HttpHandler> _handlers;
        private readonly JsonSerializerSettings _serializerSettings;

        public OpsEndpointsMiddleware(RequestDelegate next, OpsEndpointsMiddlewareOptions options)
        {
            _next = next;
            _handlers = ConfigureHandlers(options);
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        private Dictionary<string, HttpHandler> ConfigureHandlers(OpsEndpointsMiddlewareOptions options)
        {
            var ready = new HttpHandler(context =>
            {
                var op = options;
                const string readyBody = "ready\n";
                if (op.HealthModel.Ready())
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/plain";
                    return context.Response.WriteAsync(readyBody);
                }

                context.Response.StatusCode = (int) HttpStatusCode.ServiceUnavailable;
                return Task.CompletedTask;
            });

            var about = new HttpHandler(context =>
            {
                var op = options;
                AboutResponse response = op.HealthModel.About().ToAboutResponse();
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";

                return context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            });

            var health = new HttpHandler(context =>
            {
                var op = options;
                HealthResponse response = op.HealthModel.Health().ToHealthResponse();
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";

                return context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            });

            return new Dictionary<string, HttpHandler>
            {
                {"/about", about},
                {"/health", health},
                {"/ready", ready}
            };
        }

        public Task Invoke(HttpContext context)
        {
            PathString segment = null;

            if (context.Request.Path.StartsWithSegments("/__", out segment))
            {
                Func<HttpContext, Task> handler;
                
                if (_handlers.TryGetValue(segment, out handler))
                {
                    return handler.Invoke(context);
                }

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Task.CompletedTask;
            }

            return this._next.Invoke(context);
        }
    }
}
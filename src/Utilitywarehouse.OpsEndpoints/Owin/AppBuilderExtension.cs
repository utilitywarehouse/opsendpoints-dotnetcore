using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Utilitywarehouse.OpsEndpoints.Owin
{
    public static class AppBuilderExtension
    {
        [UsedImplicitly]
        public static IApplicationBuilder UseOpsEndpoints(this IApplicationBuilder builder, OpsEndpointsMiddlewareOptions options)
        {
            return builder.UseMiddleware<OpsEndpointsMiddleware>(options);
        }
    }
}
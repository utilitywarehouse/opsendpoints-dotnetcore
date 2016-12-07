using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Utilitywarehouse.OpsEndpoints.Owin;
using Xunit;

namespace Utilitywarehouse.OpsEndpoints.Tests
{
    public class MiddleWareTests
    {
        [Fact]
        public void Test1()
        {
            var config = new OpsEndpointsMiddlewareOptions();
            var server = new TestServer(new WebHostBuilder().Configure(app => app.UseOpsEndpoints(config)));
        }
    }
}

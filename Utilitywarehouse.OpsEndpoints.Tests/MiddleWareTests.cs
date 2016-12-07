using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Utilitywarehouse.OpsEndpoints.Owin;
using Xunit;

namespace Utilitywarehouse.OpsEndpoints.Tests
{
    public class MiddleWareTests
    {
        private class DummyCheck : ICheck
        {
            public CheckResult Run()
            {
                return CheckResult.Healthy("dummy", "output");
            }
        }

        [Fact]
        public void CanCallAbout()
        {
            var builder = new ApplicationHealthModelBuilder("some app", "some description")
                .WithOwners(new Owner("ownername", "ownerslack"))
                .WithLinks(new Link("link", "description"))
                .WithRevision("abcdefg")
                .WithChecks(new DummyCheck());
            var model = builder.Build();
            var config = new OpsEndpointsMiddlewareOptions
            {
                HealthModel = model
            };
            var server = new TestServer(new WebHostBuilder().Configure(app => app.UseOpsEndpoints(config)));
        }
    }
}

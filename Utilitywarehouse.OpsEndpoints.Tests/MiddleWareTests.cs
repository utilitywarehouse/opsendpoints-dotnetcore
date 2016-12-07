using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Utilitywarehouse.OpsEndpoints.Owin;
using Utilitywarehouse.OpsEndpoints.DTO;
using System.Net;
using Newtonsoft.Json;
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
        public async void ReadyTrue()
        {
            var builder = DefaultBuilder();
            var server = ConstructTestServer(builder);
            var req = server.CreateRequest("/__/ready");
            var res = await req.GetAsync();
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        [Fact]
        public async void ReadyFalse()
        {
            var builder = DefaultBuilder().NeverReady();
            var server = ConstructTestServer(builder);
            var req = server.CreateRequest("/__/ready");
            var res = await req.GetAsync();
            Assert.Equal(HttpStatusCode.ServiceUnavailable, res.StatusCode);
        }

        [Fact]
        public async void Healthy()
        {
            var builder = DefaultBuilder();
            var server = ConstructTestServer(builder);
            var req = server.CreateRequest("/__/health");
            var res = await req.GetAsync();
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var des = new JsonSerializer();
            var str = await res.Content.ReadAsStreamAsync();
            using (var rdr = new StreamReader(str))
            {
                var resp = des.Deserialize<HealthResponse>(new JsonTextReader(rdr));
                Assert.Equal(DTO.Health.Healthy, resp.Health);
            }
        }

        private TestServer ConstructTestServer(ApplicationHealthModelBuilder builder)
        {
            var model = builder.Build();
            var config = new OpsEndpointsMiddlewareOptions
            {
                HealthModel = model
            };
            var server = new TestServer(new WebHostBuilder().Configure(app => app.UseOpsEndpoints(config)));
            return server;
        }

        private ApplicationHealthModelBuilder DefaultBuilder()
        {
            var builder = new ApplicationHealthModelBuilder("some app", "some description")
                .WithOwners(new Owner("ownername", "ownerslack"))
                .WithLinks(new Link("link", "description"))
                .WithRevision("abcdefg")
                .AlwaysReady()
                .WithChecks(new DummyCheck());
            return builder;
        }
    }
}

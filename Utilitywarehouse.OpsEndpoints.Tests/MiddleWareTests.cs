using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Utilitywarehouse.OpsEndpoints.Owin;
using Utilitywarehouse.OpsEndpoints.DTO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Utilitywarehouse.OpsEndpoints.Tests
{
    public class MiddleWareTests
    {
        private class FuncCheck : ICheck
        {
            private readonly Func<CheckResult> _checkFun;

            public FuncCheck(Func<CheckResult> checkFun)
            {
                this._checkFun = checkFun;
            }
            public CheckResult Run()
            {
                return _checkFun();
            }
        }

        [Fact]
        public async void ReadyTrue()
        {
            var builder = DefaultBuilder().AlwaysReady();
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
            var res = await GetHealth(builder);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var resp = await Deserialize<HealthResponse>(res);
            Assert.Equal(DTO.Health.Healthy, resp.Health);
        }

        [Fact]
        public async void Degraded()
        {
            var builder =
                DefaultBuilder()
                    .WithChecks(
                        new FuncCheck(
                            () => CheckResult.Degraded("degraded", "not feeling too bad", "could do with massage")));

            var res = await GetHealth(builder);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var resp = await Deserialize<HealthResponse>(res);
            Assert.Equal(DTO.Health.Degraded, resp.Health);
        }

        [Fact]
        public async void Unhealthy()
        {
            var builder = DefaultBuilder()
                .WithChecks(
                    new FuncCheck(
                        () => CheckResult.Unhealthy("unhealthy", "i don't feel so good", "fix me", "bad karma")));
            var res = await GetHealth(builder);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var resp = await Deserialize<HealthResponse>(res);
            Assert.Equal(DTO.Health.Unhealthy, resp.Health);
        }

        [Fact]
        public async void About()
        {
            var builder = DefaultBuilder();
            var server = ConstructTestServer(builder);
            var req = server.CreateRequest("/__/about");
            var res = await req.GetAsync();
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var resp = await Deserialize<AboutResponse>(res);
            var owner = resp.Owners.First();
            Assert.Equal("ownername", owner.Name);
            Assert.Equal("ownerslack", owner.Slack);
            var lnk = resp.Links.First();
            Assert.Equal("description", lnk.Description);
            Assert.Equal("link", lnk.Url);
            Assert.Equal("abcdefg", resp.BuildInfo.Revision);
        }


        private static async Task<HttpResponseMessage> GetHealth(ApplicationHealthModelBuilder builder)
        {
            var server = ConstructTestServer(builder);
            var req = server.CreateRequest("/__/health");
            var res = await req.GetAsync();
            return res;
        }

        private static TestServer ConstructTestServer(ApplicationHealthModelBuilder builder)
        {
            var model = builder.Build();
            var config = new OpsEndpointsMiddlewareOptions
            {
                HealthModel = model
            };
            var server = new TestServer(new WebHostBuilder().Configure(app => app.UseOpsEndpoints(config)));
            return server;
        }

        private static async Task<T> Deserialize<T>(HttpResponseMessage res)
        {
            T resp;
            var des = new JsonSerializer();
            var str = await res.Content.ReadAsStreamAsync();
            using (var rdr = new StreamReader(str))
            {
                resp = des.Deserialize<T>(new JsonTextReader(rdr));
            }
            return resp;
        }

        private static ApplicationHealthModelBuilder DefaultBuilder()
        {
            var builder = new ApplicationHealthModelBuilder("some app", "some description")
                .WithOwners(new Owner("ownername", "ownerslack"))
                .WithLinks(new Link("link", "description"))
                .WithRevision("abcdefg")
                .AlwaysReady()
                .WithChecks(new FuncCheck(() => CheckResult.Healthy("checkname", "checkoutput")));
            return builder;
        }
    }
}

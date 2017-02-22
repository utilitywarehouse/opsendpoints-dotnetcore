using Newtonsoft.Json;
using System.Collections.Generic;

namespace Utilitywarehouse.OpsEndpoints.DTO
{
    public class AboutResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<Owner> Owners { get; set; }
        public IList<Link> Links { get; set; }
        [JsonProperty(PropertyName = "Build-info")]
        public BuildInfo BuildInfo { get; set; }
    }

    public class Owner
    {
        public string Name { get; set; }
        public string Slack { get; set; }
    }

    public class Link
    {
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public class BuildInfo
    {
        public string Revision { get; set; }
    }

    public class HealthResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Health Health { get; set; }
        public IList<Check> Checks{ get; set; }
    }

    public class Check
    {
        public string Name { get; set; }
        public Health Health { get; set; }
        public string Output { get; set; }
        public string Action { get; set; }
        public string Impact { get; set; }
    }

    public enum Health : byte
    {
        Healthy,
        Degraded,
        Unhealthy
    }
}

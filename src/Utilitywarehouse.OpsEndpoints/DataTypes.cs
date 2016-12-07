using System.Collections.Generic;
using System.Diagnostics;

namespace Utilitywarehouse.OpsEndpoints
{
    public class Link
    {
        public Link(string url, string description)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(url),
                "!string.IsNullOrWhiteSpace(url)");
            Url = url;
            Description = description ?? string.Empty;
        }

        public string Description { get; }

        public string Url { get; }
    }

    public enum Health : byte
    {
        Healthy = 0,
        Degraded = 1,
        Unhealthy = 2
    }

    public struct HealthInfo
    {
        public readonly string Name;
        public readonly string Description;
        public readonly Health Health;
        public readonly IList<CheckResult> CheckResults;

        public HealthInfo(string name, string description, Health health, IList<CheckResult> checkResults)
        {
            Name = name;
            Description = description;
            Health = health;
            CheckResults = checkResults;
        }
    }

    public class Owner
    {
        public Owner(string name, string slack)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name),
                "!string.IsNullOrWhiteSpace(name)");
            Name = name;
            Slack = slack ?? string.Empty;
        }

        public string Name { get; }

        public string Slack { get; }
    }

    public struct CheckResult
    {
        public readonly Health Health;
        public readonly string Output;
        public readonly string Action;
        public readonly string Impact;

        private CheckResult(Health health, string output, string action, string impact)
        {
            Health = health;
            Output = output;
            Action = action;
            Impact = impact;
        }

        public static CheckResult Healthy(string output)
        {
            return new CheckResult(OpsEndpoints.Health.Healthy, output, string.Empty, string.Empty);
        }

        public static CheckResult Degraded(string output, string action)
        {
            return new CheckResult(OpsEndpoints.Health.Degraded, output, action, string.Empty);
        }

        public static CheckResult Unhealthy(string output, string action, string impact)
        {
            return new CheckResult(OpsEndpoints.Health.Unhealthy, output, action, impact);
        }
    }

    public struct AboutInfo
    {
        public readonly string Name;
        public readonly string Description;
        public readonly IList<Owner> Owners;
        public readonly IList<Link> Links;
        public readonly string Revision;

        public AboutInfo(
            string name,
            string description,
            IList<Owner> owners,
            IList<Link> links,
            string revision)
        {
            Name = name;
            Description = description;
            Owners = owners;
            Links = links;
            Revision = revision;
        }
    }
}
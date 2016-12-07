using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Utilitywarehouse.OpsEndpoints
{
    using ReadyFunc = Func<bool>;

    /// <summary>
    /// A class that is used to construct an application health model,
    /// designed to assist with making sure everything is configured
    /// </summary>
    public class ApplicationHealthModelBuilder
    {
        private readonly ApplicationHealthModel _applicationHealthModel;
        private readonly bool _failOnBuildError;

        /// <summary>
        /// ApplicationHealthModelBuilder constructor
        /// </summary>
        /// <param name="name">Name of the application</param>
        /// <param name="description">Description for the application</param>
        /// <param name="failOnBuildError">Should constructing the healthcheck fail if there is fields missing</param>
        public ApplicationHealthModelBuilder(string name, string description, bool failOnBuildError = true)
        {
            _failOnBuildError = failOnBuildError;
            Debug.Assert(!string.IsNullOrWhiteSpace(name),
                "!string.IsNullOrWhiteSpace(name)");
            Debug.Assert(!string.IsNullOrWhiteSpace(description),
                "!string.IsNullOrWhiteSpace(description)");
            _applicationHealthModel = new ApplicationHealthModel(name, description);
        }

        /// <summary>
        /// Sets the revision for the application monitored.
        /// This would typically be a git hash or tag
        /// </summary>
        /// <param name="revision"></param>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder WithRevision(string revision)
        {
            Debug.Assert(revision != null);
            _applicationHealthModel.AddRevision(revision);
            return this;
        }

        /// <summary>
        /// Sets the owners of the application, pointing to
        /// the relevant slack channel if possible
        /// </summary>
        /// <param name="owners"></param>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder WithOwners(params Owner[] owners)
        {
            Debug.Assert(owners != null);
            _applicationHealthModel.AddOwners(owners);
            return this;
        }

        /// <summary>
        /// Adds links for the application (source code, build configuration, etc)
        /// </summary>
        /// <param name="links"></param>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder WithLinks(params Link[] links)
        {
            Debug.Assert(links != null);
            _applicationHealthModel.AddLinks(links);
            return this;
        }

        /// <summary>
        /// Adds checks to the application that will
        /// surface in the health endpoint
        /// </summary>
        /// <param name="checks">One or more <see cref="ICheck"/></param>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder WithChecks(params ICheck[] checks)
        {
            Debug.Assert(checks != null);
            _applicationHealthModel.AddChecks(checks);
            return this;
        }

        /// <summary>
        /// Adds a ready function, that is surfaced on the ready endpoint
        /// </summary>
        /// <param name="ready">The ready function <see cref="ReadyFunc"/></param>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder WithReadyFunc(ReadyFunc ready)
        {
            Debug.Assert(ready != null);
            _applicationHealthModel.AddReadyFunc(ready);
            return this;
        }

        /// <summary>
        /// Adds a ready function that shows the application as always ready
        /// </summary>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder AlwaysReady()
        {
            return WithReadyFunc(() => true);
        }

        /// <summary>
        /// Adds a ready function that shows the application as never ready
        /// </summary>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder NeverReady()
        {
            return WithReadyFunc(() => false);
        }

        /// <summary>
        /// Adds a ready function that uses the healthchecks to determine ready status.
        /// Only unhealthy will cause a not ready response.
        /// </summary>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder ReadyUseHealthChecks()
        {
            _applicationHealthModel.UseHealthChecksForReady();
            return this;
        }

        /// <summary>
        /// Constructs the application health model
        /// Throws an <exception cref="Exception"/> if there are unconfigured fields,
        /// and the "failOnBuildError" constructor parameter was specified in the constructor.
        /// Otherwise configuration errors will be sent to Debug.Writeln.
        /// </summary>
        /// <returns>The configured <see cref="IApplicationHealthModel"/></returns>
        [UsedImplicitly]
        public IApplicationHealthModel Build()
        {
            var errors = new List<string>();
            if (_applicationHealthModel.Owners == null || !_applicationHealthModel.Owners.Any())
            {
                errors.Add("owners");
            }
            if (_applicationHealthModel.Checks == null || !_applicationHealthModel.Checks.Any())
            {
                errors.Add("checks");
            }
            if (_applicationHealthModel.Links == null || !_applicationHealthModel.Links.Any())
            {
                errors.Add("links");
            }
            if (string.IsNullOrWhiteSpace(_applicationHealthModel.Name))
            {
                errors.Add("name");
            }
            if (string.IsNullOrWhiteSpace(_applicationHealthModel.Description))
            {
                errors.Add("description");
            }
            if (string.IsNullOrWhiteSpace(_applicationHealthModel.Revision))
            {
                errors.Add("revision");
            }


            if (errors.Count == 0) return _applicationHealthModel;

            var error = string.Join(",", errors);

            if (_failOnBuildError)
            {
                throw new Exception(
                    string.Format(
                        "Could not initialize ApplicationHealthModel. Errors: $0",
                        error));
            }

            Debug.WriteLine(string.Format("Warning: The following fields were not set: $0", error));
            return _applicationHealthModel;
        }

    }

    internal class ApplicationHealthModel : IApplicationHealthModel
    {
        private ReadyFunc _ready;

        internal ApplicationHealthModel(string name, string description)
        {
            Name = name;
            Description = description;
        }

        internal string Name { get; }

        internal string Revision { get; private set; }

        internal string Description { get; }

        internal IList<Owner> Owners { get; private set; } = new List<Owner>();

        internal IList<Link> Links { get; private set; } = new List<Link>();

        internal IEnumerable<ICheck> Checks { get; private set; } = new List<ICheck>();

        public bool Ready()
        {
            return _ready();
        }

        internal void AddOwners(params Owner[] owners)
        {
            Owners = Owners.Concat(owners).ToList();
        }

        internal void AddLinks(params Link[] links)
        {
            Links = Links.Concat(links).ToList();
        }

        internal void AddChecks(params ICheck[] checks)
        {
            Checks = Checks.Concat(checks);
        }

        internal void AddReadyFunc(ReadyFunc ready)
        {
            _ready = ready;
        }

        internal void AddRevision(string revision)
        {
            Revision = revision;
        }

        internal void UseHealthChecksForReady()
        {
            _ready = () => Health().Health != OpsEndpoints.Health.Unhealthy;
        }

        public HealthInfo Health()
        {
            var checkResults = new List<CheckResult>();
            if (Checks.Any())
            {
                checkResults = Checks.Select(c => c.Run()).ToList();
            }

            // assume unhealthy by default
            var health = OpsEndpoints.Health.Unhealthy;

            if (checkResults.Any())
            {
                health = checkResults.Aggregate(OpsEndpoints.Health.Healthy, (prev, cr) => cr.Health > prev ? cr.Health : prev);
            }

            return new HealthInfo(Name, Description, health, checkResults);
        }

        public AboutInfo About()
        {
            return new AboutInfo(Name, Description, Owners, Links, Revision);
        }
    }

    public interface IApplicationHealthModel
    {
        [UsedImplicitly] bool Ready();
        [UsedImplicitly] HealthInfo Health();
        [UsedImplicitly] AboutInfo About();
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

    public interface ICheck
    {
        CheckResult Run();
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

    public enum Health : byte
    {
        Healthy = 0,
        Degraded = 1,
        Unhealthy = 2
    }

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
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilitywarehouse.OpsEndpoints
{
    using ReadyFunc = Func<bool>;

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
}
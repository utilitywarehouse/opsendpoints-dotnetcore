using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Utilitywarehouse.OpsEndpoints
{
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
        /// <param name="ready">The ready function <see cref="Func{bool}"/></param>
        /// <returns>The modified <see cref="ApplicationHealthModelBuilder"/></returns>
        [UsedImplicitly]
        public ApplicationHealthModelBuilder WithReadyFunc(Func<bool> ready)
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
}
using System;
using System.Linq;

namespace Utilitywarehouse.OpsEndpoints.DTO
{
    public static class Mapper
    {
        public static AboutResponse ToAboutResponse(this AboutInfo about)
        {
            return new AboutResponse
            {
                Links = about.Links.Select(ToLinkResponse).ToList(),
                Description = about.Description,
                Name = about.Name,
                Owners = about.Owners.Select(ToOwnerResponse).ToList(),
                BuildInfo = new BuildInfo
                {
                    Revision = about.Revision
                }
            };
        }

        public static HealthResponse ToHealthResponse(this HealthInfo info)
        {
            return new HealthResponse
            {
                Health = MapHealth(info.Health),
                Description = info.Description,
                Name = info.Name,
                Checks = info.CheckResults.Select(ToCheckResultResponse).ToList()
            };
        }

        private static Check ToCheckResultResponse(CheckResult infoCheckResult)
        {
            return new Check
            {
                Action = infoCheckResult.Action,
                Health = MapHealth(infoCheckResult.Health),
                Name = infoCheckResult.Name,
                Impact = infoCheckResult.Impact,
                Output = infoCheckResult.Output
            };
        }

        private static Health MapHealth(OpsEndpoints.Health infoHealth)
        {
            switch (infoHealth)
            {
                case OpsEndpoints.Health.Degraded:
                    return Health.Degraded;
                case OpsEndpoints.Health.Unhealthy:
                    return Health.Unhealthy;
                case OpsEndpoints.Health.Healthy:
                    return Health.Healthy;
                default:
                    throw new ArgumentException("health status not supported", nameof(infoHealth));
            }
        }

        private static Owner ToOwnerResponse(OpsEndpoints.Owner domainOwner)
        {
            return new Owner
            {
                Name = domainOwner.Name,
                Slack = domainOwner.Slack
            };
        }

        private static Link ToLinkResponse(OpsEndpoints.Link domainLink)
        {
            return new Link
            {
                Description = domainLink.Description,
                Url = domainLink.Url
            };
        }
    }
}
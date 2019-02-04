using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint.Security;

namespace Steeltoe.Management.Endpoint.Health
{
    public class HealthEndpointOptions : AbstractEndpointOptions, IHealthOptions
    {
        private const string MANAGEMENT_INFO_PREFIX = "management:endpoints:health";

        public HealthEndpointOptions()
            : base()
        {
            Id = "health";
            RequiredPermissions = Permissions.RESTRICTED;
        }

        public HealthEndpointOptions(IConfiguration config)
            : base(MANAGEMENT_INFO_PREFIX, config)
        {
            if (string.IsNullOrEmpty(Id))
            {
                Id = "health";
            }

            if (RequiredPermissions == Permissions.UNDEFINED)
            {
                RequiredPermissions = Permissions.RESTRICTED;
            }
        }

        public override bool DefaultSensitive => false;
    }
}

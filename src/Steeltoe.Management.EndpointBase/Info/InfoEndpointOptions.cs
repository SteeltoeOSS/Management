using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint.Info
{
    public class InfoEndpointOptions : AbstractEndpointOptions, IInfoOptions
    {
        private const string MANAGEMENT_INFO_PREFIX = "management:endpoints:info";

        public InfoEndpointOptions()
            : base()
        {
            Id = "info";
            RequiredPermissions = Permissions.RESTRICTED;
        }

        public InfoEndpointOptions(IConfiguration config)
            : base(MANAGEMENT_INFO_PREFIX, config)
        {
            if (string.IsNullOrEmpty(Id))
            {
                Id = "info";
            }

            if (RequiredPermissions == Permissions.UNDEFINED)
            {
                RequiredPermissions = Permissions.RESTRICTED;
            }
        }
    }
}

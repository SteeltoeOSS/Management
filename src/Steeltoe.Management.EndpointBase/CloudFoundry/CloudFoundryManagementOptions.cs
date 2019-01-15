using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint.CloudFoundry
{
    public class CloudFoundryManagementOptions : ManagementEndpointOptions
    {
        private const string DEFAULT_ACTUATOR_PATH = "/cloudfoundryapplication";

        public CloudFoundryManagementOptions(IConfiguration config)
            : base(config)
        {
            Path = DEFAULT_ACTUATOR_PATH;
        }
    }
}


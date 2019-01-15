using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Steeltoe.Management.Endpoint.Discovery
{
    public class ActuatorManagementOptions : ManagementEndpointOptions
    {
        private const string DEFAULT_ACTUATOR_PATH = "/actuator";

        public ActuatorManagementOptions(IConfiguration config)
            : base(config)
        {
            Path = DEFAULT_ACTUATOR_PATH;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Steeltoe.Management.Endpoint.Discovery
{
    public class ActuatorManagementOptions : ManagementEndpointOptions
    {
        private const string DEFAULT_ACTUATOR_PATH = "/actuator";

        public ActuatorManagementOptions()
        {
            Path = DEFAULT_ACTUATOR_PATH;
        }

        public ActuatorManagementOptions(IConfiguration config)
            : base(config)
        {
            if (string.IsNullOrEmpty(Path))
            {
                Path = DEFAULT_ACTUATOR_PATH;
            }
        }
    }
}

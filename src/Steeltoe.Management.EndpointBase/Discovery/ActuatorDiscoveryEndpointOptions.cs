using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint.Discovery
{
    public class ActuatorDiscoveryEndpointOptions : AbstractEndpointOptions, IActuatorDiscoveryOptions
    {
        // TODO: Not sure about this! .. Look at java
        private const string MANAGEMENT_INFO_PREFIX = "management:endpoints:actuator";

        public ActuatorDiscoveryEndpointOptions()
            : base()
        {
            Id = "discovery";
        }

        public ActuatorDiscoveryEndpointOptions(IConfiguration config)
            : base(MANAGEMENT_INFO_PREFIX, config)
        {
            Id = "discovery";
        }
    }
}
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint.Discovery
{
    public class ActuatorDiscoveryEndpointOptions : AbstractEndpointOptions, IActuatorDiscoveryOptions
    {
        // TODO: Document differences with Spring boot actuators.
        private const string MANAGEMENT_INFO_PREFIX = "management:endpoints:actuator";

        public ActuatorDiscoveryEndpointOptions()
            : base()
        {
            Id = string.Empty;
        }

        public ActuatorDiscoveryEndpointOptions(IConfiguration config)
            : base(MANAGEMENT_INFO_PREFIX, config)
        {

        }

        public override bool DefaultSensitive => false;
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Discovery;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Info;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.EndpointCore
{
    public static class ActuatorServiceCollectionExtensions
    {
        public static void AddActuators(this IServiceCollection services, IConfiguration config)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IManagementOptions>(new ActuatorManagementOptions(config)));
            services.AddDiscoveryActuator(config);
            services.AddInfoActuator(config);
            services.AddHealthActuator(config);
        }
    }
}

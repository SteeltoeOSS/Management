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

namespace Steeltoe.Management.Endpoint
{
    public static class ActuatorServiceCollectionExtensions
    {
        public static void AddActuators(this IServiceCollection services, IConfiguration config)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IManagementOptions>(new ActuatorManagementOptions(config)));
            services.AddDiscoveryActuator(config);
            services.AddInfoActuator(config, true);
            services.AddHealthActuator(config, true);
        }

        public static void RegisterEndpointOptions(this IServiceCollection services, IEndpointOptions options, bool addToDiscovery)
        {
            var mgmtOptions = services.BuildServiceProvider().GetServices<IManagementOptions>();
            foreach (var mgmt in mgmtOptions)
            {
                if (!addToDiscovery && mgmt is ActuatorManagementOptions)
                {
                    continue;
                }

                mgmt.EndpointOptions.Add(options);
            }
        }
    }
}

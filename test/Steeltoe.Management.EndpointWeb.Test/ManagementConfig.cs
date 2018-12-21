using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint;

namespace Steeltoe.Management.EndpointWeb.Test
{
    public class ManagementConfig
    {
        // public static IMetricsExporter MetricsExporter { get; set; }

        public static void ConfigureManagementActuators(IConfiguration configuration, ILoggerProvider dynamicLogger, IApiExplorer apiExplorer, ILoggerFactory loggerFactory = null)
        {
            // ActuatorConfigurator.UseCloudFoundryActuators(configuration, dynamicLogger, GetHealthContributors(configuration), apiExplorer, loggerFactory);

            // You can individually configure actuators as shown below if you don't want all of them
            ActuatorConfigurator.UseEndpointSecurity(configuration, null, loggerFactory);
            ActuatorConfigurator.UseCloudFoundrySecurity(configuration, null, loggerFactory);
            //ActuatorConfigurator.UseCloudFoundryActuator(configuration, loggerFactory);
            //ActuatorConfigurator.UseHealthActuator(configuration, null, GetHealthContributors(configuration), loggerFactory);
            //ActuatorConfigurator.UseHeapDumpActuator(configuration, null, loggerFactory);
            //ActuatorConfigurator.UseThreadDumpActuator(configuration, null, loggerFactory);
            ActuatorConfigurator.UseInfoActuator(configuration, null, loggerFactory);
            //ActuatorConfigurator.UseLoggerActuator(configuration, dynamicLogger, loggerFactory);
            //ActuatorConfigurator.UseTraceActuator(configuration, null, loggerFactory);
            //ActuatorConfigurator.UseMappingsActuator(configuration, apiExplorer, loggerFactory);

            // Uncomment if you want to enable metrics actuator endpoint, it's not enabled by default
            // ActuatorConfigurator.UseMetricsActuator(configuration, loggerFactory);
        }
    }
}

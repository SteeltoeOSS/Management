// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Autofac;
using Microsoft.Extensions.Configuration;
using Steeltoe.Management.EndpointAutofac.Actuators;

namespace Steeltoe.Management.EndpointAutofac
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Add all Actuators, configure CORS and Cloud Foundry request security
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        /// <param name="config">Your application's <see cref="IConfiguration"/></param>
        public static void UseCloudFoundryActuators(this ContainerBuilder container, IConfiguration config)
        {
            container.RegisterCloudFoundrySecurity(config);
            container.RegisterCloudFoundryActuator(config);
            container.RegisterEnvActuator(config); // not used by Cloud Foundry
            container.RegisterHealthActuator(config);
            container.RegisterHeapDumpActuator(config);
            container.RegisterInfoActuator(config);
            container.RegisterLoggersActuator(config);

            // container.RegisterMappingsActuator(config); // not implemented
            container.RegisterMetricsActuator(config);
            container.RegisterRefreshActuator(config); // not used by Cloud Foundry
            container.RegisterThreadDumpActuator(config);
            container.RegisterTraceActuator(config); // not actually implemented
        }
    }
}

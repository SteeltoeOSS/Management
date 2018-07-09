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
        /// Add all actuator OWIN Middlewares, configure CORS and Cloud Foundry request security
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        /// <param name="config">Your application's <see cref="IConfiguration"/></param>
        public static void UseCloudFoundryMiddlewares(this ContainerBuilder container, IConfiguration config)
        {
            if (container == null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            if (config == null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            // comment out the next line if experimenting with EndpointOwin\Trace\HttpEventListener
            container.RegisterTracingMiddleware(config);
            container.RegisterCloudFoundrySecurityMiddleware(config);
            container.RegisterCloudFoundryMiddleware(config);
            container.RegisterEnvMiddleware(config); // not used by Cloud Foundry
            container.RegisterHealthMiddleware(config);
            container.RegisterHeapDumpMiddleware(config);
            container.RegisterInfoMiddleware(config);
            container.RegisterLoggersMiddleware(config);

            // container.RegisterMappingsActuator(config); // not implemented
            container.RegisterMetricsMiddleware(config);
            container.RegisterRefreshMiddleware(config); // not used by Cloud Foundry
            container.RegisterThreadDumpMiddleware(config);
            container.RegisterTraceMiddleware(config);
        }

        /// <summary>
        /// Add all actuator Http Modules, configure CORS and Cloud Foundry request security
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        /// <param name="config">Your application's <see cref="IConfiguration"/></param>
        //public static void RegisterCloudFoundryModules(this ContainerBuilder container, IConfiguration config)
        //{
        //    container.RegisterRequestTracingModule(config);
        //    //container.RegisterCloudFoundrySecurityModule(config);
        //    container.RegisterCloudFoundryModule(config);
        //    container.RegisterEnvModule(config); // not used by Cloud Foundry
        //    container.RegisterHealthModule(config);
        //    container.RegisterHeapDumpModule(config);
        //    container.RegisterInfoModule(config);
        //    container.RegisterLoggersModule(config);

        //    // container.RegisterMappingsModule(config); // not implemented
        //    container.RegisterMetricsModule(config);
        //    container.RegisterRefreshModule(config); // not used by Cloud Foundry
        //    container.RegisterThreadDumpModule(config);
        //    container.RegisterTraceModule(config);
        //}
    }
}

﻿// Copyright 2017 the original author or authors.
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Steeltoe.Management.Endpoint.Discovery;
using Steeltoe.Management.Endpoint.Env;

namespace Steeltoe.Management.Endpoint.Mappings
{
    public static class EndpointServiceCollectionExtensions
    {
        /// <summary>
        /// Adds components of the Mappings actuator to Microsoft-DI
        /// </summary>
        /// <param name="services">Service collection to add actuator to</param>
        /// <param name="config">Application configuration (this actuator looks for settings starting with management:endpoints:dump)</param>
        public static void AddMappingsActuator(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            //services.TryAddSingleton<IMappingsOptions>(new MappingsOptions(config));
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IManagementOptions>(new ActuatorManagementOptions(config)));

            services.TryAddSingleton<IMappingsOptions>(provider =>
            {
                var mgmtOptions = provider.GetServices<IManagementOptions>();
                var opts = new MappingsEndpointOptions(config);
                foreach (var mgmt in mgmtOptions)
                {
                    mgmt.EndpointOptions.Add(opts);
                }
                return opts;
            });
            services.TryAddSingleton<IRouteMappings, RouteMappings>();
        }
    }
}

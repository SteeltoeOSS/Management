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
using Microsoft.Extensions.Hosting;
using Steeltoe.Common.Diagnostics;
using Steeltoe.Management.Endpoint.Diagnostics;
using System;
using System.Linq;
using Steeltoe.Management.Endpoint.Discovery;
using Steeltoe.Management.Endpoint.Metrics;

namespace Steeltoe.Management.Endpoint.Trace
{
    public static class EndpointServiceCollectionExtensions
    {
        /// <summary>
        /// Adds components of the Trace actuator to Microsoft-DI
        /// </summary>
        /// <param name="services">Service collection to add trace to</param>
        /// <param name="config">Application configuration (this actuator looks for settings starting with management:endpoints:trace)</param>
        public static void AddTraceActuator(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            services.TryAddSingleton<IDiagnosticsManager, DiagnosticsManager>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, DiagnosticServices>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagnosticObserver, TraceDiagnosticObserver>());
            services.TryAddSingleton<ITraceRepository>((p) => p.GetServices<IDiagnosticObserver>().OfType<TraceDiagnosticObserver>().Single());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IManagementOptions>(new ActuatorManagementOptions(config)));

            var options = new TraceEndpointOptions(config);
            services.TryAddSingleton<ITraceOptions>(options);
            services.RegisterEndpointOptions(options);
            services.TryAddSingleton<TraceEndpoint>();
        }
        /// <summary>
        /// Adds components of the Trace actuator to Microsoft-DI
        /// </summary>
        /// <param name="services">Service collection to add trace to</param>
        /// <param name="config">Application configuration (this actuator looks for settings starting with management:endpoints:trace)</param>
        public static void AddHttpTraceActuator(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            services.TryAddSingleton<IDiagnosticsManager, DiagnosticsManager>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, DiagnosticServices>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagnosticObserver, HttpTraceDiagnosticObserver>());
           // services.TryAddSingleton<IHttpTraceRepository>((p) => p.GetServices<IDiagnosticObserver>().OfType<HttpTraceDiagnosticObserver>().Single());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IManagementOptions>(new ActuatorManagementOptions(config)));

            var options = new HttpTraceEndpointOptions(config);
            services.TryAddSingleton<ITraceOptions>(options);
            services.RegisterEndpointOptions(options);
            services.TryAddSingleton(p => new HttpTraceEndpoint(options, p.GetServices<IDiagnosticObserver>().OfType<HttpTraceDiagnosticObserver>().Single()));
        }
    }
}

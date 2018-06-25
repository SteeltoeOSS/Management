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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Owin;
using Steeltoe.Management.Census.Stats;
using Steeltoe.Management.Endpoint.Metrics;

namespace Steeltoe.Management.EndpointOwin.Metrics
{
    public static class MetricsEndpointAppBuilderExtensions
    {
        /// <summary>
        /// Add Metrics middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="config"><see cref="IConfiguration"/> of application for configuring metrics endpoint</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Metrics Endpoint added</returns>
        public static IAppBuilder UseMetricsEndpointMiddleware(this IAppBuilder builder, IConfiguration config, ILoggerFactory loggerFactory = null)
        {
            var stats = new OpenCensusStats();
            return builder.UseMetricsEndpointMiddleware(config, stats, loggerFactory);
        }

        /// <summary>
        /// Add Metrics middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="config"><see cref="IConfiguration"/> of application for configuring metrics endpoint</param>
        /// <param name="stats">Class for recording statistics - See also: <seealso cref="OpenCensusStats"/></param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Metrics Endpoint added</returns>
        public static IAppBuilder UseMetricsEndpointMiddleware(this IAppBuilder builder, IConfiguration config, IStats stats, ILoggerFactory loggerFactory = null)
        {
            var endpoint = new MetricsEndpoint(new MetricsOptions(config), stats, loggerFactory?.CreateLogger<MetricsEndpoint>());
            var logger = loggerFactory?.CreateLogger<EndpointOwinMiddleware<MetricsEndpoint, MetricsRequest>>();
            return builder.Use<MetricsEndpointOwinMiddleware>(endpoint, logger);
        }
    }
}

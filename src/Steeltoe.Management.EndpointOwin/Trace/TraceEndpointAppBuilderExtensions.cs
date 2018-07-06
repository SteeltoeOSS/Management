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
using Steeltoe.Management.Endpoint.Trace;
using System.Collections.Generic;

namespace Steeltoe.Management.EndpointOwin.Trace
{
    public static class TraceEndpointAppBuilderExtensions
    {
        /// <summary>
        /// Add Request Trace middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="config"><see cref="IConfiguration"/> of application for configuring thread dump endpoint</param>
        /// <param name="traceRepository">A repository that contains recent application traces</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Trace Endpoint added</returns>
        public static IAppBuilder UseTraceEndpointMiddleware(this IAppBuilder builder, IConfiguration config, ITraceRepository traceRepository, ILoggerFactory loggerFactory = null)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            if (config == null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            if (traceRepository == null)
            {
                throw new System.ArgumentNullException(nameof(traceRepository));
            }

            var options = new TraceOptions(config);
            var endpoint = new TraceEndpoint(options, traceRepository, loggerFactory?.CreateLogger<TraceEndpoint>());
            var logger = loggerFactory?.CreateLogger<EndpointOwinMiddleware<TraceEndpoint, List<TraceResult>>>();
            return builder.Use<EndpointOwinMiddleware<TraceEndpoint, List<TraceResult>>>(endpoint, logger);
        }
    }
}

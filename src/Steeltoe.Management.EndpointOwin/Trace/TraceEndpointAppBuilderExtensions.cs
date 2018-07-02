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
        public static IAppBuilder UseTraceEndpointMiddleware(this IAppBuilder builder, IConfiguration config, ITraceRepository traceRepository = null, ILoggerFactory loggerFactory = null)
        {
            var options = new TraceOptions(config);
            if (traceRepository == null)
            {
                traceRepository = new TraceRepository(options, loggerFactory?.CreateLogger<TraceRepository>());
            }

            var endpoint = new TraceEndpoint(options, traceRepository, loggerFactory?.CreateLogger<TraceEndpoint>());
            var logger = loggerFactory?.CreateLogger<EndpointOwinMiddleware<TraceEndpoint, List<TraceResult>>>();
            return builder.Use<EndpointOwinMiddleware<TraceEndpoint, List<TraceResult>>>(endpoint, logger);
        }
    }
}

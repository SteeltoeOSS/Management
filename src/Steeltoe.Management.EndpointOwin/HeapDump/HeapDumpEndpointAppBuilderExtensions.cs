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
using Steeltoe.Management.Endpoint.HeapDump;

namespace Steeltoe.Management.EndpointOwin.HeapDump
{
    /// <summary>
    /// TODO: Heap Dump endpoint has permissions issue running local in sample app!
    /// </summary>
    public static class HeapDumpEndpointAppBuilderExtensions
    {
        /// <summary>
        /// Adds OWIN Middleware for providing Heap Dumps to OWIN pipeline
        /// </summary>
        /// <param name="builder">Your <see cref="IAppBuilder"/></param>
        /// <param name="config"><see cref="IConfiguration"/> for configuring the endpoint</param>
        /// <param name="loggerFactory"><see cref="ILoggerFactory"/> for logging inside the middleware and its components</param>
        /// <returns>Your <see cref="IAppBuilder"/> with Heap Dump middleware attached</returns>
        public static IAppBuilder UseHeapDumpEndpointMiddleware(this IAppBuilder builder, IConfiguration config, ILoggerFactory loggerFactory = null)
        {
            if (config == null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            var options = new HeapDumpOptions(config);
            var heapDumper = new HeapDumper(options, loggerFactory?.CreateLogger<HeapDumper>());
            return builder.UseHeapDumpEndpointMiddleware(options, heapDumper, loggerFactory);
        }

        /// <summary>
        /// Adds OWIN Middleware for providing Heap Dumps to OWIN pipeline
        /// </summary>
        /// <param name="builder">Your <see cref="IAppBuilder"/></param>
        /// <param name="options"><see cref="IHeapDumpOptions"/> to configure the endpoint</param>
        /// <param name="heapDumper"><see cref="HeapDumper"/> or other implementer of <see cref="IHeapDumper"/> for retrieving a heap dump</param>
        /// <param name="loggerFactory"><see cref="ILoggerFactory"/> for logging inside the middleware and its components</param>
        /// <returns>Your <see cref="IAppBuilder"/> with Heap Dump middleware attached</returns>
        public static IAppBuilder UseHeapDumpEndpointMiddleware(this IAppBuilder builder, IHeapDumpOptions options, IHeapDumper heapDumper, ILoggerFactory loggerFactory = null)
        {
            if (options == null)
            {
                throw new System.ArgumentNullException(nameof(options));
            }

            if (heapDumper == null)
            {
                throw new System.ArgumentNullException(nameof(heapDumper));
            }

            var endpoint = new HeapDumpEndpoint(options, heapDumper, loggerFactory?.CreateLogger<HeapDumpEndpoint>());
            return builder.Use<EndpointOwinMiddleware<HeapDumpEndpoint, string>>(endpoint, loggerFactory?.CreateLogger<EndpointOwinMiddleware<HeapDumpEndpoint, string>>());
        }
    }
}

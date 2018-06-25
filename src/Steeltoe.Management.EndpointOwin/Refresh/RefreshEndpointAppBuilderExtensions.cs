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
using Microsoft.Extensions.Logging;
using Owin;
using Steeltoe.Management.Endpoint.Refresh;
using System.Collections.Generic;

namespace Steeltoe.Management.EndpointOwin.Refresh
{
    public static class RefreshEndpointAppBuilderExtensions
    {
        /// <summary>
        /// Add (Config) Refresh middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="config"><see cref="IConfiguration"/> of application for configuring refresh endpoint</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Refresh Endpoint added</returns>
        public static IAppBuilder UseRefreshEndpointMiddleware(this IAppBuilder builder, IConfiguration config, ILoggerFactory loggerFactory = null)
        {
            var endpoint = new RefreshEndpoint(new RefreshOptions(config), config, loggerFactory?.CreateLogger<RefreshEndpoint>());
            var logger = loggerFactory?.CreateLogger<EndpointOwinMiddleware<RefreshEndpoint, IList<string>>>();
            return builder.Use<EndpointOwinMiddleware<RefreshEndpoint, IList<string>>>(endpoint, logger);
        }
    }
}

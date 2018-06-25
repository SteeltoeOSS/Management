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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Owin;
using Steeltoe.Management.Endpoint.Env;
using System;

namespace Steeltoe.Management.EndpointOwin.Env
{
    public static class EnvEndpointAppBuilderExtensions
    {
        /// <summary>
        /// Add Environment middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="config"><see cref="IConfiguration"/> of application for configuring env endpoint and inclusion in response</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Env Endpoint added</returns>
        public static IAppBuilder UseEnvEndpointMiddleware(this IAppBuilder builder, IConfiguration config, ILoggerFactory loggerFactory = null)
        {
            // TODO: probably not this
            var hostingEnvironment = new HostingEnvironment()
            {
                ApplicationName = config[HostDefaults.ApplicationKey],
                EnvironmentName = config[HostDefaults.EnvironmentKey] ?? EnvironmentName.Production,
                ContentRootPath = AppContext.BaseDirectory
            };
            hostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(hostingEnvironment.ContentRootPath);
            return builder.UseEnvEndpointMiddleware(config, hostingEnvironment, loggerFactory);
        }

        /// <summary>
        /// Add Environment middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="config"><see cref="IConfiguration"/> of application for configuring env endpoint and inclusion in response</param>
        /// <param name="hostingEnvironment"><see cref="IHostingEnvironment"/> of the application</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Env Endpoint added</returns>
        public static IAppBuilder UseEnvEndpointMiddleware(this IAppBuilder builder, IConfiguration config, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory = null)
        {
            var endpoint = new EnvEndpoint(new EnvOptions(config), config, hostingEnvironment, loggerFactory?.CreateLogger<EnvEndpoint>());
            var logger = loggerFactory?.CreateLogger<EndpointOwinMiddleware<EnvEndpoint, EnvironmentDescriptor>>();
            return builder.Use<EndpointOwinMiddleware<EnvEndpoint, EnvironmentDescriptor>>(endpoint, logger);
        }
    }
}

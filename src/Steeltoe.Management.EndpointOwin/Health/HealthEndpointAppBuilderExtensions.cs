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
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Health.Contributor;
using System;
using System.Collections.Generic;

namespace Steeltoe.Management.EndpointOwin.Health
{
    public static class HealthEndpointAppBuilderExtensions
    {
        /// <summary>
        /// Add HealthCheck middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="config"><see cref="IConfiguration"/> of application for configuring health endpoint</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Health Endpoint added</returns>
        public static IAppBuilder UseHealthEndpointMiddleware(this IAppBuilder builder, IConfiguration config, ILoggerFactory loggerFactory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return builder.UseHealthEndpointMiddleware(new HealthOptions(config), new DefaultHealthAggregator(), loggerFactory);
        }

        /// <summary>
        /// Add HealthCheck middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="options"><see cref="IHealthOptions"/> for configuring Health endpoint</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Health Endpoint added</returns>
        public static IAppBuilder UseHealthEndpointMiddleware(this IAppBuilder builder, IHealthOptions options, ILoggerFactory loggerFactory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return builder.UseHealthEndpointMiddleware(options, new DefaultHealthAggregator(), loggerFactory);
        }

        /// <summary>
        /// Add HealthCheck middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="options"><see cref="IHealthOptions"/> for configuring Health endpoint</param>
        /// <param name="aggregator"><see cref="IHealthAggregator"/> for determining how to report aggregate health of the application</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Health Endpoint added</returns>
        public static IAppBuilder UseHealthEndpointMiddleware(this IAppBuilder builder, IHealthOptions options, IHealthAggregator aggregator, ILoggerFactory loggerFactory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (aggregator == null)
            {
                throw new ArgumentNullException(nameof(aggregator));
            }

            return builder.UseHealthEndpointMiddleware(options, aggregator, GetDefaultHealthContributors(), loggerFactory);
        }

        /// <summary>
        /// Add HealthCheck middleware to OWIN Pipeline
        /// </summary>
        /// <param name="builder">OWIN <see cref="IAppBuilder" /></param>
        /// <param name="options"><see cref="IHealthOptions"/> for configuring Health endpoint</param>
        /// <param name="aggregator"><see cref="IHealthAggregator"/> for determining how to report aggregate health of the application</param>
        /// <param name="contributors">A list of <see cref="IHealthContributor"/> to monitor for determining application health</param>
        /// <param name="loggerFactory">For logging within the middleware</param>
        /// <returns>OWIN <see cref="IAppBuilder" /> with Health Endpoint added</returns>
        public static IAppBuilder UseHealthEndpointMiddleware(this IAppBuilder builder, IHealthOptions options, IHealthAggregator aggregator, IEnumerable<IHealthContributor> contributors, ILoggerFactory loggerFactory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (aggregator == null)
            {
                throw new ArgumentNullException(nameof(aggregator));
            }

            if (contributors == null)
            {
                throw new ArgumentNullException(nameof(contributors));
            }

            var endpoint = new HealthEndpoint(options, aggregator, contributors, loggerFactory?.CreateLogger<HealthEndpoint>());
            var logger = loggerFactory?.CreateLogger<HealthEndpointOwinMiddleware>();
            return builder.Use<HealthEndpointOwinMiddleware>(endpoint, logger);
        }

        internal static List<IHealthContributor> GetDefaultHealthContributors()
        {
            return new List<IHealthContributor>
            {
                new DiskSpaceContributor()
            };
        }
    }
}
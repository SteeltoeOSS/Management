﻿//
// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Steeltoe.Management.Endpoint.Health.Contributor;
using System;
using System.Collections.Generic;


namespace Steeltoe.Management.Endpoint.Health
{
    public static class EndpointServiceCollectionExtensions
    {
        public static void AddHealthActuator(this IServiceCollection services, IConfiguration config)
        {
            services.AddHealthActuator(config, new DefaultHealthAggregator(), GetDefaultHealthContributors());
        }

        public static void AddHealthActuator(this IServiceCollection services, IConfiguration config, params IHealthContributor[] contributors)
        {
            services.AddHealthActuator(config, new DefaultHealthAggregator(), contributors);
        }

        public static void AddHealthActuator(this IServiceCollection services, IConfiguration config, IHealthAggregator aggregator, params IHealthContributor[] contributors)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (aggregator == null)
            {
                throw new ArgumentNullException(nameof(aggregator));
            }

            services.TryAddSingleton<IHealthOptions>(new HealthOptions(config));
            AddHealthContributors(services, contributors);
            services.TryAddSingleton<IHealthAggregator>(aggregator);
            services.TryAddSingleton<HealthEndpoint>();

        }

        internal static void AddHealthContributors(IServiceCollection services, params IHealthContributor[] contributors)
        {
            List<ServiceDescriptor> descriptors = new List<ServiceDescriptor>();
            foreach (var instance in contributors)
            {
                descriptors.Add(ServiceDescriptor.Singleton<IHealthContributor>(instance));
            }

            services.TryAddEnumerable(descriptors);
        }

        internal static IHealthContributor[] GetDefaultHealthContributors()
        {
            return new IHealthContributor[]
            {
                new DiskSpaceContributor()
            };

        }
    }
}


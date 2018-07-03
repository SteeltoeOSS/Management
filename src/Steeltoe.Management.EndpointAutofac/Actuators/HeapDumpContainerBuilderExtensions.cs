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

using Autofac;
using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.HeapDump;
using Steeltoe.Management.EndpointOwin;
using Steeltoe.Management.EndpointSysWeb;
using System;
using System.IO;
using System.Web;

namespace Steeltoe.Management.EndpointAutofac.Actuators
{
    public static class HeapDumpContainerBuilderExtensions
    {
        /// <summary>
        /// Register the HeapDump endpoint, OWIN middleware and options
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        /// <param name="config">Your application's <see cref="IConfiguration"/></param>
        public static void RegisterHeapDumpMiddleware(this ContainerBuilder container, IConfiguration config)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            container.RegisterInstance(new HeapDumpOptions(config)).As<IHeapDumpOptions>();

            // REVIEW: is this necessary? Running under IIS Express, the path comes up wrong
            container.RegisterType<HeapDumper>().As<IHeapDumper>().WithParameter("basePathOverride", GetContentRoot());
            container.RegisterType<HeapDumpEndpoint>().As<IEndpoint<string>>();
            container.RegisterType<EndpointOwinMiddleware<HeapDumpEndpoint, string>>();
        }

        /// <summary>
        /// Register the HeapDump endpoint, HttpModule and options
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        /// <param name="config">Your application's <see cref="IConfiguration"/></param>
        public static void RegisterHeapDumpModule(this ContainerBuilder container, IConfiguration config)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            container.RegisterInstance(new HeapDumpOptions(config)).As<IHeapDumpOptions>();

            // REVIEW: is this necessary? Running under IIS Express, the path comes up wrong
            container.RegisterType<HeapDumper>().As<IHeapDumper>().WithParameter("basePathOverride", GetContentRoot());
            container.RegisterType<HeapDumpEndpoint>();
            container.RegisterType<HeapDumpModule>().As<IHttpModule>();
        }

        public static string GetContentRoot()
        {
            var basePath = (string)AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY") ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(basePath);
        }
    }
}

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

using Microsoft.AspNetCore.Builder;
using System;

namespace Steeltoe.Management.Endpoint.ThreadDump
{
    public static class EndpointApplicationBuilderExtensions
    {
        /// <summary>
        /// Enable the thread dump middleware
        /// </summary>
        /// <param name="builder">Your application builder</param>
        public static void UseThreadDumpActuator(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.UseThreadDumpActuator(MediaTypeVersion.V1);
        }

        /// <summary>
        /// Enable the thread dump middleware
        /// </summary>
        /// <param name="builder">Your application builder</param>
        /// <param name="version">MediaType version</param>
        public static void UseThreadDumpActuator(this IApplicationBuilder builder, MediaTypeVersion version)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            switch (version)
            {
                case MediaTypeVersion.V1:
                    builder.UseMiddleware<ThreadDumpEndpointMiddleware>();
                    break;
                default:
                    builder.UseMiddleware<ThreadDumpEndpointMiddleware_v2>();
                    break;
            }
        }
    }
}

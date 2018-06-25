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

using Microsoft.Extensions.Logging;
using Microsoft.Owin;
using Steeltoe.Management.Endpoint.CloudFoundry;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Steeltoe.Management.EndpointOwin.CloudFoundry
{
    public class CloudFoundryEndpointOwinMiddleware : EndpointOwinMiddleware<Links, string>
    {
        protected new CloudFoundryEndpoint _endpoint;

        public CloudFoundryEndpointOwinMiddleware(OwinMiddleware next, CloudFoundryEndpoint endpoint, ILogger logger)
            : base(next, logger)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                return;
            }

            if (!IsCloudFoundryRequest(context))
            {
                await Next.Invoke(context);
            }
            else
            {
                var endpointResponse = _endpoint.Invoke(GetRequestUri(context.Request));
                _logger?.LogTrace("Returning: {EndpointResponse}", endpointResponse);
                context.Response.Headers.SetValues("Content-Type", new string[] { "application/json;charset=UTF-8" });
                await context.Response.WriteAsync(Serialize(endpointResponse));
            }
        }

        protected internal string GetRequestUri(IOwinRequest request)
        {
            string scheme = request.Scheme;

            if (request.Headers.TryGetValue("X-Forwarded-Proto", out string[] headerScheme))
            {
                scheme = headerScheme.First(); // .ToString()
            }

            return scheme + "://" + request.Host.ToString() + request.Path.ToString();
        }

        private bool AddCorsHeaders(IOwinContext context)
        {
            return context.Request.Path.StartsWithSegments(new PathString(_endpoint.Path));
        }

        private bool IsCloudFoundryRequest(IOwinContext context)
        {
            var methodMatch = context.Request.Method == "GET";
            var pathMatch = context.Request.Path.Value.Equals(_endpoint.Path);
            return methodMatch && pathMatch;
        }
    }
}

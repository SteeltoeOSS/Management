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

using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint.CloudFoundry;
using System;
using System.Web;

namespace Steeltoe.Management.EndpointSysWeb
{
    public class CloudFoundryModule : ActuatorModule<CloudFoundryEndpoint, Links, string>
    {
        public CloudFoundryModule(CloudFoundryEndpoint endpoint, ILogger logger)
            : base(endpoint, logger)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        protected internal string GetRequestUri(HttpRequest request)
        {
            string scheme = request.IsSecureConnection ? "https" : "http";
            string headerScheme = request.Headers.Get("X-Forwarded-Proto");

            if (headerScheme != null)
            {
                scheme = headerScheme;
            }

            return scheme + "://" + request.Url.Authority + request.Path.ToString();
        }

        protected override void HandleRequest(HttpContext context)
        {
            _logger?.LogTrace("Processing {SteeltoeEndpoint} request", typeof(CloudFoundryEndpoint));
            var result = _endpoint.Invoke(GetRequestUri(context.Request));
            context.Response.Headers.Set("Content-Type", "application/vnd.spring-boot.actuator.v1+json");
            context.Response.Write(Serialize(result));
        }
    }
}

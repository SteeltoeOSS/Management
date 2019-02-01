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
using Microsoft.Owin;
using Newtonsoft.Json;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Discovery;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Steeltoe.Management.EndpointOwin.Security
{
    public class SecurityMiddleware : OwinMiddleware
    {
        private ILogger<SecurityMiddleware> _logger;
        //  private ICloudFoundryOptions _options;
        private IActuatorDiscoveryOptions _options;
        private IManagementOptions _actuatorManagementOptions;

        public readonly string ENDPOINT_NOT_CONFIGURED_MESSAGE = "Endpoint is not available";

        public readonly string ACCESS_DENIED_MESSAGE = "Access denied";
        //  private SecurityHelper _helper;

        public SecurityMiddleware(OwinMiddleware next, IActuatorDiscoveryOptions options, IManagementOptions actuatorManagementOptions, ILogger<SecurityMiddleware> logger)
            : base(next)
        {
            _logger = logger;
            _options = options;
            _actuatorManagementOptions = actuatorManagementOptions;
         //   _helper = new SecurityHelper(options, logger);
        }

        public override async Task Invoke(IOwinContext context)
        {
            _logger?.LogDebug("Invoke({0})", context.Request.Path.Value);

            // if (_helper.IsCloudFoundryRequest(context.Request.Path.Value) && _options.IsEnabled)
            if (IsActuatorRequest(context.Request.Path.Value) && _options.IsEnabled(_actuatorManagementOptions))

            {
                IEndpointOptions target = FindTargetEndpoint(context.Request.Path);
                if (target == null)
                {
                    await ReturnError(
                        context,
                        new SecurityResult(HttpStatusCode.ServiceUnavailable, ENDPOINT_NOT_CONFIGURED_MESSAGE));
                    return;
                }

                if (target.IsSensitive(_actuatorManagementOptions) && !HasSensitiveClaim(context))
                {
                    await ReturnError(
                        context,
                        new SecurityResult(HttpStatusCode.Unauthorized, ACCESS_DENIED_MESSAGE));

                    return;
                }
            }

            await Next.Invoke(context);
        }

        protected bool IsActuatorRequest(string path)
        {
            var contextPath = _actuatorManagementOptions.Path;
            return path.StartsWith(contextPath);

        }


        public async Task ReturnError(IOwinContext context, SecurityResult error)
        {
         //   LogError(context, error);
            //context.Response.Headers.Add("Content-Type", ["application/json;charset=UTF-8"]);
            context.Response.StatusCode = (int)error.Code;
            await context.Response.WriteAsync(Serialize(error));
        }

        public string Serialize(SecurityResult error)
        {
            try
            {
                return JsonConvert.SerializeObject(error);
            }
            catch (Exception e)
            {
                _logger?.LogError("Serialization Exception: {0}", e);
            }

            return string.Empty;
        }

        private bool HasSensitiveClaim(IOwinContext context)
        {
            var claim = _actuatorManagementOptions.SensitiveClaim;
            var user = context.Authentication.User;
            return user != null && user.HasClaim(claim.Type, claim.Value);
        }

        private IEndpointOptions FindTargetEndpoint(PathString path)
        {
            var configEndpoints = _actuatorManagementOptions.EndpointOptions;

            foreach (var ep in configEndpoints)
            {
                var contextPath = _actuatorManagementOptions.Path;
                if (!contextPath.EndsWith("/") && !string.IsNullOrEmpty(ep.Path))
                {
                    contextPath += "/";
                }

                var fullPath = contextPath + ep.Path;
                if (path.StartsWithSegments(new PathString(fullPath)))
                {
                    return ep;
                }
            }

            return null;
        }
    }
}

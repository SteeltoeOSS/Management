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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Health;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Steeltoe.Management.EndpointOwin
{
    public class EndpointOwinMiddleware<TEndpoint, TResult> : OwinMiddleware
    {
        protected IEndpoint<TResult> _endpoint;
        protected ILogger<TEndpoint> _logger;

        public EndpointOwinMiddleware(OwinMiddleware next, ILogger<TEndpoint> logger = null)
            : base(next)
        {
            _logger = logger;
        }

        public EndpointOwinMiddleware(OwinMiddleware next, IEndpoint<TResult> endpoint, ILogger<TEndpoint> logger = null)
            : base(next)
        {
            _endpoint = endpoint;
            _logger = logger;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var mw = typeof(TEndpoint);
            if (!PathAndMethodMatch(context.Request.Path, context.Request.Method))
            {
                await Next.Invoke(context);
            }
            else
            {
                _logger?.LogTrace("Processing {SteeltoeEndpoint} request", typeof(TEndpoint));
                var result = _endpoint.Invoke();
                context.Response.Headers.SetValues("Content-Type", new string[] { "application/vnd.spring-boot.actuator.v1+json" });
                await context.Response.WriteAsync(Serialize(result));
            }
        }

        protected virtual string Serialize<T>(T result)
        {
            try
            {
                var serializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
                };
                serializerSettings.Converters.Add(new HealthJsonConverter());

                return JsonConvert.SerializeObject(result, serializerSettings);
            }
            catch (Exception e)
            {
                _logger?.LogError("Error {Exception} serializing {MiddlewareResponse}", e, result);
            }

            return string.Empty;
        }

        protected virtual bool PathAndMethodMatch(PathString requestPath, string httpMethod)
        {
            return requestPath.Equals(new PathString(_endpoint.Path)) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
        }

        protected virtual bool PathStartsWithAndMethodMatches(PathString requestPath, string httpMethod)
        {
            return requestPath.StartsWithSegments(new PathString(_endpoint.Path)) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class EndpointOwinMiddleware<TEndpoint, TResult, TRequest> : EndpointOwinMiddleware<TEndpoint, TResult>
    {
        protected new IEndpoint<TResult, TRequest> _endpoint;

        public EndpointOwinMiddleware(OwinMiddleware next, IEndpoint<TResult, TRequest> endpoint, ILogger<TEndpoint> logger)
            : base(next, logger)
        {
            this._endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public virtual string HandleRequest(TRequest arg)
        {
            var result = _endpoint.Invoke(arg);
            return Serialize(result);
        }

        protected override bool PathAndMethodMatch(PathString requestPath, string httpMethod)
        {
            return requestPath.Equals(new PathString(_endpoint.Path)) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
        }

        protected override bool PathStartsWithAndMethodMatches(PathString requestPath, string httpMethod)
        {
            return requestPath.StartsWithSegments(new PathString(_endpoint.Path)) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}
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
using Newtonsoft.Json.Serialization;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Health;
using System;
using System.Threading.Tasks;

namespace Steeltoe.Management.EndpointOwin
{
    public class EndpointOwinMiddleware<TEndpoint, TResult> : OwinMiddleware
    {
        protected IEndpoint<TResult> _endpoint;
        protected ILogger _logger;

        public EndpointOwinMiddleware(OwinMiddleware next, ILogger logger = null)
            : base(next)
        {
            _logger = logger;
        }

        public EndpointOwinMiddleware(OwinMiddleware next, IEndpoint<TResult> endpoint, ILogger logger = null)
            : base(next)
        {
            _endpoint = endpoint;
            _logger = logger;
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!_endpoint.RequestVerbAndPathMatch(context.Request.Method, context.Request.Path.Value))
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
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class EndpointOwinMiddleware<TEndpoint, TResult, TRequest> : EndpointOwinMiddleware<TEndpoint, TResult>
    {
        protected new IEndpoint<TResult, TRequest> _endpoint;

        public EndpointOwinMiddleware(OwinMiddleware next, IEndpoint<TResult, TRequest> endpoint, ILogger logger)
            : base(next, logger)
        {
            this._endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public virtual string HandleRequest(TRequest arg)
        {
            var result = _endpoint.Invoke(arg);
            return Serialize(result);
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}
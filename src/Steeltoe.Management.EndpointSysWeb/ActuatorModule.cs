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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Health;
using System;
using System.Linq;
using System.Web;

namespace Steeltoe.Management.EndpointSysWeb
{
    public class ActuatorModule<TModule, TEndpoint, TResult> : IHttpModule
    {
        protected IEndpoint<TResult> _endpoint;
        protected ILogger<TModule> _logger;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public virtual void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(FilterAndPreProcessRequest);
        }

        protected virtual void FilterAndPreProcessRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;

            var mw = typeof(TEndpoint);
            if (PathAndMethodMatch(context.Request.Path, context.Request.HttpMethod))
            {
                HandleRequest(context);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        protected virtual void HandleRequest(HttpContext context)
        {
            _logger?.LogTrace("Processing {SteeltoeEndpoint} request", typeof(TEndpoint));
            var result = _endpoint.Invoke();
            context.Response.Headers.Set("Content-Type", "application/vnd.spring-boot.actuator.v1+json");
            context.Response.Write(Serialize(result));
        }

        protected virtual bool PathAndMethodMatch(string requestPath, string httpMethod)
        {
            return requestPath.Equals(_endpoint.Path) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
        }

        protected virtual bool PathStartsWithAndMethodMatches(string requestPath, string httpMethod)
        {
            return requestPath.StartsWith(_endpoint.Path) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
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
    public class ActuatorModule<TModule, TEndpoint, TResult, TRequest> : ActuatorModule<TModule, TEndpoint, TResult>
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected new IEndpoint<TResult, TRequest> _endpoint;

        public ActuatorModule(IEndpoint<TResult, TRequest> endpoint, ILogger<TModule> logger)
            : base()
        {
            _endpoint = endpoint;
        }

        public virtual string HandleRequest(TRequest arg)
        {
            var result = _endpoint.Invoke(arg);
            return Serialize(result);
        }

        protected override bool PathAndMethodMatch(string requestPath, string httpMethod)
        {
            return requestPath.Equals(_endpoint.Path) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
        }

        protected override bool PathStartsWithAndMethodMatches(string requestPath, string httpMethod)
        {
            return requestPath.StartsWith(_endpoint.Path) && _endpoint.AllowedMethods.Any(m => m.Method.Equals(httpMethod));
        }
    }
}

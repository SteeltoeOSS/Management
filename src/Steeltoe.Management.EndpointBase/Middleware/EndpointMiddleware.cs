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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Steeltoe.Management.Endpoint.Health;
using System;

namespace Steeltoe.Management.Endpoint.Middleware
{
    public class EndpointMiddleware<TResult>
    {
        protected IEndpoint<TResult> endpoint;
        protected ILogger logger;

        public EndpointMiddleware(ILogger logger = null)
        {
            this.logger = logger;
        }

        public EndpointMiddleware(IEndpoint<TResult> endpoint, ILogger logger = null)
            : this(logger)
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        internal IEndpoint<TResult> Endpoint
        {
            get
            {
                return endpoint;
            }

            set
            {
                endpoint = value;
            }
        }

        public virtual string HandleRequest()
        {
            var result = endpoint.Invoke();
            return Serialize(result);
        }

        protected virtual string Serialize(TResult result)
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
                logger?.LogError("Error {Exception} serializing {MiddlewareResponse}", e, result);
            }

            return string.Empty;
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class EndpointMiddleware<TResult, TRequest> : EndpointMiddleware<TResult>
    {
        protected new IEndpoint<TResult, TRequest> endpoint;

        public EndpointMiddleware(IEndpoint<TResult, TRequest> endpoint, ILogger logger = null)
            : base(logger)
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public virtual string HandleRequest(TRequest arg)
        {
            var result = endpoint.Invoke(arg);
            return Serialize(result);
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}

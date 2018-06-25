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

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Steeltoe.Management.Endpoint
{
    public abstract class AbstractEndpoint : IEndpoint
    {
        protected IEndpointOptions options;

        public AbstractEndpoint(IEndpointOptions options, IEnumerable<HttpMethod> allowedMethods)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.AllowedMethods = allowedMethods ?? new List<HttpMethod> { HttpMethod.Get };
        }

        public virtual string Id => options.Id;

        public virtual bool Enabled => options.Enabled.Value;

        public virtual bool Sensitive => options.Sensitive.Value;

        public virtual IEndpointOptions Options => options;

        public string Path => options.Path;

        public IEnumerable<HttpMethod> AllowedMethods { get; }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public abstract class AbstractEndpoint<TResult> : AbstractEndpoint, IEndpoint<TResult>
    {
        public AbstractEndpoint(IEndpointOptions options, IEnumerable<HttpMethod> allowedMethods = null)
            : base(options, allowedMethods)
        {
        }

        public virtual TResult Invoke()
        {
            return default(TResult);
        }
    }

    public abstract class AbstractEndpoint<TResult, TRequest> : AbstractEndpoint, IEndpoint<TResult, TRequest>
    {
        public AbstractEndpoint(IEndpointOptions options, IEnumerable<HttpMethod> allowedMethods = null)
            : base(options, allowedMethods)
        {
        }

        public virtual TResult Invoke(TRequest arg)
        {
            return default(TResult);
        }
    }
#pragma warning restore SA1402 // File may only contain a single class
}

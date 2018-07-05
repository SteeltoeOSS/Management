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

using System.Collections.Generic;
using System.Net.Http;

namespace Steeltoe.Management.Endpoint
{
    public interface IEndpoint
    {
        string Id { get; }

        bool Enabled { get; }

        bool Sensitive { get; }

        IEndpointOptions Options { get; }

        string Path { get; }

        /// <summary>
        /// Evaluate the path and verb of an Http request to determine if it should be processed by this endpoint
        /// </summary>
        /// <param name="httpMethod">Request Verb (eg: GET, POST, PUT, etc)</param>
        /// <param name="requestPath">Request path</param>
        /// <returns>A determination of whether the request was meant for this endpoint</returns>
        bool RequestVerbAndPathMatch(string httpMethod, string requestPath);
    }

    public interface IEndpoint<TResult> : IEndpoint
    {
        TResult Invoke();
    }

    public interface IEndpoint<TResult, TRequest> : IEndpoint
    {
        TResult Invoke(TRequest arg);
    }
}

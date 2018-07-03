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
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Steeltoe.Management.EndpointOwin.Trace
{
    public class RequestTracingOwinMiddleware : OwinMiddleware
    {
        private ILogger<RequestTracingOwinMiddleware> _logger;
        private OwinTraceRepository _traceRepository;

        public RequestTracingOwinMiddleware(OwinMiddleware next, OwinTraceRepository traceRepository, ILogger<RequestTracingOwinMiddleware> logger = null)
            : base(next)
        {
            _traceRepository = traceRepository ?? throw new ArgumentNullException(nameof(traceRepository));
            _logger = logger;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await Next.Invoke(context);

            stopWatch.Stop();
            _traceRepository.RecordTrace(context, stopWatch.Elapsed);
        }
    }
}

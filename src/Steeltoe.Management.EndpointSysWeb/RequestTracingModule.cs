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
using Steeltoe.Management.EndpointOwin.Trace;
using System;
using System.Diagnostics;
using System.Web;

namespace Steeltoe.Management.EndpointSysWeb.Actuators
{
    public class RequestTracingModule : IHttpModule
    {
        private WebTraceRepository _traceRepository;
        private ILogger<RequestTracingModule> _logger;

        public RequestTracingModule(WebTraceRepository traceRepository, ILogger<RequestTracingModule> logger = null)
        {
            _traceRepository = traceRepository ?? throw new NullReferenceException(nameof(traceRepository));
            _logger = logger;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(BeginRequestTrace);
            context.EndRequest += new EventHandler(EndRequestTrace);
        }

        private void BeginRequestTrace(object sender, EventArgs e)
        {
            var stopWatch = new Stopwatch();
            HttpContext.Current.Items["Stopwatch"] = stopWatch;
            stopWatch.Start();
        }

        private void EndRequestTrace(object sender, EventArgs e)
        {
            Stopwatch stopWatch = (Stopwatch)HttpContext.Current.Items["Stopwatch"];
            stopWatch.Stop();
            _traceRepository.RecordTrace(HttpContext.Current, stopWatch.Elapsed);
        }
    }
}

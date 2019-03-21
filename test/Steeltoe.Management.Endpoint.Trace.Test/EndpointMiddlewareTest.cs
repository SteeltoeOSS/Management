﻿//
// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Steeltoe.Management.Endpoint.Trace.Test
{
    public class EndpointMiddlewareTest  : BaseTest
    {
        [Fact]
        public void IsTraceRequest_ReturnsExpected()
        {
            var opts = new TraceOptions();
            DiagnosticListener listener = new DiagnosticListener("test");

            TraceObserver obs = new TraceObserver(listener, opts);
            var ep = new TraceEndpoint(opts, obs);
            var middle = new TraceEndpointMiddleware(null, ep);
            var context = CreateRequest("GET", "/trace");
            Assert.True(middle.IsTraceRequest(context));
            var context2 = CreateRequest("PUT", "/trace");
            Assert.False(middle.IsTraceRequest(context2));
            var context3 = CreateRequest("GET", "/badpath");
            Assert.False(middle.IsTraceRequest(context3));
            listener.Dispose();
        }

        [Fact]
        public async void HandleTraceRequestAsync_ReturnsExpected()
        {
            var opts = new TraceOptions();
            DiagnosticListener listener = new DiagnosticListener("test");

            TraceObserver obs = new TraceObserver(listener, opts);
            var ep = new TestTraceEndpoint(opts, obs);
            var middle = new TraceEndpointMiddleware(null, ep);
            var context = CreateRequest("GET", "/trace");
            await middle.HandleTraceRequestAsync(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new StreamReader(context.Response.Body);
            string json = await rdr.ReadToEndAsync();
            Assert.Equal("[]", json);
            listener.Dispose();

        }

        [Fact]
        public async void TraceActuator_ReturnsExpectedData()
        {

            var builder = new WebHostBuilder().UseStartup<Startup>();
            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var result = await client.GetAsync("http://localhost/cloudfoundryapplication/trace");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var json = await result.Content.ReadAsStringAsync();
               Assert.NotNull(json);

            }
        }

        private HttpContext CreateRequest(string method, string path)
        {
            HttpContext context = new DefaultHttpContext();
            context.TraceIdentifier = Guid.NewGuid().ToString();
            context.Response.Body = new MemoryStream();
            context.Request.Method = method;
            context.Request.Path = new PathString(path);
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            return context;
        }

    }
    class TestTraceEndpoint : TraceEndpoint
    {
        public TestTraceEndpoint(ITraceOptions options, ITraceRepository traceRepository, ILogger<TraceEndpoint> logger = null) 
            : base(options, traceRepository, logger)
        {
        }
        public override List<Trace> Invoke()
        {
            return new List<Trace>();
        }
    }
}

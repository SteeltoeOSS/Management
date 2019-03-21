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
using System.IO;
using System.Net;
using System.Net.Http;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Steeltoe.Management.Endpoint.Loggers.Test
{
    public class EndpointMiddlewareTest  : BaseTest
    {
        [Fact]
        public void IsLoggersRequest_ReturnsExpected()
        {
            var opts = new LoggersOptions();
 
            var ep = new LoggersEndpoint(opts);
            var middle = new LoggersEndpointMiddleware(null, ep);

            var context = CreateRequest("GET", "/loggers");
            Assert.True(middle.IsLoggerRequest(context));

            var context2 = CreateRequest("PUT", "/loggers");
            Assert.False(middle.IsLoggerRequest(context2));

            var context3 = CreateRequest("GET", "/badpath");
            Assert.False(middle.IsLoggerRequest(context3));

            var context4 = CreateRequest("POST", "/loggers");
            Assert.True(middle.IsLoggerRequest(context4));

            var context5 = CreateRequest("POST", "/badpath");
            Assert.False(middle.IsLoggerRequest(context5));

            var context6 = CreateRequest("POST", "/loggers/Foo.Bar.Class");
            Assert.True(middle.IsLoggerRequest(context6));

            var context7 = CreateRequest("POST", "/badpath/Foo.Bar.Class");
            Assert.False(middle.IsLoggerRequest(context7));

        }

        [Fact]
        public async void HandleLoggersRequestAsync_ReturnsExpected()
        {
            var opts = new LoggersOptions();

            var ep = new TestLoggersEndpoint(opts);
            var middle = new LoggersEndpointMiddleware(null, ep);
            var context = CreateRequest("GET", "/loggers");
            await middle.HandleLoggersRequestAsync(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new StreamReader(context.Response.Body);
            string json = await rdr.ReadToEndAsync();
            Assert.Equal("{}", json);

        }

        [Fact]
        public async void LoggersActuator_ReturnsExpectedData()
        {

            var builder = new WebHostBuilder().UseStartup<Startup>();
            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var result = await client.GetAsync("http://localhost/cloudfoundryapplication/loggers");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var json = await result.Content.ReadAsStringAsync();
               Assert.NotNull(json);
         
                var loggers = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Assert.NotNull(loggers);
                Assert.True(loggers.ContainsKey("levels"));
                Assert.True(loggers.ContainsKey("loggers"));

            }
        }

        [Fact]
        public async void LoggersActuator_AcceptsPost()
        {

            var builder = new WebHostBuilder().UseStartup<Startup>();
            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var result = await client.GetAsync("http://localhost/cloudfoundryapplication/loggers");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var json = await result.Content.ReadAsStringAsync();
                Assert.NotNull(json);

                var loggers = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Assert.NotNull(loggers);
                Assert.True(loggers.ContainsKey("levels"));
                Assert.True(loggers.ContainsKey("loggers"));
                HttpContent content = new StringContent("{\"configuredLevel\":\"WARN\"}");
                var result2 = await client.PostAsync("http://localhost/cloudfoundryapplication/loggers/Steeltoe.Management.Endpoint.Loggers.LoggersEndpointMiddleware", content);
                Assert.Equal(HttpStatusCode.OK, result2.StatusCode);
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
    class TestLoggersEndpoint : LoggersEndpoint
    {
        public TestLoggersEndpoint(ILoggersOptions options, ILogger<LoggersEndpoint> logger = null) 
            : base(options, logger)
        {
        }
        public override Dictionary<string, object> Invoke(LoggersChangeRequest request)
        {
            return new Dictionary<string, object>();
        }
    }
}

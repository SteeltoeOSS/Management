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
using Xunit;
using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint.Info.Contributor;

namespace Steeltoe.Management.Endpoint.Info.Test
{
    public class EndpointMiddlewareTest : BaseTest
    {
        [Fact]
        public void IsInfoRequest_ReturnsExpected()
        {
            var opts = new InfoOptions();
            var contribs = new List<IInfoContributor>() { new GitInfoContributor() };
            var ep = new InfoEndpoint(opts, contribs);
            var middle = new InfoEndpointMiddleware(null, ep);

            var context = CreateRequest("GET", "/info");
            Assert.True(middle.IsInfoRequest(context));

            var context2 = CreateRequest("PUT", "/info");
            Assert.False(middle.IsInfoRequest(context2));

            var context3 = CreateRequest("GET", "/badpath");
            Assert.False(middle.IsInfoRequest(context3));

        }

        [Fact]
        public async void HandleInfoRequestAsync_ReturnsExpected()
        {
            var opts = new InfoOptions();
            var contribs = new List<IInfoContributor>() { new GitInfoContributor() };
            var ep = new TestInfoEndpoint(opts, contribs);
            var middle = new InfoEndpointMiddleware(null, ep);
            var context = CreateRequest("GET", "/loggers");
            await middle.HandleInfoRequestAsync(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new StreamReader(context.Response.Body);
            string json = await rdr.ReadToEndAsync();
            Assert.Equal("{}", json);

        }

        [Fact]
        public async void InfoActuator_ReturnsExpectedData()
        {
            // Note: This test pulls in from git.properties and appsettings created 
            // in the Startup class

            var builder = new WebHostBuilder().UseStartup<Startup>();
            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var result = await client.GetAsync("http://localhost/management/infomanagement");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var json = await result.Content.ReadAsStringAsync();
                Assert.NotNull(json);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);
                Assert.NotNull(dict);

                Assert.Equal(3, dict.Count);
                Assert.True(dict.ContainsKey("application"));
                Assert.True(dict.ContainsKey("NET"));
                Assert.True(dict.ContainsKey("git"));

                var appNode = dict["application"] as Dictionary<string, object>;
                Assert.NotNull(appNode);
                Assert.Equal("foobar", appNode["name"]);

                var netNode = dict["NET"] as Dictionary<string, object>;
                Assert.NotNull(netNode);
                Assert.Equal("Core", netNode["type"]);

                var gitNode = dict["git"] as Dictionary<string, object>;
                Assert.NotNull(gitNode);
                Assert.True(gitNode.ContainsKey("build"));
                Assert.True(gitNode.ContainsKey("branch"));
                Assert.True(gitNode.ContainsKey("commit"));
                Assert.True(gitNode.ContainsKey("closest"));
                Assert.True(gitNode.ContainsKey("dirty"));
                Assert.True(gitNode.ContainsKey("remote"));
                Assert.True(gitNode.ContainsKey("tags"));



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
    class TestInfoEndpoint : InfoEndpoint
    {
        public TestInfoEndpoint(IInfoOptions options, IEnumerable<IInfoContributor> contributors, ILogger<InfoEndpoint> logger = null) 
            : base(options, contributors, logger)
        {
        }
        public override Dictionary<string, object> Invoke()
        {
            return new Dictionary<string, object>();
        }
    }
}

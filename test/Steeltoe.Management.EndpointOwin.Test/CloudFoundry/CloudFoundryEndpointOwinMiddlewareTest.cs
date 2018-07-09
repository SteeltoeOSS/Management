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

using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Test;
using Steeltoe.Management.EndpointOwin.Test;
using System.Net;
using Xunit;

namespace Steeltoe.Management.EndpointOwin.CloudFoundry.Test
{
    public class CloudFoundryEndpointOwinMiddlewareTest : BaseTest
    {
        [Fact]
        public async void CloudFoundryInvoke_ReturnsExpected()
        {
            // arrange
            var middle = new CloudFoundryEndpointOwinMiddleware(null, new TestCloudFoundryEndpoint(new CloudFoundryOptions()));
            var context = OwinTestHelpers.CreateRequest("GET", "/");

            // act
            var json = await middle.InvokeAndReadResponse(context);

            // assert
            Assert.Equal("{\"type\":\"steeltoe\",\"_links\":{}}", json);
        }

        [Fact]
        public async void CloudFoundryHttpCall_ReturnsExpected()
        {
            using (var server = TestServer.Create<Startup>())
            {
                var client = server.HttpClient;
                var result = await client.GetAsync("http://localhost/cloudfoundryapplication");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var json = await result.Content.ReadAsStringAsync();
                Assert.NotNull(json);
                var links = JsonConvert.DeserializeObject<Links>(json);
                Assert.NotNull(links);
                Assert.True(links._links.ContainsKey("self"));
                Assert.Equal("http://localhost/cloudfoundryapplication", links._links["self"].href);
                Assert.True(links._links.ContainsKey("info"));
                Assert.Equal("http://localhost/cloudfoundryapplication/info", links._links["info"].href);
            }
        }

        [Fact]
        public async void CloudFoundryEndpoint_ServiceContractIsNotBroken()
        {
            // this test is here to prevent breaking changes in response serialization
            // arrange a server and client
            using (var server = TestServer.Create<Startup>())
            {
                var client = server.HttpClient;

                // send the request
                var result = await client.GetAsync("http://localhost/cloudfoundryapplication");
                var json = await result.Content.ReadAsStringAsync();

                // assert
                Assert.Equal("{\"type\":\"steeltoe\",\"_links\":{\"self\":{\"href\":\"http://localhost/cloudfoundryapplication\"},\"info\":{\"href\":\"http://localhost/cloudfoundryapplication/info\"}}}", json);
            }
        }
    }
}
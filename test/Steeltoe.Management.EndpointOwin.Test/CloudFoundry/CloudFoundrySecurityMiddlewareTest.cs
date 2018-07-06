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

using Microsoft.Extensions.Primitives;
using Microsoft.Owin.Testing;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.EndpointOwin.Test;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Steeltoe.Management.EndpointOwin.CloudFoundry.Test
{
    public class CloudFoundrySecurityMiddlewareTest : IDisposable
    {
        private readonly SecurityBase _base = new SecurityBase(new CloudFoundryOptions());

        public CloudFoundrySecurityMiddlewareTest()
        {
            Environment.SetEnvironmentVariable("VCAP_APPLICATION", "somestuff");
        }

        [Fact]
        public async void Incomplete_Config_ReturnsServiceUnavailable()
        {
            using (var server = TestServer.Create<StartupWithSecurity>())
            {
                var client = server.HttpClient;
                var result = await client.GetAsync("http://localhost/info");
                Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
                var response = await result.Content.ReadAsStringAsync();
                Assert.Contains(_base.APPLICATION_ID_MISSING_MESSAGE, response);
            }

            Environment.SetEnvironmentVariable("VCAP__APPLICATION__APPLICATION_ID", "foobar");
            using (var server = TestServer.Create<StartupWithSecurity>())
            {
                var client = server.HttpClient;
                var result = await client.GetAsync("http://localhost/info");
                Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
                var response = await result.Content.ReadAsStringAsync();
                Assert.Contains(_base.CLOUDFOUNDRY_API_MISSING_MESSAGE, response);
            }

            Environment.SetEnvironmentVariable("VCAP__APPLICATION__CF_API", "http://localhost:9999/foo");
            using (var server = TestServer.Create<StartupWithSecurity>())
            {
                var client = server.HttpClient;
                var result = await client.GetAsync("http://localhost/barfoo");
                Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
                var response = await result.Content.ReadAsStringAsync();
                Assert.Contains(_base.ENDPOINT_NOT_CONFIGURED_MESSAGE, response);
            }
        }

        [Fact]
        public async void No_AuthHeader_ReturnsUnauthorized()
        {
            Environment.SetEnvironmentVariable("VCAP__APPLICATION__APPLICATION_ID", "foobar");
            Environment.SetEnvironmentVariable("VCAP__APPLICATION__CF_API", "http://localhost:9999/foo");

            using (var server = TestServer.Create<StartupWithSecurity>())
            {
                var client = server.HttpClient;
                var result = await client.GetAsync("http://localhost/info");
                Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
                var response = await result.Content.ReadAsStringAsync();
                Assert.Contains(_base.AUTHORIZATION_HEADER_INVALID, response);
            }
        }

        [Fact]
        public async void SkipsSecurityCheck_WhenEnabledFalse()
        {
            Environment.SetEnvironmentVariable("management__endpoints__cloudfoundry__enabled", "false");

            using (var server = TestServer.Create<StartupWithSecurity>())
            {
                var client = server.HttpClient;
                var result = await client.GetAsync("http://localhost/info");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var response = await result.Content.ReadAsStringAsync();
                Assert.Equal("{\"application\":{\"date\":\"5/1/2008\",\"name\":\"foobar\",\"time\":\"8:30:52 AM\",\"version\":\"1.0.0\"},\"NET\":{\"ASPNET\":{\"type\":\"Core\",\"version\":\"2.0.0\"},\"type\":\"Core\",\"version\":\"2.0.0\"}}", response);
            }
        }

        [Fact]
        public void GetAccessToken_ReturnsExpected()
        {
            var opts = new CloudFoundryOptions();
            var middle = new CloudFoundrySecurityOwinMiddleware(null, opts, null);
            var context = OwinTestHelpers.CreateRequest("GET", "/");
            var token = middle.GetAccessToken(context.Request);
            Assert.Null(token);

            var context2 = OwinTestHelpers.CreateRequest("GET", "/");
            context2.Request.Headers.Add("Authorization", new StringValues("Bearer foobar"));
            var token2 = middle.GetAccessToken(context2.Request);
            Assert.Equal("foobar", token2);
        }

        [Fact]
        public async void GetPermissions_ReturnsExpected()
        {
            var opts = new CloudFoundryOptions();
            var middle = new CloudFoundrySecurityOwinMiddleware(null, opts, null);
            var context = OwinTestHelpers.CreateRequest("GET", "/");
            var result = await middle.GetPermissions(context);
            Assert.NotNull(result);
            Assert.Equal(Endpoint.Security.Permissions.NONE, result.Permissions);
            Assert.Equal(HttpStatusCode.Unauthorized, result.Code);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("VCAP_APPLICATION", null);
        }
    }
}

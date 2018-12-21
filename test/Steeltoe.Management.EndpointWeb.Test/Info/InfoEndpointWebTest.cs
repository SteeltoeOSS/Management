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
using Steeltoe.Management.Endpoint.Module;
using System;
using System.Web;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Steeltoe.Management.Endpoint.Handler;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.CloudFoundry;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Steeltoe.Management.Endpoint.Security;

namespace Steeltoe.Management.EndpointWeb.Test
{
    using Microsoft.Extensions.Configuration;

    using Newtonsoft.Json;

    public class InfoEndpointWebTest
    {
        private readonly ITestOutputHelper _output;
        private TestLoggerFactory loggerFactory;

        public InfoEndpointWebTest(ITestOutputHelper output)
        {
            loggerFactory = new TestLoggerFactory(output);
            _output = output;
        }

        private Dictionary<string, string> appSettings = new Dictionary<string, string>()
         {
             ["management:endpoints:enabled"] = "false",
             ["management:endpoints:sensitive"] = "false",
             ["management:endpoints:path"] = "/management",
             ["management:endpoints:info:enabled"] = "true",
             ["management:endpoints:info:sensitive"] = "false",
             ["management:endpoints:info:id"] = "infomanagement",
             ["info:application:name"] = "foobar",
             ["info:application:version"] = "1.0.0'",
             ["info:application:date"] = "5/1/2008",
             ["info:application:time"] = "8:30:52 AM",
             ["info:NET:type"] = "Core",
             ["info:NET:version"] = "2.0.0",
             ["info:NET:ASPNET:type"] = "Core",
             ["info:NET:ASPNET:version"] = "2.0.0"
         };

        [Fact]
        public async void InfoCall_ReturnsExpected()
        {
            var request = new TestRequest("GET", "localhost", "/management/infomanagement");
            var mockContext = GetMockContext(request);
            var response = string.Empty;
            mockContext.Setup(ctx => ctx.Response.Write(It.IsAny<string>())).Callback((string s) => response = s);

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(this.appSettings).Build();
            ManagementConfig.ConfigureManagementActuators(configuration,null,null,null);
            var options = new CloudFoundryOptions();

            var actuatorModule = new ActuatorModule(ActuatorConfigurator.ConfiguredHandlers, loggerFactory.CreateLogger<ActuatorModule>());

            await actuatorModule.FilterAndPreProcessRequest(mockContext.Object, () => _output.WriteLine("request completed"));

            Assert.NotEmpty(response);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response);
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

        private static Mock<HttpContextBase> GetMockContext(TestRequest request)
        {
            var mockContext = new Mock<HttpContextBase>();
            mockContext.Setup(ctx => ctx.Request.HttpMethod).Returns(request.HttpMethod);
            mockContext.Setup(ctx => ctx.Request.Path).Returns(request.Path);
            mockContext.Setup(ctx => ctx.Request.Headers).Returns(new NameValueCollection());
            mockContext.Setup(ctx => ctx.Response.Headers).Returns(new NameValueCollection());
            mockContext.Setup(ctx => ctx.Request.Url).Returns(request.Uri);
            return mockContext;
        }
    }
}

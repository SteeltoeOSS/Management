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
using Microsoft.Owin;
using Steeltoe.Management.Endpoint.Test;
using Steeltoe.Management.Endpoint.Trace;
using Steeltoe.Management.EndpointOwin.Test;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Steeltoe.Management.EndpointOwin.Trace.Test
{
    public class OwinTraceRepositoryTest : BaseTest
    {
        private TraceOptions traceOptions = new TraceOptions();

        [Fact]
        public void Constructor_ThrowsOnNullOptions()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new OwinTraceRepository(null));
            Assert.Equal("options", exception.ParamName);
        }

        [Fact]
        public void GetSessionId_NoSession_ReturnsExpected()
        {
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");
            var result = traceRepo.GetSessionId(context);
            Assert.Null(result);
        }

        ////[Fact]
        ////public void GetSessionId_WithSession_ReturnsExpected()
        ////{
        ////    var option = new TraceOptions();

        ////    var traceRepo = new OwinTraceRepository(option);
        ////    var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");

        ////    var session = new TestSession();
        ////    ISessionFeature sessFeature = new SessionFeature
        ////    {
        ////        Session = session
        ////    };
        ////    context.Features.Set<ISessionFeature>(sessFeature);

        ////    var result = traceRepo.GetSessionId(context);
        ////    Assert.Equal("TestSessionId", result);
        ////}

        [Fact]
        public void GetUserPrincipal_NotAuthenticated_ReturnsExpected()
        {
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");
            var result = traceRepo.GetUserPrincipal(context);
            Assert.Null(result);
        }

        [Fact]
        public void GetUserPrincipal_Authenticated_ReturnsExpected()
        {
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");

            context.Authentication.User = new ClaimsPrincipal(new MyIdentity());
            var result = traceRepo.GetUserPrincipal(context);
            Assert.Equal("MyTestName", result);
        }

        [Fact]
        public void GetRemoteAddress_NoConnection_ReturnsExpected()
        {
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");
            var result = traceRepo.GetRemoteAddress(context);
            Assert.Null(result);
        }

        [Fact]
        public void GetPathInfo_ReturnsExpected()
        {
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/myPath");

            var result = traceRepo.GetPathInfo(context.Request);
            Assert.Equal("/myPath", result);
        }

        [Fact]
        public async Task GetRequestParameters_ReturnsExpected()
        {
            // arrange
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");
            context.Request.QueryString = new QueryString("foo=bar&bar=foo");

            // act
            var result = await traceRepo.GetRequestParametersAsync(context.Request);

            // assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("foo"), "Result contains key 'foo'");
            Assert.True(result.ContainsKey("bar"), "Result contains key 'bar'");
            var fooVal = result["foo"];
            Assert.Single(fooVal);
            Assert.Equal("bar", fooVal[0]);
            var barVal = result["bar"];
            Assert.Single(barVal);
            Assert.Equal("foo", barVal[0]);
        }

        [Fact]
        public void GetTimeTaken_ReturnsExpected()
        {
            // arrange
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");
            TimeSpan time = TimeSpan.FromTicks(10000000);

            // act
            var result = traceRepo.GetTimeTaken(time);
            var expected = time.TotalMilliseconds.ToString();

            // assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetHeaders_ReturnsExpected()
        {
            // arrange
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");
            context.Request.QueryString = new QueryString("?foo=bar&bar=foo");
            context.Request.Headers.Add("Header1", new StringValues("header1Value"));
            context.Request.Headers.Add("Header2", new StringValues("header2Value"));

            // act
            var result = traceRepo.GetResponseHeaders(100, context.Request.Headers);

            // assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("header1"), "Result should contain header1");
            Assert.True(result.ContainsKey("header2"), "Result should contain header2");
            Assert.True(result.ContainsKey("status"), "Result should contain status");
            var header1Val = result["header1"] as string;
            Assert.Equal("header1Value", header1Val);
            var header2Val = result["header2"] as string;
            Assert.Equal("header2Value", header2Val);
            var statusVal = result["status"] as string;
            Assert.Equal("100", statusVal);
        }

        [Fact]
        public void MakeTrace_ReturnsExpected()
        {
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/myPath");
            TimeSpan duration = TimeSpan.FromTicks(20000000 - 10000000);
            TraceResult result = traceRepo.MakeTrace(context, duration);
            Assert.NotNull(result);
            Assert.NotNull(result.Info);
            Assert.NotEqual(0, result.TimeStamp);
            Assert.True(result.Info.ContainsKey("method"));
            Assert.True(result.Info.ContainsKey("path"));
            Assert.True(result.Info.ContainsKey("headers"));
            Assert.True(result.Info.ContainsKey("timeTaken"));
            Assert.Equal("GET", result.Info["method"]);
            Assert.Equal("/myPath", result.Info["path"]);
            var headers = result.Info["headers"] as Dictionary<string, object>;
            Assert.NotNull(headers);
            Assert.True(headers.ContainsKey("request"));
            Assert.True(headers.ContainsKey("response"));
            var timeTaken = result.Info["timeTaken"] as string;
            Assert.NotNull(timeTaken);
            var expected = duration.TotalMilliseconds.ToString();
            Assert.Equal(expected, timeTaken);
        }

        [Fact]
        public void RecordTrace_AddsToQueue()
        {
            // arrange
            var traceRepo = new OwinTraceRepository(traceOptions);
            var context = OwinTestHelpers.CreateRequest("GET", "/myPath");

            // act
            traceRepo.RecordTrace(context, new TimeSpan(0));

            // assert
            Assert.Single(traceRepo._queue);
            Assert.True(traceRepo._queue.TryPeek(out TraceResult result));
            Assert.NotNull(result.Info);
            Assert.NotEqual(0, result.TimeStamp);
            Assert.True(result.Info.ContainsKey("method"));
            Assert.True(result.Info.ContainsKey("path"));
            Assert.True(result.Info.ContainsKey("headers"));
            Assert.True(result.Info.ContainsKey("timeTaken"));
            Assert.Equal("GET", result.Info["method"]);
            Assert.Equal("/myPath", result.Info["path"]);
            var headers = result.Info["headers"] as Dictionary<string, object>;
            Assert.NotNull(headers);
            Assert.True(headers.ContainsKey("request"));
            Assert.True(headers.ContainsKey("response"));
            var timeTaken = result.Info["timeTaken"] as string;
            Assert.NotNull(timeTaken);
            Assert.Equal("0", timeTaken);
        }

        [Fact]
        public void RecordTracesAndGetThemLater_StayingWithinQueueCapacity()
        {
            // arrange
            var traceRepo = new OwinTraceRepository(traceOptions);

            // act
            for (int i = 0; i < 200; i++)
            {
                var context = OwinTestHelpers.CreateRequest("GET", "/doNotCare");
                traceRepo.RecordTrace(context, new TimeSpan(i));
            }

            // assert
            Assert.Equal(traceOptions.Capacity, traceRepo._queue.Count);
            List<TraceResult> traces = traceRepo.GetTraces();
            Assert.Equal(traceOptions.Capacity, traces.Count);
            Assert.Equal(traceOptions.Capacity, traceRepo._queue.Count);
        }
    }
}

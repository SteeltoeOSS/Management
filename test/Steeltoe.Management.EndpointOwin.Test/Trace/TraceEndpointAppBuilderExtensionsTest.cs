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

using Microsoft.Extensions.Configuration;
using Microsoft.Owin.Builder;
using Owin;
using Steeltoe.Management.Endpoint.Trace;
using Steeltoe.Management.EndpointOwin.Test;
using System;
using Xunit;

namespace Steeltoe.Management.EndpointOwin.Trace.Test
{
    public class TraceEndpointAppBuilderExtensionsTest : OwinBaseTest
    {
        [Fact]
        public void UseTraceActuator_ThrowsIfBuilderNull()
        {
            IAppBuilder builder = null;
            var config = new ConfigurationBuilder().Build();
            var traceRepo = new TraceDiagnosticObserver(new TraceOptions(config));

            var exception = Assert.Throws<ArgumentNullException>(() => builder.UseTraceEndpointMiddleware(config, traceRepo));
            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void UseTraceActuator_ThrowsIfConfigNull()
        {
            IAppBuilder builder = new AppBuilder();
            var traceRepo = new TraceDiagnosticObserver(new TraceOptions());

            var exception = Assert.Throws<ArgumentNullException>(() => builder.UseTraceEndpointMiddleware(null, traceRepo));
            Assert.Equal("config", exception.ParamName);
        }
    }
}

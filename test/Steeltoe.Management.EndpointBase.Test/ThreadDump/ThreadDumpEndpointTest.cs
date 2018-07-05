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

using Steeltoe.Management.Endpoint.Test;
using System;
using Xunit;

namespace Steeltoe.Management.Endpoint.ThreadDump.Test
{
    public class ThreadDumpEndpointTest : BaseTest
    {
        [Fact]
        public void Constructor_ThrowsIfNullRepo()
        {
            Assert.Throws<ArgumentNullException>(() => new ThreadDumpEndpoint(new ThreadDumpOptions(), null));
        }

        [Fact]
        public void Invoke_CallsDumpThreads()
        {
            var dumper = new TestThreadDumper();
            var ep = new ThreadDumpEndpoint(new ThreadDumpOptions(), dumper);
            var result = ep.Invoke();
            Assert.NotNull(result);
            Assert.True(dumper.DumpThreadsCalled);
        }

        [Fact]
        public void DumpRequest_PathAndVerbMatching_ReturnsExpected()
        {
            var opts = new ThreadDumpOptions();
            var obs = new ThreadDumper(opts);
            var ep = new ThreadDumpEndpoint(opts, obs);

            Assert.True(ep.RequestVerbAndPathMatch("GET", "/dump"));
            Assert.False(ep.RequestVerbAndPathMatch("PUT", "/dump"));
            Assert.False(ep.RequestVerbAndPathMatch("GET", "/badpath"));
        }
    }
}

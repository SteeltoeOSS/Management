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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steeltoe.Management.Endpoint.Test;
using Xunit;

namespace Steeltoe.Management.Endpoint.Health.Test
{
    public class HealthEndpointTest : BaseTest
    {
        [Fact]
        public void Constructor_ThrowsOptionsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new HealthEndpoint(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new HealthEndpoint(new HealthOptions(), null, null));
            Assert.Throws<ArgumentNullException>(() => new HealthEndpoint(new HealthOptions(), new DefaultHealthAggregator(), null));
        }

        [Fact]
        public void Invoke_NoContributors_ReturnsExpectedHealth()
        {

            var opts = new HealthOptions();
            var contributors = new List<IHealthContributor>();
            var agg = new DefaultHealthAggregator();
            var ep = new HealthEndpoint(opts, agg, contributors, GetLogger<HealthEndpoint>());

            var health = ep.Invoke();
            Assert.NotNull(health);
            Assert.Equal(HealthStatus.UNKNOWN, health.Status);

        }

        [Fact]
        public void Invoke_CallsAllContributors()
        {
            var opts = new HealthOptions();
            var contributors = new List<IHealthContributor>() { new TestContrib("h1"), new TestContrib("h2"), new TestContrib("h3") };
            var ep = new HealthEndpoint(opts, new DefaultHealthAggregator(), contributors);

            var info = ep.Invoke();

            foreach (var contrib in contributors)
            {
                TestContrib tc = (TestContrib)contrib;
                Assert.True(tc.called);
            }
        }

        [Fact]
        public void Invoke_HandlesExceptions_ReturnsExpectedHealth()
        {
            var opts = new HealthOptions();
            var contributors = new List<IHealthContributor>() { new TestContrib("h1"), new TestContrib("h2",true), new TestContrib("h3") };
            var ep = new HealthEndpoint(opts, new DefaultHealthAggregator(), contributors);

            var info = ep.Invoke();

            foreach (var contrib in contributors)
            {
                TestContrib tc = (TestContrib)contrib;
                if (tc.throws)
                    Assert.False(tc.called);
                else
                    Assert.True(tc.called);
            }
            Assert.Equal(HealthStatus.UP, info.Status);
        }

    }
    class TestContrib : IHealthContributor
    {
  
        public string Id { get; private set; } 

        public Health Health()
        {
            if (throws)
            {
                throw new Exception();
            }
            called = true;
            return new Health()
            {
                Status = HealthStatus.UP
            };

        }
        public bool called = false;
        public bool throws = false;
        public TestContrib(string id)
        {
            this.Id = id;
            this.throws = false;
        }
        public TestContrib(string id, bool throws)
        {
            this.Id = id;
            this.throws = throws;
        }
 
    }
}

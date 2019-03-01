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

using Newtonsoft.Json;
using OpenCensus.Stats;
using OpenCensus.Stats.Measures;
using OpenCensus.Tags;
using Steeltoe.Management.Census.Stats;
using System;
using System.Collections.Generic;

namespace Steeltoe.Management.Exporter.Metrics.CloudFoundryForwarder.Test
{
    public class BaseTest
    {
        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }

        public void SetupTestView(OpenCensusStats stats, IAggregation agg, IMeasure measure = null, string viewName = "test.test")
        {
            ITagKey aKey = TagKey.Create("a");
            ITagKey bKey = TagKey.Create("b");
            ITagKey cKey = TagKey.Create("c");

            if (measure == null)
            {
                measure = MeasureDouble.Create(Guid.NewGuid().ToString(), "test", MeasureUnit.Bytes);
            }

            IViewName testViewName = ViewName.Create(viewName);
            IView testView = View.Create(
                                        testViewName,
                                        "test",
                                        measure,
                                        agg,
                                        new List<ITagKey>() { aKey, bKey, cKey });

            stats.ViewManager.RegisterView(testView);
        }
    }
}

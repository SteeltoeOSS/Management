﻿// Copyright 2017 the original author or authors.
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

using OpenCensus.Stats;
using Steeltoe.Management.Census.Stats;

namespace Steeltoe.Management.Exporter.Metrics.CloudFoundryForwarder.Test
{
    public class OpenCensusStats : IStats
    {
        private IStatsComponent statsComponent = new StatsComponent();

        public OpenCensusStats()
        {
        }

        public IStatsRecorder StatsRecorder
        {
            get
            {
                return statsComponent.StatsRecorder;
            }
        }

        public IViewManager ViewManager
        {
            get
            {
                return statsComponent.ViewManager;
            }
        }

        public StatsCollectionState State
        {
            get
            {
                return statsComponent.State;
            }

            set
            {
                ((StatsComponent)statsComponent).State = value;
            }
        }
    }
}

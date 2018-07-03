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

using Microsoft.Owin;
using Steeltoe.Management.Endpoint.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;

// REVIEW: originally from https://www.azurefromthetrenches.com/capturing-and-tracing-all-http-requests-in-c-and-net/
// the result is much less information than our other trace approaches... can we get more request info from this angle?
namespace Steeltoe.Management.EndpointOwin.Trace
{
    public class HttpEventListener : EventListener, ITraceRepository
    {
        private class HttpEvent
        {
            public Stopwatch Stopwatch { get; set; }

            public string Url { get; set; }

            public string Verb { get; set; }

            public DateTimeOffset RequestedAt { get; set; }
        }

        private readonly ConcurrentDictionary<Guid, HttpEvent> _trackedEvents = new ConcurrentDictionary<Guid, HttpEvent>();
        private readonly ConcurrentBag<TraceResult> _traces = new ConcurrentBag<TraceResult>();

        public List<TraceResult> GetTraces()
        {
            return _traces.ToList();
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource != null /*&& eventSource.Name == "System.Diagnostics.Eventing.FrameworkEventSource"*/)
            {
                EnableEvents(eventSource, EventLevel.LogAlways, (EventKeywords)4);
            }

            base.OnEventSourceCreated(eventSource);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData?.Payload == null)
            {
                return;
            }

            try
            {
                switch (eventData.EventName)
                {
                    case "RequestStarted":
                        OnBeginHttpResponse(eventData);
                        break;
                    case "RequestCompleted":
                        OnEndHttpResponse(eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // don't let the tracer break due to frailities underneath, you might want to consider unbinding it
            }
        }

        private void OnBeginHttpResponse(EventWrittenEventArgs httpEventData)
        {
            if (httpEventData.Payload.Count < 2)
            {
                return;
            }

            int indexOfUrl = httpEventData.PayloadNames.IndexOf("FullUrl");
            int indexOfVerb = httpEventData.PayloadNames.IndexOf("HttpVerb");

            if (indexOfUrl == -1 || indexOfVerb == -1)
            {
                return;
            }

            Guid id = httpEventData.ActivityId;
            string url = Convert.ToString(httpEventData.Payload[indexOfUrl], CultureInfo.InvariantCulture);
            string verb = Convert.ToString(httpEventData.Payload[indexOfVerb], CultureInfo.InvariantCulture);
            _trackedEvents[id] = new HttpEvent
            {
                Url = new Uri(url).PathAndQuery,
                Verb = verb,
                Stopwatch = new Stopwatch(),
                RequestedAt = DateTimeOffset.UtcNow
            };
            _trackedEvents[id].Stopwatch.Start();
        }

        private void OnEndHttpResponse(EventWrittenEventArgs httpEventData)
        {
            if (_trackedEvents.TryRemove(httpEventData.ActivityId, out HttpEvent trackedEvent))
            {
                trackedEvent.Stopwatch.Stop();
                var trace = new TraceResult(trackedEvent.Stopwatch.ElapsedMilliseconds, new Dictionary<string, object> { { "path", trackedEvent.Url } });
                _traces.Add(trace);
            }
        }
    }
}
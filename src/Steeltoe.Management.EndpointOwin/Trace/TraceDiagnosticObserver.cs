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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Owin;
using Steeltoe.Common.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Steeltoe.Management.Endpoint.Trace
{
    public class TraceDiagnosticObserver : DiagnosticObserver, ITraceRepository
    {
        internal ConcurrentQueue<Trace> _queue = new ConcurrentQueue<Trace>();

        // REVIEW: this is obviously very wrong
        private const string DIAGNOSTIC_NAME = "Microsoft.AspNetCore";
        private const string OBSERVER_NAME = "TraceDiagnosticObserver";
        private const string STOP_EVENT = "Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop";

        private static DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private ILogger<TraceDiagnosticObserver> _logger;
        private ITraceOptions _options;

        public TraceDiagnosticObserver(ITraceOptions options, ILogger<TraceDiagnosticObserver> logger = null)
            : base(OBSERVER_NAME, DIAGNOSTIC_NAME, logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public List<Trace> GetTraces()
        {
            Trace[] traces = _queue.ToArray();
            return new List<Trace>(traces);
        }

        public override void ProcessEvent(string key, object value)
        {
            if (!STOP_EVENT.Equals(key))
            {
                return;
            }

            Activity current = Activity.Current;
            if (current == null)
            {
                return;
            }

            if (value == null)
            {
                return;
            }

            GetProperty(value, out IOwinContext context);

            if (context != null)
            {
                Trace trace = MakeTrace(context, current.Duration);
                _queue.Enqueue(trace);

                if (_queue.Count > _options.Capacity)
                {
                    if (!_queue.TryDequeue(out Trace discard))
                    {
                        _logger?.LogDebug("Stop - Dequeue failed");
                    }
                }
            }
        }

        protected internal Trace MakeTrace(IOwinContext context, TimeSpan duration)
        {
            var request = context.Request;
            var response = context.Response;

            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { "method", request.Method },
                { "path", GetPathInfo(request) }
            };

            Dictionary<string, object> headers = new Dictionary<string, object>();
            details.Add("headers", headers);

            if (_options.AddRequestHeaders)
            {
                headers.Add("request", GetHeaders(request.Headers));
            }

            if (_options.AddResponseHeaders)
            {
                headers.Add("response", GetHeaders(response.StatusCode, response.Headers));
            }

            if (_options.AddPathInfo)
            {
                details.Add("pathInfo", GetPathInfo(request));
            }

            if (_options.AddUserPrincipal)
            {
                details.Add("userPrincipal", GetUserPrincipal(context));
            }

            if (_options.AddParameters)
            {
                details.Add("parameters", GetRequestParameters(request));
            }

            if (_options.AddQueryString)
            {
                details.Add("query", request.QueryString.Value);
            }

            if (_options.AddAuthType)
            {
                details.Add("authType", GetAuthType(request)); // TODO
            }

            if (_options.AddRemoteAddress)
            {
                details.Add("remoteAddress", GetRemoteAddress(context));
            }

            if (_options.AddSessionId)
            {
                details.Add("sessionId", GetSessionId(context));
            }

            if (_options.AddTimeTaken)
            {
                details.Add("timeTaken", GetTimeTaken(duration));
            }

            return new Trace(GetJavaTime(DateTime.Now.Ticks), details);
        }

        protected internal long GetJavaTime(long ticks)
        {
            long javaTicks = ticks - baseTime.Ticks;
            return javaTicks / 10000;
        }

        protected internal string GetSessionId(IOwinContext context)
        {
            // REVIEW: accessing session in OWIN is... not this easy
            //var sessionFeature = context.Features.Get<ISessionFeature>();
            //return sessionFeature == null ? null : context.Session.Id;
            return "Not Implemented";
        }

        protected internal string GetTimeTaken(TimeSpan duration)
        {
            long timeInMilli = (long)duration.TotalMilliseconds;
            return timeInMilli.ToString();
        }

        protected internal string GetAuthType(IOwinRequest request)
        {
            return string.Empty;
        }

        protected internal async Task<Dictionary<string, string[]>> GetRequestParameters(IOwinRequest request)
        {
            Dictionary<string, string[]> parameters = new Dictionary<string, string[]>();
            var query = request.Query;
            foreach (var p in query)
            {
                parameters.Add(p.Key, p.Value);
            }

            if (HasFormContentType(request))
            {
                var formData = await request.ReadFormAsync();
                foreach (var p in formData)
                {
                    parameters.Add(p.Key, p.Value);
                }
            }

            return parameters;
        }

        protected internal string GetRequestUri(IOwinRequest request)
        {
            return request.Scheme + "://" + request.Host.Value + request.Path.Value;
        }

        protected internal string GetPathInfo(IOwinRequest request)
        {
            return request.Path.Value;
        }

        protected internal string GetUserPrincipal(IOwinContext context)
        {
            return context?.Request?.User?.Identity?.Name;
        }

        protected internal string GetRemoteAddress(IOwinContext context)
        {
            return context?.Request?.RemoteIpAddress?.ToString();
        }

        protected internal Dictionary<string, object> GetHeaders(int status, IHeaderDictionary headers)
        {
            var result = GetHeaders(headers);
            result.Add("status", status.ToString());
            return result;
        }

        protected internal Dictionary<string, object> GetHeaders(IHeaderDictionary headers)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var h in headers)
            {
                // Add filtering
                result.Add(h.Key.ToLowerInvariant(), GetHeaderValue(h.Value));
            }

            return result;
        }

        protected internal object GetHeaderValue(StringValues values)
        {
            List<string> result = new List<string>();
            foreach (var v in values)
            {
                result.Add(v);
            }

            if (result.Count == 1)
            {
                return result[0];
            }

            if (result.Count == 0)
            {
                return string.Empty;
            }

            return result;
        }

        protected internal void GetProperty(object obj, out IOwinContext context)
        {
            context = DiagnosticHelpers.GetProperty<IOwinContext>(obj, "HttpContext");
        }

        private bool HasFormContentType(IOwinRequest request)
        {
            return request.MediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) || request.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase);
        }
    }
}

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
using Microsoft.Owin;
using Steeltoe.Management.Endpoint.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Steeltoe.Management.EndpointOwin.Metrics
{
    public class MetricsEndpointOwinMiddleware : EndpointOwinMiddleware<IMetricsResponse, MetricsRequest>
    {
        protected new MetricsEndpoint _endpoint;

        public MetricsEndpointOwinMiddleware(OwinMiddleware next, MetricsEndpoint endpoint, ILogger logger)
            : base(next, logger)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!IsMetricsRequest(context))
            {
                await Next.Invoke(context);
            }
            else
            {
                await HandleMetricsRequestAsync(context);
            }
        }

        public string HandleRequest(MetricsRequest arg)
        {
            var result = _endpoint.Invoke(arg);
            return result == null ? null : Serialize(result);
        }

        protected internal async Task HandleMetricsRequestAsync(IOwinContext context)
        {
            IOwinRequest request = context.Request;
            IOwinResponse response = context.Response;

            _logger?.LogDebug("Incoming path: {0}", request.Path.Value);

            string metricName = GetMetricName(request);
            if (!string.IsNullOrEmpty(metricName))
            {
                // GET /metrics/{metricName}?tag=key:value&tag=key:value
                var tags = ParseTags(request.Query);
                var metricRequest = new MetricsRequest(metricName, tags);
                var serialInfo = HandleRequest(metricRequest);

                if (serialInfo != null)
                {
                    await context.Response.WriteAsync(serialInfo);
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                // GET /metrics
                var serialInfo = this.HandleRequest(null);
                _logger?.LogDebug("Returning: {0}", serialInfo);
                response.Headers.SetValues("Content-Type", new string[] { "application/vnd.spring-boot.actuator.v1+json" });
                await context.Response.WriteAsync(serialInfo);
                response.StatusCode = (int)HttpStatusCode.OK;
            }
        }

        protected internal string GetMetricName(IOwinRequest request)
        {
            PathString epPath = new PathString(_endpoint.Path);
            if (request.Path.StartsWithSegments(epPath, out PathString remaining))
            {
                if (remaining.HasValue)
                {
                    return remaining.Value.TrimStart('/');
                }
            }

            return null;
        }

        protected internal List<KeyValuePair<string, string>> ParseTags(IReadableStringCollection query)
        {
            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();
            if (query == null)
            {
                return results;
            }

            foreach (var q in query)
            {
                if (q.Key.Equals("tag", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var kvp in q.Value)
                    {
                        var pair = ParseTag(kvp);
                        if (pair != null)
                        {
                            if (!results.Contains(pair.Value))
                            {
                                results.Add(pair.Value);
                            }
                        }
                    }
                }
            }

            return results;
        }

        protected internal KeyValuePair<string, string>? ParseTag(string kvp)
        {
            var str = kvp.Split(new char[] { ':' }, 2);
            if (str != null && str.Length == 2)
            {
                return new KeyValuePair<string, string>(str[0], str[1]);
            }

            return null;
        }

        protected internal bool IsMetricsRequest(IOwinContext context)
        {
            if (!context.Request.Method.Equals("GET"))
            {
                return false;
            }

            PathString path = new PathString(_endpoint.Path);
            return context.Request.Path.StartsWithSegments(path);
        }
    }
}

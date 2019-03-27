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

using Microsoft.AspNetCore.Http;
using Steeltoe.Management.Endpoint.Middleware;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace Steeltoe.Management.Endpoint.Loggers
{

    public class LoggersEndpointMiddleware : EndpointMiddleware<Dictionary<string,object>, LoggersChangeRequest>
    {
        private RequestDelegate _next;

        public LoggersEndpointMiddleware(RequestDelegate next, LoggersEndpoint endpoint, ILogger<LoggersEndpointMiddleware> logger = null)
            : base(endpoint, logger)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsLoggerRequest(context))
            {
                await HandleLoggersRequestAsync(context);
            }
            else
            {
                await _next(context);
            }
        }

        internal protected async Task HandleLoggersRequestAsync(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            if (context.Request.Method.Equals("POST"))
            {
                // POST - change a logger level
                logger?.LogDebug("Incoming path: {0}", request.Path.Value);
                PathString epPath = new PathString(endpoint.Path);
                PathString remaining;
                if (request.Path.StartsWithSegments(epPath, out remaining))
                {
                    if (remaining.HasValue) {
                        string loggerName = remaining.Value.TrimStart('/');

                        var change = Deserialize(request.Body);

                        string level = null;
                        change.TryGetValue("configuredLevel", out level);

                        logger?.LogDebug("Change Request: {0}, {1}", loggerName, level);
                        if (!string.IsNullOrEmpty(loggerName) && !string.IsNullOrEmpty(level))
                        {
                            var changeReq = new LoggersChangeRequest(loggerName, level);
                            base.HandleRequest(changeReq);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            return;
                        }
                    }
                }
       
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            
            // GET request
            var serialInfo = base.HandleRequest(null);
            logger?.LogDebug("Returning: {0}", serialInfo);
            response.Headers.Add("Content-Type", "application/vnd.spring-boot.actuator.v1+json");
            await context.Response.WriteAsync(serialInfo);

        }

        internal protected bool IsLoggerRequest(HttpContext context)
        {
            if (!context.Request.Method.Equals("GET") && !context.Request.Method.Equals("POST")) { return false; }
            PathString path = new PathString(endpoint.Path);
            return context.Request.Path.StartsWithSegments(path);
        }

  
        private Dictionary<string, string> Deserialize(Stream stream)
        {
            try
            {
                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return serializer.Deserialize<Dictionary<string,string>>(jsonTextReader);
                    }
                }
            } catch(Exception e)
            {
                logger?.LogError("Error {0} deserializing", e);
            }

            return new Dictionary<string, string>();
        }
    }
}

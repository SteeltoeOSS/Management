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

using CommonServiceLocator;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Loggers;
using Steeltoe.Management.EndpointSysWeb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(ActuatorModule<LoggersModule, LoggersEndpoint, Dictionary<string, object>, LoggersChangeRequest>), "Start")]
namespace Steeltoe.Management.EndpointSysWeb
{
    public class LoggersModule : ActuatorModule<LoggersModule, LoggersEndpoint, Dictionary<string, object>, LoggersChangeRequest>
    {
        // HELP: Not having any luck getting actual DI to work
        // https://haacked.com/archive/2011/06/03/dependency-injection-with-asp-net-httpmodules.aspx/
        public LoggersModule(/*LoggersEndpoint endpoint, ILogger<LoggersModule> logger = null*/)
            : base(ServiceLocator.Current.GetInstance<LoggersEndpoint>(), null)
        {
            //_endpoint = endpoint;
            //_logger = logger;
            //_endpoint = ServiceLocator.Current.GetInstance<IEndpoint<Dictionary<string, object>, LoggersChangeRequest>>();
            //_logger = ServiceLocator.Current.GetInstance<ILogger<LoggersModule>>() ?? null; // THROWS ...
            // To avoid this exception, either register a component to provide the service, check for service registration using IsRegistered(), or use the ResolveOptional() method to resolve an optional dependency.
        }

        protected override void HandleRequest(HttpContext context)
        {
            _logger?.LogTrace("Processing {SteeltoeEndpoint} request", typeof(LoggersEndpoint).Name);
            if (context.Request.HttpMethod == "GET")
            {
                // GET request
                var endpointResponse = _endpoint.Invoke(null);
                _logger?.LogTrace("Returning: {EndpointResponse}", endpointResponse);
                context.Response.Headers.Set("Content-Type", "application/vnd.spring-boot.actuator.v1+json");
                context.Response.Write(Serialize(endpointResponse));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }
            else
            {
                // POST - change a logger level
                _logger?.LogDebug("Incoming logger path: {0}", context.Request.Path);
                //PathString epPath = new PathString(_endpoint.Path);
                //if (context.Request.Path.StartsWithSegments(epPath, out PathString remaining))
                if (context.Request.Path.StartsWith(_endpoint.Path))
                {
                    //if (remaining.HasValue)
                    //{
                        //string loggerName = remaining.Value.TrimStart('/');
                        string loggerName = context.Request.Path.TrimStart('/');

                        var change = Deserialize(context.Request.InputStream);

                        change.TryGetValue("configuredLevel", out string level);

                        _logger?.LogDebug("Change Request: {Logger}, {Level}", loggerName, level ?? "RESET");
                        if (!string.IsNullOrEmpty(loggerName))
                        {
                            var changeReq = new LoggersChangeRequest(loggerName, level);
                            _endpoint.Invoke(changeReq);
                        }
                    //}
                }

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        // TODO: dedupe this method
        private Dictionary<string, string> Deserialize(Stream stream)
        {
            try
            {
                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return serializer.Deserialize<Dictionary<string, string>>(jsonTextReader);
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("Exception deserializing LoggersEndpoint Response: {Exception}", e);
            }

            return new Dictionary<string, string>();
        }
    }
}

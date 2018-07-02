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
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.EndpointSysWeb;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(ActuatorModule<HealthModule, HealthEndpoint, HealthCheckResult>), "Start")]
namespace Steeltoe.Management.EndpointSysWeb
{
    public class HealthModule : ActuatorModule<HealthModule, HealthEndpoint, HealthCheckResult>
    {
        // HELP: Not having any luck getting actual DI to work
        // https://haacked.com/archive/2011/06/03/dependency-injection-with-asp-net-httpmodules.aspx/
        public HealthModule(/*IEndpoint<HealthCheckResult> endpoint, ILogger<HealthModule> logger = null*/)
        {
            //_endpoint = endpoint;
            //_logger = logger;
            _endpoint = ServiceLocator.Current.GetInstance<IEndpoint<HealthCheckResult>>();
            _logger = ServiceLocator.Current.GetInstance<ILogger<HealthModule>>() ?? null; // THROWS ...
            // To avoid this exception, either register a component to provide the service, check for service registration using IsRegistered(), or use the ResolveOptional() method to resolve an optional dependency.
        }
    }
}

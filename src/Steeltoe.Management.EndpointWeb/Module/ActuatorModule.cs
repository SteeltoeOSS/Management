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
using Steeltoe.Management.Endpoint.Handler;
using Steeltoe.Management.Endpoint.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Steeltoe.Management.Endpoint.Module
{
    public class ActuatorModule : IHttpModule
    {
        protected ILogger<ActuatorModule> _logger;
        protected IDictionary<IEndpoint, IActuatorHandler> _endpoints;
        protected ISecurityService _securityService;

        public ActuatorModule()
            : base()
        {
        }

        public ActuatorModule(ISecurityService securityService, IDictionary<IEndpoint, IActuatorHandler> endpoints, ILogger<ActuatorModule> logger)
        {
            _logger = logger;
            _endpoints = endpoints;
            _securityService = securityService;
        }

        public virtual void Dispose()
        {
        }

        public virtual void Init(HttpApplication context)
        {
            if (_logger == null)
            {
                _logger = ActuatorConfiguration.LoggerFactory?.CreateLogger<ActuatorModule>();
            }

            if (_endpoints == null)
            {
                _endpoints = ActuatorConfiguration.ConfiguredEndpoints;
            }

            if (_securityService == null)
            {
                _securityService = ActuatorConfiguration.SecurityService;
            }

            EventHandlerTaskAsyncHelper asyncHelper = new EventHandlerTaskAsyncHelper(FilterAndPreProcessRequest);
            context.AddOnBeginRequestAsync(asyncHelper.BeginEventHandler, asyncHelper.EndEventHandler);
        }

        protected async virtual Task FilterAndPreProcessRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            if (_endpoints == null)
            {
                return;
            }

            foreach (var kvp in _endpoints)
            {
                var ep = kvp.Key;
                var handler = kvp.Value;

                if (ep.RequestVerbAndPathMatch(context.Request.HttpMethod, context.Request.Path))
                {
                    if (await _securityService?.IsAccessAllowed(context, ep.Options))
                    {
                        handler.HandleRequest(context);
                    }

                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
        }
    }
}

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
using System;
using System.Collections.Generic;

namespace Steeltoe.Management.Endpoint.CloudFoundry
{
    public class CloudFoundryEndpoint : AbstractEndpoint<Links, string>
    {
        private ILogger<CloudFoundryEndpoint> _logger;

        public CloudFoundryEndpoint(ICloudFoundryOptions options, ILogger<CloudFoundryEndpoint> logger = null)
            : base(options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _logger = logger;
        }

        protected new ICloudFoundryOptions Options => options as ICloudFoundryOptions;

        public override Links Invoke(string baseUrl)
        {
            var endpointOptions = Options.Global.EndpointOptions;
            var links = new Links();

            if (!Options.Enabled.Value)
            {
                return links;
            }

            foreach (var opt in endpointOptions)
            {
                if (!opt.Enabled.Value)
                {
                    continue;
                }

                if (opt == Options)
                {
                    links._links.Add("self", new Link(baseUrl));
                }
                else
                {
                    var ids = new List<string> {opt.Id};
                    opt.AltIds?.ForEach(id => ids.Add(id));
                    ids.ForEach(id =>
                    {
                        if (!links._links.ContainsKey(id))
                        {
                            links._links.Add(id, new Link(baseUrl + "/" + id));
                        }
                        else
                        {
                            _logger?.LogWarning("Duplicate endpoint id detected: {DuplicateEndpointId}", id);
                        }
                    });
                }
            }

            return links;
        }
        
    }
}

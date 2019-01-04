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

using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint.Security;
using Steeltoe.Management.EndpointBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Steeltoe.Management.Endpoint
{
    public abstract class AbstractOptions : IEndpointOptions
    {
        protected bool? _enabled;

        protected bool? _sensitive;

        public AbstractOptions()
        {
            Global = ManagementOptions.GetInstance();
            Global.EndpointOptions.Add(this);
        }

        public AbstractOptions(string sectionName, IConfiguration config)
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Global = ManagementOptions.GetInstance(config);
            Global.EndpointOptions.Add(this);

            var section = config.GetSection(sectionName);
            if (section != null)
            {
                section.Bind(this);
            }
        }

        public virtual bool IsEnabled
        {
            get
            {
                return Enabled.Value;
            }
        }

        public virtual bool? Enabled
        {
            get
            {
                if (_enabled.HasValue)
                {
                    return _enabled.Value;
                }
                else if (Global.Enabled.HasValue)
                {
                    return Global.Enabled;
                }
                else
                {
                    return DefaultEnabled;
                }
            }

            set
            {
                _enabled = value;
            }
        }

        public virtual bool IsSensitive
        {
            get
            {
                return Sensitive.Value;
            }
        }

        public virtual bool? Sensitive
        {
            get
            {
                if (_sensitive.HasValue)
                {
                    return _sensitive.Value;
                }
                else if (Global.Sensitive.HasValue)
                {
                    return Global.Sensitive;
                }
                else
                {
                    return DefaultSensitive;
                }
            }

            set
            {
                _sensitive = value;
            }
        }

        public virtual IManagementOptions Global { get; set; }

        public virtual string Id { get; set; }

        public List<string> AltIds { get; set; }

        public virtual List<string> Paths
        {
            get
            {
                var basePaths = new List<string> { Global.Path, Global.CloudFoundryPath };
                var ids = new List<string>(AltIds ?? new List<string>()) { Id };

                // Return a path for each combination of path and Id
                var paths = basePaths.SelectMany(
                        x => ids,
                        (p, id) => p.AddPath(id))
                    .Distinct().ToList();
                return paths;
            }
        }

        public Permissions RequiredPermissions { get; set; } = Permissions.UNDEFINED;

        public virtual bool IsAccessAllowed(Permissions permissions)
        {
            return permissions >= RequiredPermissions;
        }

        protected virtual bool DefaultEnabled { get; } = true;

        protected virtual bool DefaultSensitive { get; } = true;
    }
}

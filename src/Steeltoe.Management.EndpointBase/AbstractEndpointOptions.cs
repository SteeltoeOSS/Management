﻿using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint
{
    public abstract class AbstractEndpointOptions : IEndpointOptions
    {
        protected bool? _enabled;

        protected bool? _sensitive;

        protected string _path;

        public AbstractEndpointOptions()
        {
        }

        public AbstractEndpointOptions(string sectionName, IConfiguration config)
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var section = config.GetSection(sectionName);
            if (section != null)
            {
                section.Bind(this);
            }
        }

        [Obsolete]
        public virtual bool IsEnabled
        {
            get
            {
                return false; 
                
            }
        }

        public virtual bool? Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                _enabled = value;
            }
        }

        [Obsolete]
        public virtual bool IsSensitive
        {
            get { return false; }
        }

        public virtual bool? Sensitive
        {
            get
            {
                return _sensitive;
            }

            set
            {
                _sensitive = value;
            }
        }

        public virtual string Id { get; set; }

        public virtual string Path
        {
            get
            {
                if (!string.IsNullOrEmpty(_path))
                {
                    return _path;
                }

                return Id;
            }

            set
            {
                _path = value;
            }
        }

        public Permissions RequiredPermissions { get; set; } = Permissions.UNDEFINED;

        public IManagementOptions Global { get; set; }

        public virtual bool IsAccessAllowed(Permissions permissions)
        {
            return permissions >= RequiredPermissions;
        }

        public virtual bool DefaultEnabled { get; } = true;

        public virtual bool DefaultSensitive { get; } = true;
    }
}

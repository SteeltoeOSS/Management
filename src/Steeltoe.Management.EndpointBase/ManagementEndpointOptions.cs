using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint
{
    public class ManagementEndpointOptions : IManagementOptions
    {
        private const string DEFAULT_PATH = "/actuator";
        private const string MANAGEMENT_INFO_PREFIX = "management:endpoints";

        private bool? _enabled;
        private bool? _sensitive;

        public ManagementEndpointOptions()
        {
            Path = DEFAULT_PATH;
            EndpointOptions = new List<IEndpointOptions>();
        }

        public ManagementEndpointOptions(IConfiguration config)
            : this()
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var section = config.GetSection(MANAGEMENT_INFO_PREFIX);
            if (section != null)
            {
                section.Bind(this);
            }
        }

        public bool? Enabled
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

        public bool? Sensitive
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

        public string Path { get; set; }

        public List<IEndpointOptions> EndpointOptions { get; set; }
    }
}

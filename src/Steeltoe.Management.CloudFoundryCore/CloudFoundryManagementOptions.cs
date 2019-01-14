using Microsoft.Extensions.Configuration;
using Steeltoe.Management.Endpoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.CloudFoundry
{
    public class CloudFoundryManagementOptions : ManagementOptions
    {
        public CloudFoundryManagementOptions(IConfiguration config)
        : base(config)
        {
        }
    }
}

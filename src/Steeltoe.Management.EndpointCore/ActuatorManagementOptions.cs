using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Steeltoe.Management.Endpoint
{
    public class ActuatorManagementOptions : ManagementOptions
    {
        public ActuatorManagementOptions(IConfiguration config) 
            : base(config)
        {
        }
    }
}

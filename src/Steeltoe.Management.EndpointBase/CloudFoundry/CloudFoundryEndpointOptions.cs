﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint.CloudFoundry
{
    public class CloudFoundryEndpointOptions : AbstractEndpointOptions, ICloudFoundryOptions
    {
        private const string MANAGEMENT_INFO_PREFIX = "management:endpoints:cloudfoundry";
        private const string VCAP_APPLICATION_ID_KEY = "vcap:application:application_id";
        private const string VCAP_APPLICATION_CLOUDFOUNDRY_API_KEY = "vcap:application:cf_api";
        private const bool Default_ValidateCertificates = true;

        public CloudFoundryEndpointOptions()
            : base()
        {
            Id = string.Empty;
        }

        public CloudFoundryEndpointOptions(IConfiguration config)
            : base(MANAGEMENT_INFO_PREFIX, config)
        {
            Id = string.Empty;
            ApplicationId = config[VCAP_APPLICATION_ID_KEY];
            CloudFoundryApi = config[VCAP_APPLICATION_CLOUDFOUNDRY_API_KEY];
        }

        public bool ValidateCertificates { get; set; } = Default_ValidateCertificates;

        public string ApplicationId { get; set; }

        public string CloudFoundryApi { get; set; }
    }
}

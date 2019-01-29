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
using Steeltoe.Management.Endpoint.Security;
using Steeltoe.Management.Endpoint.ThreadDump;
using System;
using System.Collections.Generic;

namespace Steeltoe.Management.Endpoint.Handler
{
    public class ThreadDumpHandler : ActuatorHandler<ThreadDumpEndpoint, List<ThreadInfo>>
    {
        public ThreadDumpHandler(ThreadDumpEndpoint endpoint, ISecurityService securityService, IEnumerable<IManagementOptions> mgmtOptions, ILogger<ThreadDumpHandler> logger = null)
          : base(endpoint, securityService, mgmtOptions, null, true, logger)
        {
        }

        [Obsolete]
         public ThreadDumpHandler(ThreadDumpEndpoint endpoint, ISecurityService securityService, ILogger<ThreadDumpHandler> logger = null)
            : base(endpoint, securityService, null, true, logger)
        {
        }
    }
}

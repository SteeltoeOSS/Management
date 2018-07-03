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
using Steeltoe.Management.Endpoint.HeapDump;
using Steeltoe.Management.EndpointBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Steeltoe.Management.EndpointSysWeb
{
    public class HeapDumpModule : ActuatorModule<HeapDumpEndpoint, string>
    {
        public HeapDumpModule(HeapDumpEndpoint endpoint, ILogger<HeapDumpEndpoint> logger = null)
            : base(endpoint, logger)
        {
        }

        protected override void HandleRequest(HttpContext context)
        {
            var filename = _endpoint.Invoke();
            _logger?.LogDebug("Returning: {0}", filename);
            context.Response.Headers.Set("Content-Type", "application/octet-stream");

            if (!File.Exists(filename))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            string gzFilename = filename + ".gz";
            var result = Utils.CompressFileAsync(filename, gzFilename);

            if (result != null)
            {
                using (result)
                {
                    context.Response.Headers.Add("Content-Disposition", new string[] { "attachment; filename=\"" + Path.GetFileName(gzFilename) + "\"" });
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentLength = result.Length;
                    await result.CopyToAsync(context.Response.Body);
                }

                File.Delete(gzFilename);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steeltoe.Management.EndpointWeb.Test
{
    public class TestRequest
    {
        public TestRequest(string httpMethod, string host, string path)
        {
            Host = host;
            Path = path;
            HttpMethod = httpMethod;
        }

        public string Host { get; set; }

        public string Path { get; set; }

        public string HttpMethod { get; set; }

        public Uri Uri => new Uri($"http://{Host}{Path}");
    }
}

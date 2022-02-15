using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Diagnostics.Tracing
{
    public class TraceEndpoint : ITraceEndpoint
    {
        private string _endpoint;
        private string _category;

        public TraceEndpoint() { }

        public TraceEndpoint(string category, string endpoint)
        {
            this.Category = category;
            this.Endpoint = endpoint;
        }

        public string Endpoint { get => _endpoint; set => _endpoint = value?.ToLower(); }

        public string Category { get => _category; set => _category = value?.ToLower(); }
    }
}

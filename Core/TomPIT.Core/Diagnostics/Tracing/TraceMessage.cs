using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Diagnostics.Tracing
{
    public class TraceMessage : ITraceMessage
    {
        public TraceMessage() { }

        public TraceMessage(ITraceEndpoint endpoint, string content) 
        {
            this.Content = content;
            this.Endpoint = endpoint;
        }

        public TraceMessage(string category, string endpoint, string content) 
        {
            this.Content = content;
            this.Endpoint = new TraceEndpoint(category, endpoint);
        }

        public string Content { get; set; }

        public ITraceEndpoint Endpoint { get; set; }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Diagnostics.Tracing;

namespace TomPIT.Diagnostics
{
    public interface ITraceService
    {
        event EventHandler<ITraceMessage> TraceReceived;
        
        ConcurrentQueue<ITraceMessage> MessageQueue { get; }

        IEnumerable<string> Categories { get; }

        IEnumerable<ITraceEndpoint> Endpoints { get; }

        IEnumerable<ITraceEndpoint> GetEndpoints(string category);

        IEnumerable<ITraceEndpoint> GetEndpoints(IEnumerable<string> categories);

        void AddEndpoint(ITraceEndpoint endpoint);

        void AddEndpoint(string category, string endpoint);

        void Trace(ITraceMessage message);

        void Trace(string category, string endpoint, string message);
    }
}

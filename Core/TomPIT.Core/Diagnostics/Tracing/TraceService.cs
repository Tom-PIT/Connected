using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Collections;

namespace TomPIT.Diagnostics.Tracing
{
    public class TraceService : ITraceService
    {
        private ConcurrentBag<ITraceEndpoint> _endpoints;

        private ILookup<string, ITraceEndpoint> _endpointsCache;

        private readonly ConcurrentQueue<ITraceMessage> _messageQueue = new ();

        private readonly long _maxQueueSize = 10;

        private ILookup<string, ITraceEndpoint> EndpointsCache
        {
            get
            {
                if (_endpointsCache is null)
                    UpdateEndpointsCache();

                return _endpointsCache;
            }
        }

        public event EventHandler<ITraceMessage> TraceReceived;

        public IEnumerable<string> Categories => _endpointsCache?.Select(e => e.Key) ?? new List<string>();

        public IEnumerable<ITraceEndpoint> Endpoints => (_endpoints ??= new ConcurrentBag<ITraceEndpoint>());

        public ConcurrentQueue<ITraceMessage> MessageQueue => _messageQueue;

        public void AddEndpoint(string category, string endpoint)
        {
            var traceEndpoint = new TraceEndpoint(category.ToLower(), endpoint.ToLower());

            AddEndpoint(traceEndpoint);
        }

        public void AddEndpoint(ITraceEndpoint endpoint)
        {
            if (Endpoints.Contains(endpoint, TraceEndpointComparer.Instance))
                return;

            _endpoints.Add(endpoint);

            UpdateEndpointsCache();
        }

        public IEnumerable<ITraceEndpoint> GetEndpoints([DisallowNull] string category)
        {
            lock (EndpointsCache)
            {
                return EndpointsCache[category.ToLower()].ToImmutableList();
            }
        }

        public IEnumerable<ITraceEndpoint> GetEndpoints(IEnumerable<string> categories)
        {
            lock (EndpointsCache)
            {
                foreach (var category in categories)
                    foreach (var entry in EndpointsCache[category.ToLower()])
                        yield return entry;
            }
        }

        public void Trace([DisallowNull] ITraceMessage message)
        {
            AddEndpoint(message.Endpoint);

            //force flush to prevent memory overflow
            if (MessageQueue.Count > _maxQueueSize)
                new System.Threading.Thread(() => Flush()).Start();
            
            MessageQueue.Enqueue(message);
        }

        public void Trace([DisallowNull] string category, [DisallowNull] string endpoint, [DisallowNull] string message)
        {
            var traceMessage = new TraceMessage(category, endpoint, message);
            Trace(traceMessage);
        }

        public void Flush() 
        {
            while(_messageQueue.TryDequeue(out var message)) 
            {
                TraceReceived?.Invoke(message.Endpoint, message);
            }
        }

        private readonly object _cacheLock = new();
        private void UpdateEndpointsCache()
        {
            if (_endpointsCache is null)
            {
                lock (_cacheLock)
                {
                    _endpointsCache ??= Endpoints.ToLookup(e => e.Category, e => e);
                    return;
                }
            }

            lock (_endpointsCache)
            {
                _endpointsCache = Endpoints.ToLookup(e => e.Category, e => e);
            }
        }
    }
}

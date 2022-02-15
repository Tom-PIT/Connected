using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Collections;
using TomPIT.Middleware;

namespace TomPIT.Diagnostics.Tracing
{
    public class TraceService : ITraceService
    {
        private ConcurrentBag<ITraceEndpoint> _endpoints;

        private ILookup<string, ITraceEndpoint> _endpointsCache;

        private readonly ConcurrentQueue<ITraceMessage> _messageQueue = new ();

        private readonly long _maxQueueSize = 50000;

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

        public IEnumerable<string> Categories => _endpointsCache?.Select(e => e.Key).ToImmutableList(true) ?? new List<string>().ToImmutableList(false);

        public IEnumerable<ITraceEndpoint> Endpoints => (_endpoints ??= new ConcurrentBag<ITraceEndpoint>()).ToImmutableList(true);

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
                return EndpointsCache[category.ToLower()].ToImmutableList(false);
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
                    if(_endpointsCache is null)
                        _endpointsCache = Endpoints.ToLookup(e => e.Category, e => e);
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

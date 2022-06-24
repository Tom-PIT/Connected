using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Diagnostics.Tracing
{
    public class TraceEndpointComparer : IEqualityComparer<ITraceEndpoint>
    {
        private static readonly Lazy<TraceEndpointComparer> _instance = new();

        public static TraceEndpointComparer Instance => _instance.Value;

        public bool Equals(ITraceEndpoint x, ITraceEndpoint y)
        {
            return x is ITraceEndpoint first && y is ITraceEndpoint second && string.Compare(first.Identifier, second.Identifier, true) == 0; 
        }

        public int GetHashCode([DisallowNull] ITraceEndpoint obj)
        {
            return obj.Identifier.GetHashCode();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Diagnostics.Tracing
{
    public interface ITraceEndpoint
    {
        string Endpoint { get; }
        string Category { get; }

        public string Identifier => $"{Category}/{Endpoint}";
    }
}

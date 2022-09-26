using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Diagnostics.Tracing
{
    public interface ITraceMessage
    {
        string Content { get; }
        ITraceEndpoint Endpoint { get; }
    }
}

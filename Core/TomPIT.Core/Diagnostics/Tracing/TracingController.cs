using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Diagnostics.Tracing
{

    [Authorize(AuthenticationSchemes = "TomPIT")]
    public class TracingController : Controller
    {
        private readonly ITraceService _traceService;
        public TracingController(ITraceService traceService)
        {
            _traceService = traceService;
        }
                
        public IEnumerable<ITraceEndpoint> Endpoints(IEnumerable<string> categories = null)
        {
            var endpoints = categories?.Any() ?? false ? _traceService?.GetEndpoints(categories) : _traceService?.Endpoints;

            return endpoints ?? new List<ITraceEndpoint>();
        }
    }
}

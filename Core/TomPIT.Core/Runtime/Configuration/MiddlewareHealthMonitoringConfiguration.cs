using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Runtime.Configuration
{
    public class MiddlewareHealthMonitoringConfiguration
    {
        public string Type { get; set; }
#if DEBUG
        public string EndpointUrl { get; set; } = "http://sys-connected.tompitdev.net/rest/portal.healthmonitoring";
#else
        public string EndpointUrl { get; set; } = "https://sys-connected.tompit.com/rest/portal.healthmonitoring";
#endif
        public string SubscriptionKey { get; set; }

        public string EndpointKey { get; set; }

        public string RestToken { get; set; }
    }
}

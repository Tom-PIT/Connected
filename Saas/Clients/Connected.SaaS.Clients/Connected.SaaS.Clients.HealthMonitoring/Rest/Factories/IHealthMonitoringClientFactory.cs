using Connected.SaaS.Clients.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients.HealthMonitoring.Rest
{
    public interface IHealthMonitoringClientFactory
    {
        public HealthMonitoringClient Select(string url, string subscriptionKey, IRestAuthenticationProvider authenticationProvider);
    }
}

using Connected.SaaS.Clients.Authentication;
using Connected.SaaS.Clients.HealthMonitoring.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.HealthMonitoring
{
    public class HealthMonitoringRestClientFactory : IHealthMonitoringRestClientFactory
    {
        public HealthMonitoringRestClient Select(string url, string subscriptionKey, IRestAuthenticationProvider authenticationProvider)
        {
            return MemoryCache.Default.Get<HealthMonitoringRestClient>("healthMonitoringRestClient", (e) =>
            {
                return string.Compare(e.EndpointUrl, url, true) == 0 && string.Compare(e.SubscriptionKey, subscriptionKey, true) == 0 && e.AuthenticationProvider.Equals(authenticationProvider);
            },
            (opts) =>
            {
                opts.Key = (url + subscriptionKey + authenticationProvider.GetHashCode()).GetHashCode().ToString();
                opts.Duration = TimeSpan.Zero;
                return new HealthMonitoringRestClient(url, subscriptionKey, authenticationProvider);
            });
        }
    }
}

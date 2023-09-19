using Connected.SaaS.Clients.Authentication;
using Connected.SaaS.Clients.Rest;

namespace Connected.SaaS.Clients.HealthMonitoring.Rest
{
    public class HealthMonitoringClient : RestClientBase
    {
        public string SubscriptionKey { get; }

        public HealthMonitoringRequests Requests { get; }

        public HealthMonitoringClient(string endpointUrl, string subscriptionKey, IRestAuthenticationProvider restAuthenticationProvider) : base(endpointUrl, restAuthenticationProvider)
        {
            SubscriptionKey = subscriptionKey;
            Requests = new HealthMonitoringRequests(subscriptionKey, this);
        }
    }
}

using Connected.SaaS.Clients.Authentication;
using Connected.SaaS.Clients.HealthMonitoring;
using Connected.SaaS.Clients.HealthMonitoring.Rest;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TomPIT.Middleware;

using TomPIT.Runtime.Configuration;

namespace TomPIT.HealthMonitoring
{
	public class HealthMonitoringService : IHealthMonitoringService
	{
		public bool TryLog(string message, TraceLevel level, MiddlewareHealthMonitoringConfiguration config)
		{
			if (config is null)
				return false;

			try
			{
				var authProvider = new BearerAuthenticationProvider(config.RestToken);
			
				var client = MiddlewareDescriptor.Current.Tenant.GetService<IHealthMonitoringClientFactory>().Select(config.EndpointUrl, config.SubscriptionKey, authProvider);

				if (client is null)
					return false;

				AsyncUtils.RunSync(() => client.Requests.LogManagement.InsertLog(new Endpoint { Key = config.EndpointKey }, message, level, default));

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}

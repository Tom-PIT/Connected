using System.Diagnostics;

namespace TomPIT.ServiceProviders.HealthMonitoring
{
	public interface ILogging
	{
		void Insert(string subscription, string endpoint, string message, TraceLevel level);
	}
}

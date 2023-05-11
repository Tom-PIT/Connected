using TomPIT.Environment;

namespace TomPIT.Runtime.Configuration
{
	public interface ISys
	{
		PluginSet Plugins { get; }

		HealthMonitoringConfiguration HealthMonitoring { get; set; }

		InstanceFeatures Features { get; }
	}
}

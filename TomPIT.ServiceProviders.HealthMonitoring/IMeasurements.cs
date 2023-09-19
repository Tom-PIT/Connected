using System.Threading.Tasks;

namespace TomPIT.ServiceProviders.HealthMonitoring
{
	public interface IMeasurements
	{
		Task Insert(string subscription, string endpoint, int quality);
	}
}

namespace TomPIT.ServiceProviders.HealthMonitoring
{
	public interface IManagement
	{
		void EnableEndpoint(string subscription, string endpoint);
		void DisableEndpoint(string subscription, string endpoint);
	}
}

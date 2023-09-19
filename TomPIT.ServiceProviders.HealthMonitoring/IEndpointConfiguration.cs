namespace TomPIT.ServiceProviders.HealthMonitoring
{
	public interface IEndpointConfiguration
	{
		string Subscription { get; }
		string Name { get; }
	}
}

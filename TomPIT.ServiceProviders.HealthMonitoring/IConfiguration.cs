using System.Collections.Immutable;

namespace TomPIT.ServiceProviders.HealthMonitoring
{
	public interface IConfiguration
	{
		ImmutableList<string> Subscriptions { get; }
		ImmutableList<IEndpointConfiguration> Endpoints { get; }
	}
}

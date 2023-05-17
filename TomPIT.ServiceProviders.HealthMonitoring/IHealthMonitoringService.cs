namespace TomPIT.ServiceProviders.HealthMonitoring
{
	public interface IHealthMonitoringService
	{
		IMeasurements Measurements { get; }
		ILogging Logging { get; }
		IManagement Management { get; }
		IConfiguration Configuration { get; }
	}
}

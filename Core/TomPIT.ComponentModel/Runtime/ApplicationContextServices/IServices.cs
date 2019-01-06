namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface IServices
	{
		IDataService Data { get; }
		ITimezoneService Timezone { get; }
		IIdentityService Identity { get; }
		ILocalizationService Localization { get; }
		IRoutingService Routing { get; }
		ILoggingService Log { get; }
		ICachingService Cache { get; }
		IValidationService Validation { get; }
		IEnvironmentService Environment { get; }
	}
}

namespace TomPIT.Services.Context
{
	public interface IContextServices
	{
		IContextDataService Data { get; }
		IContextTimezoneService Timezone { get; }
		IContextIdentityService Identity { get; }
		IContextLocalizationService Localization { get; }
		IContextRoutingService Routing { get; }
		IContextDiagnosticService Diagnostic { get; }
		IContextCachingService Cache { get; }
		IContextValidationService Validation { get; }
		IContextEnvironmentService Environment { get; }
		IContextStorageService Storage { get; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services.Context;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareServices
	{
		IMiddlewareService Middleware { get; }
		IMiddlewareAuthorizationService Authorization { get; }
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
		IContextCdnService Cdn { get; }
		IContextIoTService IoT { get; }
		IContextMediaService Media { get; }
		IContextFeatureService Features { get; }
		IContextSearchService Search { get; }
		IContextBigDataService BigData { get; }

	}
}

﻿namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareServices
	{
		IMiddlewareIoCService IoC { get; }
		IMiddlewareAuthorizationService Authorization { get; }
		IMiddlewareDataService Data { get; }
		IMiddlewareGlobalizationService Globalization { get; }
		IMiddlewareIdentityService Identity { get; }
		IMiddlewareRoutingService Routing { get; }
		IMiddlewareDiagnosticService Diagnostic { get; }
		IMiddlewareCachingService Cache { get; }
		IMiddlewareValidationService Validation { get; }
		IMiddlewareStorageService Storage { get; }
		IMiddlewareCdnService Cdn { get; }
		IMiddlewareIoTService IoT { get; }
		IMiddlewareMediaService Media { get; }
		IMiddlewareSearchService Search { get; }
		IMiddlewareBigDataService BigData { get; }
	}
}
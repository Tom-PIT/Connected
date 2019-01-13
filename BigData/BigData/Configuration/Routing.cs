using Microsoft.AspNetCore.Routing;

namespace TomPIT.BigData.Configuration
{
	internal static class Routing
	{
		public static void Register(IRouteBuilder builder)
		{
			//			builder.MapRoute(
			//	 name: "DefaultApi",
			//	 routeTemplate: "api/{controller}/{action}",
			//	 defaults: new { action = RouteParameter.Optional }
			//);

			//			builder.MapRoute(
			//				name: "datahub",
			//				routeTemplate: "data/{endpoint}",
			//				defaults: null,
			//				constraints: null,
			//				handler: HttpClientFactory.CreatePipeline(
			//													new HttpControllerDispatcher(config),
			//													new DelegatingHandler[] { new DataHubEndpointHandler(config) })
			//		);

			//			builder.MapRoute(
			//					name: "datahubpartition",
			//					routeTemplate: "task/{partition}/{task}",
			//					defaults: null,
			//					constraints: null,
			//					handler: HttpClientFactory.CreatePipeline(
			//														new HttpControllerDispatcher(config),
			//														new DelegatingHandler[] { new PartitionHandler(config) })
			//			);



		}
	}
}

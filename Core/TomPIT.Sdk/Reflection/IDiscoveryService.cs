using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Middleware;

namespace TomPIT.Reflection
{
	public interface IDiscoveryService
	{
		IMicroServiceDiscovery MicroServices { get; }
		IConfigurationDiscovery Configuration { get; }
		//IManifestDiscovery Manifests { get; }

		[Obsolete("Please use MicroServices.References instead.")]
		IServiceReferencesConfiguration References(string microService);
		[Obsolete("Please use MicroServices.References instead.")]
		IServiceReferencesConfiguration References(Guid microService);
		[Obsolete("Please use MicroServices.References instead.")]
		List<IMicroService> FlattenReferences(Guid microService);
		[Obsolete("Please use MicroServices.Info instead.")]
		IMicroServiceInfoMiddleware MicroServiceInfo(IMicroServiceContext context, Guid microService);
		[Obsolete("Please use Configuration.Find instead.")]
		IElement Find(Guid component, Guid id);
		[Obsolete("Please use Configuration.Find instead.")]
		IElement Find(IConfiguration configuration, Guid id);
		[Obsolete("Please use Configuration.Query instead.")]
		List<T> Children<T>(IConfiguration configuration) where T : IElement;

		[Obsolete("Please use Configuration.QueryDependencies instead.")]
		List<Guid> Dependencies(IConfiguration configuration);
		[Obsolete("Please use ReflectionExtensions instead.")]
		PropertyInfo[] Properties(object instance, bool writableOnly, bool filterByEnvironment);
	}
}

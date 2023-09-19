using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Reflection
{
	internal class DiscoveryService : TenantObject, IDiscoveryService
	{
		private IConfigurationDiscovery _configuration;
		private IMicroServiceDiscovery _microServices;

		public DiscoveryService(ITenant tenant) : base(tenant)
		{

		}

		public IConfigurationDiscovery Configuration => _configuration ??= new ConfigurationDiscovery(Tenant);
		public IMicroServiceDiscovery MicroServices => _microServices ??= new MicroServiceDiscovery(Tenant);


		public IElement Find(IConfiguration configuration, Guid id)
		{
			return Configuration.Find(configuration, id);
		}

		public IElement Find(Guid component, Guid id)
		{
			return Configuration.Find(component, id);
		}

		public IServiceReferencesConfiguration References(Guid microService)
		{
			return MicroServices.References.Select(microService);
		}
		public IServiceReferencesConfiguration References(string microService)
		{
			return MicroServices.References.Select(microService);
		}

		public List<IMicroService> FlattenReferences(Guid microService)
		{
			return MicroServices.References.Flatten(microService).ToList();
		}

		public List<T> Children<T>(IConfiguration configuration) where T : IElement
		{
			return Configuration.Query<T>(configuration).ToList();
		}


		public List<Guid> Dependencies(IConfiguration configuration)
		{
			return Configuration.QueryDependencies(configuration).ToList();
		}

		public IMicroServiceInfoMiddleware MicroServiceInfo(IMicroServiceContext context, Guid microService)
		{
			return MicroServices.Info.SelectMiddleware(context, microService);
		}

		[Obsolete("Use ReflectionExtensions instead.")]
		public PropertyInfo[] Properties(object instance, bool writableOnly, bool filterByEnvironment)
		{
			return ReflectionExtensions.Properties(instance, writableOnly);
		}
	}
}

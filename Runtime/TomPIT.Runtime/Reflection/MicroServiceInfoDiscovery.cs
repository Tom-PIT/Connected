using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Reflection
{
	internal class MicroServiceInfoDiscovery : TenantObject, IMicroServiceInfoDiscovery
	{
		public MicroServiceInfoDiscovery(ITenant tenant) : base(tenant)
		{
		}

		public IMicroServiceInfoMiddleware SelectMiddleware(IMicroServiceContext context, Guid microService)
		{
			var components = Tenant.GetService<IComponentService>().QueryComponents(microService, ComponentCategories.MicroServiceInfo);

			if (components.Count == 0)
				return null;

			if (Tenant.GetService<IComponentService>().SelectConfiguration(components[0].Token) is not IMicroServiceInfoConfiguration config)
				return null;

			var type = config.Middleware(context);

			if (type == null)
				return null;

			return context.CreateMiddleware<IMicroServiceInfoMiddleware>(type);

		}
	}
}

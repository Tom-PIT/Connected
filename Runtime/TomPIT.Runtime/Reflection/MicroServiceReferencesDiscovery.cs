using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	internal class MicroServiceReferencesDiscovery : TenantObject, IMicroServiceReferencesDiscovery
	{
		public MicroServiceReferencesDiscovery(ITenant tenant) : base(tenant)
		{
		}

		public List<IMicroService> Flatten(Guid microService)
		{
			var r = new List<IMicroService>();

			Flatten(microService, r);

			return r;
		}

		public IServiceReferencesConfiguration Select(string microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(ms.Token, ComponentCategories.Reference, "References");

			if (component == null)
				return null;

			return Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IServiceReferencesConfiguration;
		}

		public IServiceReferencesConfiguration Select(Guid microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			return Select(ms.Name);
		}

		private void Flatten(Guid microService, List<IMicroService> existing)
		{
			var refs = Select(microService);

			if (refs == null)
				return;

			foreach (var reference in refs.MicroServices)
			{
				if (string.IsNullOrWhiteSpace(reference.MicroService))
					continue;

				if (existing.FirstOrDefault(f => string.Compare(f.Name, reference.MicroService, true) == 0) != null)
					continue;

				var ms = Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

				if (ms != null)
				{
					existing.Add(ms);
					Flatten(ms.Token, existing);
				}
			}
		}
	}
}

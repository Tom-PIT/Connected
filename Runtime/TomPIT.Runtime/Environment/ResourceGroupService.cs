using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Environment
{
	internal class ResourceGroupService : ClientRepository<IResourceGroup, Guid>, IResourceGroupService
	{
		public ResourceGroupService(ITenant tenant) : base(tenant, "resourceGroup")
		{

		}

		public IResourceGroup Default => Select(new Guid("E14372D117CD48D6BC29D57C397AF87C"));

		public List<IResourceGroup> Query()
		{
			var u = Tenant.CreateUrl("ResourceGroup", "Query");

			return Tenant.Get<List<ResourceGroup>>(u).ToList<IResourceGroup>();
		}

		public IResourceGroup Select(string name)
		{
			var r = Get(f => string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
			{
				if (Shell.GetConfiguration<IClientSys>().ResourceGroups.FirstOrDefault(f => string.Compare(f, name, true) == 0) == null)
					return null;
			}

			var u = Tenant.CreateUrl("ResourceGroup", "SelectByName")
			.AddParameter("resourceGroup", name);

			r = Tenant.Get<ResourceGroup>(u);

			if (r != null)
				Set(r.Token, r, TimeSpan.Zero);

			return r;
		}

		public IResourceGroup Select(Guid resourceGroup)
		{
			return Get(resourceGroup,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var u = Tenant.CreateUrl("ResourceGroup", "Select")
					.AddParameter("resourceGroup", resourceGroup);

					var r = Tenant.Get<ResourceGroup>(u);

					if (r != null)
					{
						if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
						{
							if (Shell.GetConfiguration<IClientSys>().ResourceGroups.FirstOrDefault(g => string.Compare(g, r.Name, true) == 0) == null)
								return null;
						}
					}

					return r;
				});
		}
	}
}

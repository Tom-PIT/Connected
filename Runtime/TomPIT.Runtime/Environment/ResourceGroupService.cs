using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Environment
{
	internal class ResourceGroupService : ClientRepository<IResourceGroup, Guid>, IResourceGroupService
	{
		public ResourceGroupService(ISysConnection connection) : base(connection, "resourceGroup")
		{

		}

		public List<IResourceGroup> Query()
		{
			var u = Connection.CreateUrl("ResourceGroup", "Query");

			return Connection.Get<List<ResourceGroup>>(u).ToList<IResourceGroup>();
		}

		public IResourceGroup Select(string name)
		{
			var r = Get(f => string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
			{
				if (Instance.ResourceGroups.FirstOrDefault(f => string.Compare(f, name, true) == 0) == null)
					return null;
			}

			var u = Connection.CreateUrl("ResourceGroup", "SelectByName")
			.AddParameter("resourceGroup", name);

			r = Connection.Get<ResourceGroup>(u);

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

					var u = Connection.CreateUrl("ResourceGroup", "Select")
					.AddParameter("resourceGroup", resourceGroup);

					var r = Connection.Get<ResourceGroup>(u);

					if (r != null)
					{
						if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
						{
							if (Instance.ResourceGroups.FirstOrDefault(g => string.Compare(g, r.Name, true) == 0) == null)
								return null;
						}
					}

					return r;
				});
		}
	}
}

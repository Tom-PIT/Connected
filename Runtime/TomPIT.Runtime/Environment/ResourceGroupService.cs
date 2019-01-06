using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

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

		public IResourceGroup Select(Guid resourceGroup)
		{
			return Get(resourceGroup,
				(f) =>
				{
					var u = Connection.CreateUrl("ResourceGroup", "Select")
					.AddParameter("resourceGroup", resourceGroup);

					return Connection.Get<ResourceGroup>(u);
				});
		}
	}
}

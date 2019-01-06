using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Net;

namespace TomPIT.Environment
{
	internal class ResourceGroupService : ContextCacheRepository<IResourceGroup, Guid>, IResourceGroupService
	{
		public ResourceGroupService(ISysContext server) : base(server, "resourceGroup")
		{

		}

		public List<IResourceGroup> Query()
		{
			var u = Server.CreateUrl("ResourceGroup", "Query");

			return Server.Connection.Get<List<ResourceGroup>>(u).ToList<IResourceGroup>();
		}

		public IResourceGroup Select(Guid resourceGroup)
		{
			return Get(resourceGroup,
				(f) =>
				{
					var u = Server.CreateUrl("ResourceGroup", "Select")
					.AddParameter("resourceGroup", resourceGroup);

					return Server.Connection.Get<ResourceGroup>(u);
				});
		}
	}
}

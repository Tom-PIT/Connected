using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Environment;

namespace TomPIT.Proxy.Remote
{
	internal class ResourceGroupController : IResourceGroupController
	{
		private const string Controller = "ResourceGroup";
		public ImmutableList<IResourceGroup> Query()
		{
			return Connection.Get<List<ResourceGroup>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IResourceGroup>();
		}

		public IResourceGroup Select(Guid resourceGroup)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("resourceGroup", resourceGroup);

			return Connection.Get<ResourceGroup>(u);
		}

		public IResourceGroup Select(string name)
		{
			var u = Connection.CreateUrl(Controller, "SelectByName")
				.AddParameter("resourceGroup", name);

			return Connection.Get<ResourceGroup>(u);
		}
	}
}

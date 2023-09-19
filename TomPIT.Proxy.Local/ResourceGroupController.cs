using System;
using System.Collections.Immutable;
using TomPIT.Environment;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class ResourceGroupController : IResourceGroupController
	{
		public ImmutableList<IResourceGroup> Query()
		{
			return DataModel.ResourceGroups.Query().ToImmutableList<IResourceGroup>();
		}

		public IResourceGroup Select(Guid resourceGroup)
		{
			return DataModel.ResourceGroups.Select(resourceGroup);
		}

		public IResourceGroup Select(string name)
		{
			return DataModel.ResourceGroups.Select(name);
		}
	}
}

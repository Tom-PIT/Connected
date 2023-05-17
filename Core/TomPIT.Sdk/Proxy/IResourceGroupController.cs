using System;
using System.Collections.Immutable;
using TomPIT.Environment;

namespace TomPIT.Proxy
{
	public interface IResourceGroupController
	{
		ImmutableList<IResourceGroup> Query();
		IResourceGroup Select(Guid resourceGroup);
		IResourceGroup Select(string name);
	}
}

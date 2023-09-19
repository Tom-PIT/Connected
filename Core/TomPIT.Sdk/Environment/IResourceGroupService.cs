using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TomPIT.Environment
{
	public interface IResourceGroupService
	{
		List<IResourceGroup> Query();
		ImmutableList<IResourceGroup> QuerySupported();
		IResourceGroup Select(Guid resourceGroup);
		IResourceGroup Select(string name);

		IResourceGroup Default { get; }
	}
}

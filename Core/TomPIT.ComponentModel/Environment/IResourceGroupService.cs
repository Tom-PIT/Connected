using System;
using System.Collections.Generic;

namespace TomPIT.Environment
{
	public interface IResourceGroupService
	{
		List<IResourceGroup> Query();
		IResourceGroup Select(Guid resourceGroup);
	}
}

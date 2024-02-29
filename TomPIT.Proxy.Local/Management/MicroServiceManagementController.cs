using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management
{
	internal class MicroServiceManagementController : IMicroServiceManagementController
	{
		public void Delete(Guid token)
		{
			DataModel.MicroServices.Delete(token);
		}

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, string version, string commit)
		{
			DataModel.MicroServices.Insert(token, name, resourceGroup, template, version, commit);
		}

		public ImmutableList<IMicroService> Query(Guid resourceGroup)
		{
			return DataModel.MicroServices.Query(resourceGroup).ToImmutableList();
		}

		public void Update(Guid token, string name, Guid resourceGroup, Guid template, string version, string commit)
		{
			DataModel.MicroServices.Update(token, name, template, resourceGroup, version, commit);
		}
	}
}

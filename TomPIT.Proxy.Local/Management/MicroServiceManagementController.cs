using System;
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

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, string meta)
		{
			DataModel.MicroServices.Insert(token, name, status, resourceGroup, template, meta, null);
		}

		public void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus, Guid package, Guid plan)
		{
			DataModel.MicroServices.Update(token, name, status, template, resourceGroup, package, plan, updateStatus, commitStatus);
		}
	}
}

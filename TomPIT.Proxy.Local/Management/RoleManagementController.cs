using System;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management
{
	internal class RoleManagementController : IRoleManagementController
	{
		public void Delete(Guid token)
		{
			DataModel.Roles.Delete(token);
		}

		public Guid Insert(string name)
		{
			return DataModel.Roles.Insert(name);
		}

		public void Update(Guid token, string name)
		{
			DataModel.Roles.Update(token, name);
		}
	}
}

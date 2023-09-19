using System;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management
{
	internal class RoleManagementController : IRoleManagementController
	{
		private const string Controller = "RoleManagement";
		public void Delete(Guid token)
		{
			Connection.Post<Guid>(Connection.CreateUrl(Controller, "Delete"), new
			{
				token
			});
		}

		public Guid Insert(string name)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Insert"), new
			{
				name
			});
		}

		public void Update(Guid token, string name)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Update"), new
			{
				name,
				token
			});
		}
	}
}

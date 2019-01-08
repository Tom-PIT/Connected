using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class RoleManagementService : IRoleManagementService
	{
		public RoleManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid token)
		{
			var u = Connection.CreateUrl("RoleManagement", "Delete");
			var e = new JObject
			{
				{"token", token}
			};

			Connection.Post<Guid>(u, e);

			if (Connection.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}

		public Guid Insert(string name)
		{
			var u = Connection.CreateUrl("RoleManagement", "Insert");
			var e = new JObject
			{
				{"name", name}
			};

			var id = Connection.Post<Guid>(u, e);

			if (Connection.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(id));

			return id;
		}

		public void Update(Guid token, string name)
		{
			var u = Connection.CreateUrl("RoleManagement", "Update");
			var e = new JObject
			{
				{"name", name},
				{"token", token}
			};

			Connection.Post(u, e);

			if (Connection.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}
	}
}

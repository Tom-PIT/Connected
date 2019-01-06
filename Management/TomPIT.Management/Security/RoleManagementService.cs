using System;
using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.Security
{
	internal class RoleManagementService : IRoleManagementService
	{
		public RoleManagementService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public void Delete(Guid token)
		{
			var u = Server.CreateUrl("RoleManagement", "Delete");
			var e = new JObject
			{
				{"token", token}
			};

			Server.Connection.Post<Guid>(u, e);

			if (Server.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}

		public Guid Insert(string name)
		{
			var u = Server.CreateUrl("RoleManagement", "Insert");
			var e = new JObject
			{
				{"name", name}
			};

			var id = Server.Connection.Post<Guid>(u, e);

			if (Server.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(id));

			return id;
		}

		public void Update(Guid token, string name)
		{
			var u = Server.CreateUrl("RoleManagement", "Update");
			var e = new JObject
			{
				{"name", name},
				{"token", token}
			};

			Server.Connection.Post(u, e);

			if (Server.GetService<IRoleService>() is IRoleNotification n)
				n.NotifyChanged(this, new RoleEventArgs(token));
		}
	}
}

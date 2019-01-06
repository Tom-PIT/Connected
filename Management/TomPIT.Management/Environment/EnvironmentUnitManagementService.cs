using Newtonsoft.Json.Linq;
using System;
using TomPIT.Net;

namespace TomPIT.Environment
{
	internal class EnvironmentUnitManagementService : IEnvironmentUnitManagementService
	{
		public EnvironmentUnitManagementService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public void Delete(Guid unit)
		{
			var u = Server.CreateUrl("EnvironmentUnitManagement", "Delete");
			var e = new JObject
			{
				{"token", unit }
			};

			Server.Connection.Post(u, e);

			if (Server.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyRemoved(this, new EnvironmentUnitEventArgs(unit));
		}

		public Guid Insert(string name, Guid parent, int ordinal)
		{
			var u = Server.CreateUrl("EnvironmentUnitManagement", "Insert");
			var e = new JObject
			{
				{"name", name },
				{"parent", parent },
				{"ordinal", ordinal }
			};

			var id = Server.Connection.Post<Guid>(u, e);

			if (Server.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyChanged(this, new EnvironmentUnitEventArgs(id));

			return id;
		}

		public void Update(Guid unit, string name, Guid parent, int ordinal)
		{
			var u = Server.CreateUrl("EnvironmentUnitManagement", "Update");
			var e = new JObject
			{
				{"name", name },
				{"parent", parent },
				{"ordinal", ordinal }
			};

			Server.Connection.Post(u, e);

			if (Server.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyChanged(this, new EnvironmentUnitEventArgs(unit));
		}
	}
}

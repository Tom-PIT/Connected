using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;

namespace TomPIT.Environment
{
	internal class EnvironmentUnitManagementService : IEnvironmentUnitManagementService
	{
		public EnvironmentUnitManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid unit)
		{
			var u = Connection.CreateUrl("EnvironmentUnitManagement", "Delete");
			var e = new JObject
			{
				{"token", unit }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyRemoved(this, new EnvironmentUnitEventArgs(unit));
		}

		public Guid Insert(string name, Guid parent, int ordinal)
		{
			var u = Connection.CreateUrl("EnvironmentUnitManagement", "Insert");
			var e = new JObject
			{
				{"name", name },
				{"parent", parent },
				{"ordinal", ordinal }
			};

			var id = Connection.Post<Guid>(u, e);

			if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyChanged(this, new EnvironmentUnitEventArgs(id));

			return id;
		}

		public void Update(Guid unit, string name, Guid parent, int ordinal)
		{
			var u = Connection.CreateUrl("EnvironmentUnitManagement", "Update");
			var e = new JObject
			{
				{"name", name },
				{"parent", parent },
				{"ordinal", ordinal }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyChanged(this, new EnvironmentUnitEventArgs(unit));
		}
	}
}

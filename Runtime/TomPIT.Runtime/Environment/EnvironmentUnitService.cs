using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.Environment
{
	internal class EnvironmentUnitService : ClientRepository<IEnvironmentUnit, Guid>, IEnvironmentUnitService, IEnvironmentUnitNotification
	{
		public EnvironmentUnitService(ISysConnection connection) : base(connection, "environmentUnit")
		{

		}

		public List<IEnvironmentUnit> Query()
		{
			var u = Connection.CreateUrl("EnvironmentUnit", "Query");

			return Connection.Get<List<EnvironmentUnit>>(u).ToList<IEnvironmentUnit>();
		}

		public List<IEnvironmentUnit> Query(Guid parent)
		{
			var u = Connection.CreateUrl("EnvironmentUnit", "QueryChildren")
				.AddParameter("parent", parent);

			return Connection.Get<List<EnvironmentUnit>>(u).ToList<IEnvironmentUnit>();
		}

		public IEnvironmentUnit Select(Guid environmentUnit)
		{
			return Get(environmentUnit,
				(f) =>
				{
					var u = Connection.CreateUrl("EnvironmentUnit", "Select")
						.AddParameter("environmentUnit", environmentUnit);

					return Connection.Get<EnvironmentUnit>(u);

				});
		}

		public void NotifyChanged(object sender, EnvironmentUnitEventArgs e)
		{
			Remove(e.EnvironmentUnit);
		}

		public void NotifyRemoved(object sender, EnvironmentUnitEventArgs e)
		{
			Remove(e.EnvironmentUnit);
		}
	}
}

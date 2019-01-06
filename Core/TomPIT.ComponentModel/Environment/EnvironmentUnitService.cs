using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Net;

namespace TomPIT.Environment
{
	internal class EnvironmentUnitService : ContextCacheRepository<IEnvironmentUnit, Guid>, IEnvironmentUnitService, IEnvironmentUnitNotification
	{
		public EnvironmentUnitService(ISysContext server) : base(server, "environmentUnit")
		{

		}

		public List<IEnvironmentUnit> Query()
		{
			var u = Server.CreateUrl("EnvironmentUnit", "Query");

			return Server.Connection.Get<List<EnvironmentUnit>>(u).ToList<IEnvironmentUnit>();
		}

		public List<IEnvironmentUnit> Query(Guid parent)
		{
			var u = Server.CreateUrl("EnvironmentUnit", "QueryChildren")
				.AddParameter("parent", parent);

			return Server.Connection.Get<List<EnvironmentUnit>>(u).ToList<IEnvironmentUnit>();
		}

		public IEnvironmentUnit Select(Guid environmentUnit)
		{
			return Get(environmentUnit,
				(f) =>
				{
					var u = Server.CreateUrl("EnvironmentUnit", "Select")
						.AddParameter("environmentUnit", environmentUnit);

					return Server.Connection.Get<EnvironmentUnit>(u);

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

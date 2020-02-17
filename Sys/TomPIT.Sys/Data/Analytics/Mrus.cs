using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Analytics;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data.Analytics
{
	internal class Mrus
	{
		public void Modify(int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, int capacity)
		{
			var total = Shell.GetService<IDatabaseService>().Proxy.Analytics.Mru.Modify(Guid.NewGuid(), type, primaryKey, entity, entityPrimaryKey, tags, DateTime.UtcNow);

			if (total > capacity)
			{
				var existing = Shell.GetService<IDatabaseService>().Proxy.Analytics.Mru.Query(entity, entityPrimaryKey, tags).OrderByDescending(f => f.Date);
				var obsolete = existing.Skip(capacity);

				foreach (var o in obsolete)
					Shell.GetService<IDatabaseService>().Proxy.Analytics.Mru.Delete(o);
			}
		}

		public List<IMru> Query(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Analytics.Mru.Query(entity, entityPrimaryKey, tags);
		}
	}
}

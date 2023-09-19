using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Analytics;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class AnalyticsController : IAnalyticsController
	{
		public void ModifyMru(int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, int capacity)
		{
			DataModel.Mrus.Modify(type, primaryKey, entity, entityPrimaryKey, tags, capacity);
		}

		public ImmutableList<IMru> QueryMru(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags)
		{
			return DataModel.Mrus.Query(entity, entityPrimaryKey, tags).ToImmutableList();
		}

		public void Flush()
		{

		}
	}
}

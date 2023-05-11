using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Analytics;

namespace TomPIT.Proxy
{
	public interface IAnalyticsController
	{
		void ModifyMru(int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, int capacity);
		ImmutableList<IMru> QueryMru(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags);
		void Flush();
	}
}

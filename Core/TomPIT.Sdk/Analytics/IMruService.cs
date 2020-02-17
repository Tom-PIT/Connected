using System.Collections.Generic;

namespace TomPIT.Analytics
{
	public interface IMruService
	{
		void Modify(int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, int capacity);
		List<IMru> Query(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags);
	}
}

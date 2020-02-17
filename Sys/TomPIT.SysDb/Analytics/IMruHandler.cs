using System;
using System.Collections.Generic;
using TomPIT.Analytics;

namespace TomPIT.SysDb.Analytics
{
	public interface IMruHandler
	{
		List<IMru> Query(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags);
		void Delete(IMru item);
		int Modify(Guid token, int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, DateTime date);
	}
}

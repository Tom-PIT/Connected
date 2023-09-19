using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Analytics
{
	internal class MruService : TenantObject, IMruService
	{
		public MruService(ITenant tenant) : base(tenant)
		{

		}

		public void Modify(int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, int capacity)
		{
			Instance.SysProxy.Analytics.ModifyMru(type, primaryKey, entity, entityPrimaryKey, tags, capacity);
		}

		public List<IMru> Query(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags)
		{
			return Instance.SysProxy.Analytics.QueryMru(entity, entityPrimaryKey, tags).ToList();
		}
	}
}

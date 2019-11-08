using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionFieldStatisticsCache : SynchronizedClientRepository<IPartitionFieldStatistics, string>
	{
		public PartitionFieldStatisticsCache(ITenant tenant) : base(tenant, "partitionfieldstats")
		{
		}

		public IPartitionFieldStatistics Select(Guid file, string fieldName)
		{
			return Get(GenerateKey(file, fieldName));
		}

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("BigDataManagement", "QueryFieldStatistics");
			var fields = Tenant.Get<List<PartitionFieldStatistics>>(u);

			foreach (var field in fields)
				Set(GenerateKey(field.File, field.FieldName), field, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var u = Tenant.CreateUrl("BigDataManagement", "SelectFieldStatistic");
			var e = new JObject
			{
				{"file", tokens[0] },
				{"fieldName", tokens[1] }
			};

			var field = Tenant.Post<PartitionFieldStatistics>(u, e);

			if (field != null)
				Set(GenerateKey(field.File, field.FieldName), field, TimeSpan.Zero);
		}

		public void Notify(Guid file, string fieldName, bool remove = false)
		{
			var key = GenerateKey(file, fieldName);

			if (remove)
				Remove(key);
			else
				Refresh(key);
		}
	}
}
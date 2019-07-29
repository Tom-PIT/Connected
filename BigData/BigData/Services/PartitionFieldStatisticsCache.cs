using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.BigData.Services
{
	internal class PartitionFieldStatisticsCache : SynchronizedClientRepository<IPartitionFieldStatistics, string>
	{
		public PartitionFieldStatisticsCache(ISysConnection connection) : base(connection, "partitionfieldstats")
		{
		}

		public IPartitionFieldStatistics Select(Guid file, string fieldName)
		{
			return Get(GenerateKey(file, fieldName));
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryFieldStatistics");
			var fields = Connection.Get<List<PartitionFieldStatistics>>(u);

			foreach (var field in fields)
				Set(GenerateKey(field.File, field.FieldName), field, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var u = Connection.CreateUrl("BigDataManagement", "SelectFieldStatistic");
			var e = new JObject
			{
				{"file", tokens[0] },
				{"fieldName", tokens[1] }
			};

			var field = Connection.Post<PartitionFieldStatistics>(u, e);

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
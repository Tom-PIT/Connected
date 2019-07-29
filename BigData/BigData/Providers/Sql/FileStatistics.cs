using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class FileStatistics : DatabaseRecord
	{
		public DateTime MinTimestamp { get; set; }
		public DateTime MaxTimestamp { get; set; }
		public int Count { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			MinTimestamp = GetDate("min_timestamp");
			MaxTimestamp = GetDate("max_timestamp");
			Count = GetInt("count");
		}
	}
}

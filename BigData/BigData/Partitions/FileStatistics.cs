using Amt.Api.Data;
using Amt.Sys.Model;
using System;

namespace Amt.DataHub.Partitions
{
	internal class FileStatistics : Record
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

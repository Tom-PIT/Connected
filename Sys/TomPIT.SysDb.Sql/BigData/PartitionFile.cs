using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class PartitionFile : LongPrimaryKeyRecord, IPartitionFile
	{
		public DateTime StartTimestamp { get; set; }

		public DateTime EndTimestamp { get; set; }

		public int Count { get; set; }

		public PartitionFileStatus Status { get; set; }

		public Guid Node { get; set; }

		public Guid FileName { get; set; }

		public Guid Partition { get; set; }

		public string Key { get; set; }
		public Guid Timezone { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			StartTimestamp = GetDate("start_timestamp");
			EndTimestamp = GetDate("end_timestamp");
			Count = GetInt("count");
			Status = GetValue("status", PartitionFileStatus.Closed);
			Node = GetGuid("node_token");
			FileName = GetGuid("file");
			Partition = GetGuid("partition_token");
			Key = GetString("key");
			Timezone = GetGuid("timezone_token");
		}
	}
}

using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class PartitionFieldStatistics : LongPrimaryKeyRecord, IPartitionFieldStatistics
	{
		public Guid File { get; set; }
		public string StartString { get; set; }
		public string EndString { get; set; }
		public decimal StartNumber { get; set; }
		public decimal EndNumber { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string FieldName { get; set; }
		public Guid Partition { get; set; }
		public string Key { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			File = GetGuid("file");
			StartString = GetString("start_string");
			EndString = GetString("end_string");
			StartNumber = GetDecimal("start_number");
			EndNumber = GetDecimal("end_number");
			StartDate = GetDate("start_date");
			EndDate = GetDate("end_date");
			FieldName = GetString("field_name");
			Partition = GetGuid("index");
			Key = GetString("key");
		}
	}
}

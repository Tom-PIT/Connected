using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class PartitionFieldStatistics : LongPrimaryKeyRecord, IPartitionFieldStatistics
	{
		public Guid File {get;set;}
		public string StartString {get;set;}
		public string EndString {get;set;}
		public double StartNumber {get;set;}
		public double EndNumber {get;set;}
		public DateTime StartDate {get;set;}
		public DateTime EndDate {get;set;}
		public string FieldName {get;set;}

		protected override void OnCreate()
		{
			base.OnCreate();

			File = GetGuid("file");
			StartString = GetString("start_string");
			EndString = GetString("end_string");
			StartNumber = GetDouble("start_number");
			EndNumber = GetDouble("end_number");
			StartDate = GetDate("start_date");
			EndDate = GetDate("end_date");
			FieldName = GetString("field_name");
		}
	}
}

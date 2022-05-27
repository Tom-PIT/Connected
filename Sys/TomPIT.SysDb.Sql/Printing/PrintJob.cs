using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Printing
{
	internal class PrintJob : LongPrimaryKeyRecord, IPrintJob
	{
		public Guid Token { get; set; }

		public PrintJobStatus Status { get; set; }

		public string Error { get; set; }

		public DateTime Created { get; set; }

		public string Provider { get; set; }
		public string Arguments { get; set; }
		public Guid Component { get; set; }
		public long SerialNumber { get; set; }
		public string Category { get; set; }

		public string User {get;set;}
		public int CopyCount { get; set; }
		protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetGuid("token");
			Status = GetValue("status", PrintJobStatus.Pending);
			Error = GetString("error");
			Created = GetDate("created");
			Provider = GetString("provider");
			Arguments = GetString("arguments");
			Component = GetGuid("component");
			User = GetString("user");
			SerialNumber = GetLong("serial_number");
			Category = GetString("category");
			CopyCount = GetInt("copy_count");
		}
	}
}

using System;
using TomPIT.Data.Sql;
using TomPIT.QA;

namespace TomPIT.SysDb.Sql.Development
{
	internal abstract class TestSessionEntity : LongPrimaryKeyRecord, ITestSessionEntity
	{
		public DateTime Start { get; set; }
		public DateTime Complete { get; set; }
		public TestRunStatus Status { get; set; }
		public TestRunResult Result { get; set; }
		public string Error { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Start = GetDate("start");
			Complete = GetDate("complete");
			Status = GetValue("status", TestRunStatus.Pending);
			Result = GetValue("result", TestRunResult.Success);
			Error = GetString("error");
		}
	}
}

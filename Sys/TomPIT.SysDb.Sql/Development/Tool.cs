using System;
using TomPIT.Analysis;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Tool : PrimaryKeyRecord, ITool
	{
		public string Name { get; set; }

		public ToolStatus Status { get; set; }

		public DateTime LastRun { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("tool");
			Status = GetValue("status", ToolStatus.Idle);
			LastRun = GetDate("last_run");
		}
	}
}

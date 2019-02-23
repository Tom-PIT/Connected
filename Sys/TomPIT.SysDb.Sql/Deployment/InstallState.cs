using System;
using TomPIT.Data.Sql;
using TomPIT.Deployment;

namespace TomPIT.SysDb.Sql.Deployment
{
	internal class InstallState : PrimaryKeyRecord, IInstallState
	{
		public Guid Package { get; set; }
		public Guid Parent { get; set; }
		public InstallStateStatus Status { get; set; }
		public DateTime Modified { get; set; }
		public string Error { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Package = GetGuid("package");
			Parent = GetGuid("parent");
			Status = GetValue("status", InstallStateStatus.Pending);
			Modified = GetDate("modified");
			Error = GetString("error");
		}
	}
}

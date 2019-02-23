using System;
using TomPIT.Data.Sql;
using TomPIT.Deployment;

namespace TomPIT.SysDb.Sql.Deployment
{
	internal class InstallAudit : PrimaryKeyRecord, IInstallAudit
	{
		public Guid Package { get; set; }
		public InstallAuditType Type { get; set; }
		public DateTime Created { get; set; }
		public string Message { get; set; }
		public string Version { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Package = GetGuid("package");
			Type = GetValue("type", InstallAuditType.PendingInstall);
			Created = GetDate("created");
			Message = GetString("message");
			Version = GetString("version");
		}
	}
}

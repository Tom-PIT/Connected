using System;

namespace TomPIT.Deployment
{
	public enum InstallAuditType
	{
		PendingInstall = 1,
		Installing = 2,
		PendingUpgrade = 3,
		Upgrading = 4,
		Complete = 5,
		Error = 6
	}

	public interface IInstallAudit
	{
		Guid Package { get; }
		InstallAuditType Type { get; }
		DateTime Created { get; }
		string Message { get; }
		string Version { get; }
	}
}

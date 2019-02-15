using System;

namespace TomPIT.Deployment
{
	public enum InstallStateStatus
	{
		Pending = 1,
		Installing = 2,
		Error = 3
	}

	public interface IInstallState
	{
		Guid Package { get; }
		Guid Parent { get; }
		InstallStateStatus Status { get; }
	}
}

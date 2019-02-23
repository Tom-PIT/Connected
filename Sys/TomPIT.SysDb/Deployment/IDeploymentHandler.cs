using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.SysDb.Deployment
{
	public interface IDeploymentHandler
	{
		List<IInstallState> QueryInstallers();
		IInstallState SelectInstaller(Guid package);
		void Insert(List<IInstallState> installers);
		void Update(IInstallState state, InstallStateStatus status, string error);
		void Delete(IInstallState state);

		Guid SelectInstallerConfiguration(Guid package);
		void InsertInstallerConfiguration(Guid package, Guid configuration);

		void InsertInstallAudit(InstallAuditType type, Guid package, DateTime created, string message, string version);
		List<IInstallAudit> QueryInstallAudit(Guid package);
		List<IInstallAudit> QueryInstallAudit(DateTime from);
	}
}

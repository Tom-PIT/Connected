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
		void Update(IInstallState state, InstallStateStatus status);
		void Delete(IInstallState state);
	}
}

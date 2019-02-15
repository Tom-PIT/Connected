using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Management.Deployment;

namespace TomPIT.Services
{
	public class InstallerService : HostedService
	{
		public InstallerService()
		{
			IntervalTimeout = TimeSpan.FromSeconds(10);
		}

		protected override Task Process()
		{
			var connections = Shell.GetService<IConnectivityService>().QueryConnections();

			Parallel.ForEach(connections,
				(i) =>
				{
					var connection = Shell.GetService<IConnectivityService>().Select(i.Url);
					var installers = connection.GetService<IDeploymentService>().QueryInstallers();

					if (installers.Count > 0)
						InstallPackage(connection, installers);
				});


			return Task.CompletedTask;
		}

		private void InstallPackage(ISysConnection connection, List<IInstallState> installers)
		{
			var pending = installers.Where(f => f.Status == InstallStateStatus.Pending);

			if (pending.Count() == 0)
				return;

			var installer = pending.FirstOrDefault(f => f.Parent == Guid.Empty);
			var package = connection.GetService<IDeploymentService>().DownloadPackage(installer.Package);
		}
	}
}

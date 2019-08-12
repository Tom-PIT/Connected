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
					ExecuteInstallers(Shell.GetService<IConnectivityService>().Select(i.Url));
				});


			return Task.CompletedTask;
		}

		private void ExecuteInstallers(ISysConnection connection)
		{
			var installers = connection.GetService<IDeploymentService>().QueryInstallers();

			if (installers.Count > 0)
				InstallPackage(connection, installers);
		}

		private void InstallPackage(ISysConnection connection, List<IInstallState> installers)
		{
			var pending = installers.Where(f => f.Status == InstallStateStatus.Pending);

			if (pending.Count() == 0)
				return;

			var roots = pending.Where(f => f.Parent == Guid.Empty);

			if (roots.Count() == 0)
				return;

			foreach (var installer in roots)
			{
				var package = connection.GetService<IDeploymentService>().DownloadPackage(installer.Package);

				connection.GetService<IDeploymentService>().UpdateInstaller(installer.Package, InstallStateStatus.Installing, null);

				try
				{
					connection.GetService<IDeploymentService>().Deploy(installer.Package, package);
					connection.GetService<IDeploymentService>().DeleteInstaller(installer.Package);
				}
				catch (Exception ex)
				{
					connection.GetService<IDeploymentService>().UpdateInstaller(installer.Package, InstallStateStatus.Error, ex.Message);
					connection.LogError(null, "InstallerService", ex.Source, ex.Message);
				}
			}

			ExecuteInstallers(connection);
		}
	}
}

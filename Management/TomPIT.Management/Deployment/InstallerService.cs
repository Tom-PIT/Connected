using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Management.Deployment
{
	public class InstallerService : HostedService
	{
		public InstallerService()
		{
			IntervalTimeout = TimeSpan.FromSeconds(10);
		}

		protected override bool Initialize(CancellationToken cancel)
		{
			return Instance.State == InstanceState.Running;
		}
		protected override Task Process(CancellationToken cancel)
		{
			var tenants = Shell.GetService<IConnectivityService>().QueryTenants();

			Parallel.ForEach(tenants,
				(i) =>
				{
					MiddlewareDescriptor.Current.Tenant = Shell.GetService<IConnectivityService>().SelectTenant(i.Url);

					ExecuteInstallers(cancel, Shell.GetService<IConnectivityService>().SelectTenant(i.Url));
				});


			return Task.CompletedTask;
		}

		private void ExecuteInstallers(CancellationToken cancel, ITenant tenant)
		{
			var installers = tenant.GetService<IDeploymentService>().QueryInstallers();

			if (installers.Count > 0)
				InstallPackage(cancel, tenant, installers);
		}

		private void InstallPackage(CancellationToken cancel, ITenant tenant, List<IInstallState> installers)
		{
			var pending = installers.Where(f => f.Status == InstallStateStatus.Pending);

			if (pending.Count() == 0)
				return;

			var roots = pending.Where(f => f.Parent == Guid.Empty);

			if (roots.Count() == 0)
				return;

			foreach (var installer in roots)
			{
				if (cancel.IsCancellationRequested)
					return;

				var package = tenant.GetService<IDeploymentService>().DownloadPackage(installer.Package);

				if (cancel.IsCancellationRequested)
					return;

				tenant.GetService<IDeploymentService>().UpdateInstaller(installer.Package, InstallStateStatus.Installing, null);

				try
				{
					tenant.GetService<IDeploymentService>().Deploy(installer.Package, package);
					tenant.GetService<IDeploymentService>().DeleteInstaller(installer.Package);
				}
				catch (Exception ex)
				{
					tenant.GetService<IDeploymentService>().UpdateInstaller(installer.Package, InstallStateStatus.Error, ex.Message);
					tenant.LogError(ex.Source, ex.Message, LogCategories.Installer);
				}
			}

			ExecuteInstallers(cancel, tenant);
		}
	}
}

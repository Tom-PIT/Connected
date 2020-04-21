using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Management.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Management.Deployment
{
	public class UpdateService : HostedService
	{
		public UpdateService()
		{
			IntervalTimeout = TimeSpan.FromMinutes(1);
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

					 CheckForUpdates(Shell.GetService<IConnectivityService>().SelectTenant(i.Url), cancel);
				 });


			return Task.CompletedTask;
		}

		private void CheckForUpdates(ITenant tenant, CancellationToken cancel)
		{
			var microServices = tenant.GetService<IMicroServiceService>().Query();
			var checkingMicroServices = new ListItems<IMicroService>();

			foreach (var microService in microServices)
			{
				if (string.IsNullOrWhiteSpace(microService.Version) || microService.UpdateStatus == UpdateStatus.UpdateAvailable)
					continue;

				checkingMicroServices.Add(microService);
			}

			var updates = tenant.GetService<IDeploymentService>().CheckForUpdates(checkingMicroServices);

			if (cancel.IsCancellationRequested)
				return;

			foreach (var update in updates)
			{
				var microService = microServices.FirstOrDefault(f => f.Token == update.MicroService);

				if (microService == null)
					continue;

				tenant.GetService<IMicroServiceManagementService>().Update(update.MicroService, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
					 microService.Package, microService.Plan, UpdateStatus.UpdateAvailable, microService.CommitStatus);
			}
		}
	}
}

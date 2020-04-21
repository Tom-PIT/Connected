using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Designers;
using TomPIT.Middleware;

namespace TomPIT.Management.Deployment
{
	public class PublishService : HostedService
	{
		public PublishService()
		{
			IntervalTimeout = TimeSpan.FromSeconds(30);
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

					 Publish(Shell.GetService<IConnectivityService>().SelectTenant(i.Url));
				 });


			return Task.CompletedTask;
		}

		private void Publish(ITenant tenant)
		{
			var microServices = tenant.GetService<IMicroServiceService>().Query();

			foreach (var microService in microServices)
			{
				if (microService.CommitStatus != CommitStatus.Publishing || microService.Package == Guid.Empty)
					continue;

				var package = tenant.GetService<IDeploymentService>().SelectPackage(microService.Token);

				if (package == null)
				{
					tenant.LogWarning(null, nameof(PublishService), $"{SR.WrnPackageNotFound} ({microService.Name})");

					tenant.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
						 microService.Package, microService.Plan, microService.UpdateStatus, CommitStatus.Invalidated);

					continue;
				}

				try
				{
					var version = Version.Parse(package.MetaData.Version);

					tenant.GetService<IDeploymentService>().CreatePackage(microService.Token, package.MetaData.Plan, package.MetaData.Name, package.MetaData.Title, PackageDesigner.IncrementVersion(version).ToString(),
						 package.MetaData.Description, package.MetaData.Tags,
						 package.MetaData.ProjectUrl, package.MetaData.ImageUrl, package.MetaData.LicenseUrl, package.MetaData.Licenses, package.Configuration.RuntimeConfigurationSupported,
						 package.Configuration.AutoVersioning);

					tenant.GetService<IDeploymentService>().PublishPackage(microService.Token);

					tenant.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
						 microService.Package, microService.Plan, microService.UpdateStatus, CommitStatus.Synchronized);
				}
				catch (Exception ex)
				{
					tenant.LogError(ex.Source, ex.Message, LogCategories.Deployment);

					tenant.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
						 microService.Package, microService.Plan, microService.UpdateStatus, CommitStatus.PublishError);
				}
			}
		}
	}
}

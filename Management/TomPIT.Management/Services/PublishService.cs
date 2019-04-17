using System;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Designers;
using TomPIT.Management.Deployment;
using TomPIT.Services;

namespace TomPIT.Management.Services
{
    public class PublishService : HostedService
    {
        public PublishService()
        {
            IntervalTimeout = TimeSpan.FromSeconds(30);
        }

        protected override Task Process()
        {
            var connections = Shell.GetService<IConnectivityService>().QueryConnections();

            Parallel.ForEach(connections,
                (i) =>
                {
                    Publish(Shell.GetService<IConnectivityService>().Select(i.Url));
                });


            return Task.CompletedTask;
        }

        private void Publish(ISysConnection connection)
        {
            var microServices = connection.GetService<IMicroServiceService>().Query();

            foreach (var microService in microServices)
            {
                if (microService.CommitStatus != CommitStatus.Publishing || microService.Package == Guid.Empty)
                    continue;

                var package = connection.GetService<IDeploymentService>().SelectPackage(microService.Token);

                if (package == null)
                {
                    connection.LogWarning(null, nameof(PublishService), $"{SR.WrnPackageNotFound} ({microService.Name})");

                    connection.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
                        microService.Package, microService.UpdateStatus, CommitStatus.Invalidated);

                    continue;
                }

                try
                {
                    var version = Version.Parse(package.MetaData.Version);

                    connection.GetService<IDeploymentService>().CreatePackage(microService.Token, package.MetaData.Name, package.MetaData.Title, PackageDesigner.IncrementVersion(version).ToString(),
                        package.MetaData.Scope, package.MetaData.Trial, package.MetaData.TrialPeriod, package.MetaData.Description, package.MetaData.Price, package.MetaData.Tags,
                        package.MetaData.ProjectUrl, package.MetaData.ImageUrl, package.MetaData.LicenseUrl, package.MetaData.Licenses, package.Configuration.RuntimeConfigurationSupported,
                        package.Configuration.AutoVersioning);

                    connection.GetService<IDeploymentService>().PublishPackage(microService.Token);

                    connection.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
                        microService.Package, microService.UpdateStatus, CommitStatus.Synchronized);
                }
                catch (Exception ex)
                {
                    connection.LogError(null, microService.Name, nameof(PublishService), ex.Message);

                    connection.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
                        microService.Package, microService.UpdateStatus, CommitStatus.PublishError);
                }
            }
        }
    }
}

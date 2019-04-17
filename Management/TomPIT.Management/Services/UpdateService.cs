using System;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Management.Deployment;
using TomPIT.Services;

namespace TomPIT.Management.Services
{
    public class UpdateService : HostedService
    {
        public UpdateService()
        {
            IntervalTimeout = TimeSpan.FromHours(1);
        }

        protected override Task Process()
        {
            var connections = Shell.GetService<IConnectivityService>().QueryConnections();

            Parallel.ForEach(connections,
                (i) =>
                {
                    CheckForUpdates(Shell.GetService<IConnectivityService>().Select(i.Url));
                });


            return Task.CompletedTask;
        }

        private void CheckForUpdates(ISysConnection connection)
        {
            var microServices = connection.GetService<IMicroServiceService>().Query();
            var checkingMicroServices = new ListItems<IMicroService>();

            foreach (var microService in microServices)
            {
                if (string.IsNullOrWhiteSpace(microService.Version) || microService.UpdateStatus == UpdateStatus.UpdateAvailable)
                    continue;

                checkingMicroServices.Add(microService);
            }

            var updates = connection.GetService<IDeploymentService>().CheckForUpdates(checkingMicroServices);

            foreach (var update in updates)
            {
                var microService = microServices.FirstOrDefault(f => f.Token == update);

                if (microService == null)
                    continue;

                connection.GetService<IMicroServiceManagementService>().Update(update, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
                    microService.Package, UpdateStatus.UpdateAvailable, microService.CommitStatus);
            }
        }
    }
}

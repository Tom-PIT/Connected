using Connected.SaaS.Clients.Authentication;
using Connected.SaaS.Clients.HealthMonitoring;
using Connected.SaaS.Clients.HealthMonitoring.Rest;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.HealthMonitoring
{
    internal class HealthMonitoringHostedService : IHostedService
    {
        public HealthMonitoringHostedService()
        {
            this.Configuration = Shell.GetConfiguration<ISys>().HealthMonitoring;

            if (Configuration is null)
                return;

            var authProvider = new BearerAuthenticationProvider(Configuration.RestToken);

            this.Client = MiddlewareDescriptor.Current.Tenant.GetService<IHealthMonitoringRestClientFactory>().Select(Configuration.EndpointUrl, Configuration.SubscriptionKey, authProvider);

            if (Client is null)
                return;

            this.Endpoint = new Endpoint { Key = Configuration.EndpointKey };
        }

        protected internal HealthMonitoringConfiguration Configuration { get; }

        protected internal HealthMonitoringRestClient Client { get; }

        protected internal Endpoint Endpoint { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            StartSignallingThread(cancellationToken);
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task StartSignallingThread(CancellationToken cancellationToken)
        {
            var enabled = Configuration?.Enabled ?? false;
            enabled &= Client is not null;

            if (cancellationToken.IsCancellationRequested || !enabled)
                return;

            /*
             * Attempt to turn on endpoint if it is not             
             */
            try
            {
                await Client.Requests.EndpointManagement.TurnOn(Endpoint, cancellationToken);
            }
            catch (Exception ex)
            {
                /*
                 * Log and let die, endpoint not available or active
                 */
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Client.Requests.Measurements.Insert(Endpoint, 100, cancellationToken);
                    await Task.Delay(Configuration.HeartbeatInterval, cancellationToken);
                }
                catch (Exception ex)
                {
                    /*
                     * Log and keep trying                     
                     */
                    Console.WriteLine(ex.ToString());
                    await Task.Delay(Configuration.HeartbeatInterval, cancellationToken);
                }
            }
        }
    }
}
